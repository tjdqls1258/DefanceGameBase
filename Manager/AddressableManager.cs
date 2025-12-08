using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

/// <summary>
/// Unity Addressables 시스템을 관리하는 MonoSingleton 클래스입니다.
/// 자원 로드, 인스턴스화, 다운로드 및 해제(Release)를 비동기로 처리하며, 로드된 에셋을 캐싱합니다.
/// </summary>
public class AddressableManager : MonoSingleton<AddressableManager>
{
    // ====== Addressables Caching Dictionaries ======

    // 로드된 Addressables 에셋 객체(프리팹, 텍스처 등)를 저장합니다. Addressables.Release(Object)를 위해 사용됩니다.
    // Key: Addressable Key
    private readonly Dictionary<string, Object> _loadedAssets = new();

    // 특정 Addressable 키로 생성된 모든 인스턴스화된 GameObject를 추적합니다.
    // Key: Addressable Key
    private readonly Dictionary<string, List<GameObject>> _instantiatedGameObjects = new();

    // 캐싱 딕셔너리 접근 시 동시성 문제를 방지하기 위한 Lock 객체입니다. (Addressables 콜백이 메인 스레드가 아닐 수 있으므로 사용)
    private readonly object _lock = new();

    // ====== State ======
    private bool _isInitialized = false;

    /// <summary>
    /// Addressables 시스템 초기화 완료 여부를 나타냅니다.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    public override void Init()
    {
        base.Init();
        // MonoSingleton 초기화. Addressables 시스템 자체의 초기화는 InitAsync에서 비동기로 처리됩니다.
    }

    // ----------------------------------------------------------------------
    // ## 초기화 및 다운로드 (Initialization and Download)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Unity Addressables 시스템을 초기화합니다.
    /// </summary>
    public async UniTask InitAsync()
    {
        if (_isInitialized) return;

        await Addressables.InitializeAsync();
        _isInitialized = true;
    }

    /// <summary>
    /// 지정된 라벨에 연결된 에셋의 다운로드 크기를 확인하고, 필요 시 다운로드를 진행합니다.
    /// </summary>
    /// <param name="labels">다운로드할 에셋 그룹의 라벨 배열</param>
    /// <param name="onDownloading">다운로드 진행 상태 콜백 (DownloadedBytes, TotalBytes)</param>
    public async UniTask DownloadAssetsAsync(string[] labels, Action<long, long> onDownloading = null,
        Action onSuccess = null, Action onFail = null)
    {
        if (!_isInitialized) await InitAsync();

        if (labels == null || labels.Length == 0) return;

        foreach (string label in labels)
        {
            // 1. 다운로드 크기 확인
            AsyncOperationHandle<long> downloadSizeHandle = Addressables.GetDownloadSizeAsync(label);
            await downloadSizeHandle.Task;

            if (downloadSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                onFail?.Invoke();
                Addressables.Release(downloadSizeHandle); // 핸들 해제
                continue;
            }

            long totalBytes = downloadSizeHandle.Result;
            Addressables.Release(downloadSizeHandle); // 사용 완료된 크기 확인 핸들 해제

            if (totalBytes <= 0)
            {
                onSuccess?.Invoke();
                continue;
            }

            // 2. 종속성 다운로드 시작
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(label);

            while (!downloadHandle.IsDone)
            {
                var status = downloadHandle.GetDownloadStatus();
                onDownloading?.Invoke(status.DownloadedBytes, status.TotalBytes);
                await UniTask.Delay(100); // 100ms 간격으로 다운로드 상태를 업데이트
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Logger.LogError($"[AddressableManager] Download failed for label: {label}, Error: {downloadHandle.OperationException}");
                onFail?.Invoke();
            }
        }
    }


    // ----------------------------------------------------------------------
    // ## 에셋 로딩 (Asset Loading - No Instantiation)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 키의 에셋을 메모리에 로드하고 내부 캐시에 저장하여 재사용 및 해제에 대비합니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입 (예: GameObject, Texture2D)</typeparam>
    /// <param name="key">Addressable 키</param>
    public async UniTask<T> LoadAssetAndCacheAsync<T>(string key) where T : Object
    {
        if (_loadedAssets.ContainsKey(key)) return _loadedAssets[key] as T;

        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        await loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            lock (_lock)
            {
                // 성공 시, Addressables.Release()를 위해 로드된 Object 결과 자체를 캐시에 저장합니다.
                _loadedAssets[key] = loadHandle.Result;
            }

            return loadHandle.Result;
        }
        else
        {
            Logger.LogError($"[AddressableManager] Failed to load asset: {key}, Error: {loadHandle.OperationException}");

            return null;
        }
    }

    // ----------------------------------------------------------------------
    // ## 인스턴스화 (Instantiation)
    // ----------------------------------------------------------------------

    /// <summary>
    /// GameObject 프리팹을 인스턴스화하고, 인스턴스 캐싱 및 자동 릴리즈 컴포넌트 설정을 처리합니다.
    /// </summary>
    /// <param name="key">Addressable 키</param>
    /// <param name="parent">인스턴스의 부모 Transform (선택 사항)</param>
    /// <returns>인스턴스화된 GameObject</returns>
    public async UniTask<GameObject> InstantiateObjectAsync(string key, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> instantiateHandle = Addressables.InstantiateAsync(key, parent);
        await instantiateHandle;

        if (instantiateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Logger.LogError($"[AddressableManager] Failed to instantiate object: {key}, Error: {instantiateHandle.OperationException}");
            return null;
        }

        GameObject result = instantiateHandle.Result;

        // 1. 인스턴스 캐싱 (Destroy 시 추적 목록에서 제거하기 위함)
        lock (_lock)
        {
            if (!_instantiatedGameObjects.ContainsKey(key))
                _instantiatedGameObjects.Add(key, new List<GameObject>());

            _instantiatedGameObjects[key].Add(result);
        }

        // 2. 자동 릴리즈 로직 추가: 인스턴스가 파괴될 때 Addressables.ReleaseInstance를 호출하도록 설정
        AutoRelease autorelease = result.GetComponent<AutoRelease>();
        if (autorelease == null)
            autorelease = result.AddComponent<AutoRelease>();

        // 인스턴스 핸들 및 파괴 콜백을 AutoRelease 컴포넌트에 초기화
        autorelease.Init(key, instantiateHandle, HandleInstanceDestroyed);

        return result;
    }

    /// <summary>
    /// InstantiateObjectAsync를 호출하고, 생성된 GameObject에서 특정 컴포넌트를 찾아서 반환합니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="key">Addressable 키</param>
    /// <param name="parent">부모 Transform (선택 사항)</param>
    /// <returns>인스턴스화된 GameObject에서 찾은 컴포넌트 T</returns>
    public async UniTask<T> InstantiateComponentAsync<T>(string key, Transform parent = null) where T : Component
    {
        GameObject obj = await InstantiateObjectAsync(key, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    /// <summary>
    /// 부모 없이 GameObject를 인스턴스화합니다.
    /// </summary>
    public UniTask<GameObject> InstantiateObjectAsync(string key) => InstantiateObjectAsync(key, null);


    // ----------------------------------------------------------------------
    // ## 해제 및 정리 (Release and Cleanup)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 인스턴스화된 GameObject가 Unity의 Destroy로 파괴될 때, AutoRelease에 의해 호출됩니다.
    /// 내부 캐시에서 해당 GameObject를 제거합니다. (Addressables ReleaseInstance는 AutoRelease가 처리)
    /// </summary>
    private void HandleInstanceDestroyed(string key, GameObject obj)
    {
        lock (_lock)
        {
            if (_instantiatedGameObjects.TryGetValue(key, out List<GameObject> list))
            {
                list.Remove(obj);
            }
        }
    }

    /// <summary>
    /// 지정된 키에 해당하는 로드된 에셋(프리팹, 텍스처 등)을 캐시에서 제거하고 Addressables에 해제(Release) 요청합니다.
    /// </summary>
    /// <param name="key">해제할 에셋의 Addressable 키</param>
    public void UnloadAsset(string key)
    {
        Object assetToRelease;
        lock (_lock)
        {
            if (!_loadedAssets.TryGetValue(key, out assetToRelease))
                return;

            _loadedAssets.Remove(key);
        }

        if (assetToRelease != null)
        {
            Addressables.Release(assetToRelease);
        }
    }

    /// <summary>
    /// 현재 캐싱된 모든 로드된 에셋(프리팹, 텍스처 등)을 해제(Release)합니다.
    /// 인스턴스화된 GameObject는 AutoRelease 컴포넌트를 통해 관리되므로, 이 함수는 인스턴스를 파괴하지 않습니다.
    /// </summary>
    public void UnloadAllAssets()
    {
        lock (_lock)
        {
            foreach (var asset in _loadedAssets.Values)
            {
                if (asset != null)
                {
                    Addressables.Release(asset);
                }
            }
            _loadedAssets.Clear();
        }
    }
}