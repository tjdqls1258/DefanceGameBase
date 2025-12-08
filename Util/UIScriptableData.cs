using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI Scriptable Data
/// UI 레이아웃 정보를 저장하는 ScriptableObject입니다.
/// 에디터에서는 JSON 데이터를 로드/저장하여 데이터 관리를 돕고,
/// 런타임에서는 이 데이터를 기반으로 UI 객체를 비동기적으로 인스턴스화하고 배치하는 역할을 합니다.
/// </summary>
[CreateAssetMenu(fileName = "UIScriptableData", menuName = "Scriptable Objects/UIScriptableData")]
public class UIScriptableData : ScriptableObject
{
    // Note: UIBaseData 클래스는 외부(JSON)에서 UI의 이름, 위치, 앵커, 타입 등을 정의한다고 가정합니다.

    /// <summary>
    /// 로드되거나 에디터에서 관리되는 UI 레이아웃 데이터 목록입니다.
    /// </summary>
    public List<UIBaseData> m_UIDataList;

    // ----------------------------------------------------------------------
    // ## Editor/Data Setting (Unity Editor Only)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부에서 받은 데이터를 ScriptableObject에 설정하고, 에디터에서 즉시 저장합니다.
    /// </summary>
    /// <param name="data">설정할 UIBaseData 목록</param>
    public void SettingData(List<UIBaseData> data)
    {
        m_UIDataList = data;

#if UNITY_EDITOR
        // 에디터에서만 실행: ScriptableObject의 변경 사항을 즉시 디스크에 저장합니다.
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    // ----------------------------------------------------------------------
    // ## Runtime UI Instantiation
    // ----------------------------------------------------------------------

    /// <summary>
    /// ScriptableObject에 저장된 데이터를 기반으로 UI 객체들을 비동기적으로 인스턴스화하고,
    /// 각각의 타입에 맞는 부모 Transform 아래에 배치합니다.
    /// </summary>
    /// <param name="command">Command UI가 배치될 부모 Transform</param>
    /// <param name="mainUI">Main UI가 배치될 부모 Transform</param>
    /// <param name="InGame">InGame UI가 배치될 부모 Transform</param>
    public async UniTask MakeUIList(Transform command, Transform mainUI, Transform InGame)
    {
        var uiData = m_UIDataList;

        List<UniTask> tasks = new();

        foreach (var d in uiData)
        {
            // UI 타입에 따라 인스턴스화 및 배치 작업을 Task 리스트에 추가합니다.
            switch (d.uiType)
            {
                case UIBaseData.UIType.Command:
                    tasks.Add(InstantiateObjectSetting(d, command));
                    break;
                case UIBaseData.UIType.MainUI:
                    tasks.Add(InstantiateObjectSetting(d, mainUI));
                    break;
                case UIBaseData.UIType.InGameUI:
                    tasks.Add(InstantiateObjectSetting(d, InGame));
                    break;
            }
        }

        // 모든 UI 객체 생성 및 배치가 완료될 때까지 대기
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// Addressables를 사용하여 UI 프리팹을 인스턴스화하고, UIBaseData에 정의된 RectTransform 속성을 설정합니다.
    /// </summary>
    public async UniTask InstantiateObjectSetting(UIBaseData data, Transform parent)
    {
        // Addressables를 통해 GameObject를 비동기로 인스턴스화
        var obj = await AddressableManager.Instance.InstantiateObjectAsync(data.dataName, parent);

        if (obj == null) return;

        RectTransform rect = (RectTransform)obj.transform;

        // 1. 이름 설정
        rect.gameObject.name = data.dataName;

        // 2. RectTransform 레이아웃 설정 (UIBaseData에서 필요한 정보를 가져옴)
        rect.anchorMin = data.GetAchorMinMax().min;
        rect.anchorMax = data.GetAchorMinMax().max;
        rect.pivot = data.GetPivot();
        rect.anchoredPosition = data.GetAnchorPos();
        rect.sizeDelta = data.GetSizeDetail();
    }

    // ----------------------------------------------------------------------
    // ## Editor Json Utilities (Unity Editor Only)
    // ----------------------------------------------------------------------

#if UNITY_EDITOR

    /// <summary>
    /// [ContextMenu] ScriptableObject에 저장된 m_UIDataList를 JSON 파일로 직렬화하여 저장합니다.
    /// </summary>
    [ContextMenu("Make Json")]
    public void MakeJson()
    {
        // JSON 직렬화 설정 (가독성을 위해 Indented 포맷 사용)
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        // 데이터 직렬화
        string data = JsonConvert.SerializeObject(m_UIDataList, settings);

        // 저장 경로 설정 (Assets/TextAsset/UIData.json)
        string path = Path.Combine($"{Application.dataPath}/TextAsset/", "UIData.json");

        // 파일 쓰기
        File.WriteAllText(path, data);
        Logger.Log($"[UIScriptableData] JSON data saved to: {path}");
    }

    /// <summary>
    /// [ContextMenu] Addressables에서 JSON 파일을 로드하여 ScriptableObject의 데이터로 역직렬화합니다.
    /// </summary>
    [ContextMenu("Load Json")]
    public void LoadJson()
    {
        // 비동기 로드를 시작하고 결과를 무시합니다. (Editor Context에서 사용)
        LoadJsonUI().Forget();
    }

    /// <summary>
    /// Addressables를 통해 "UIData" TextAsset을 비동기로 로드하고 데이터를 파싱합니다.
    /// </summary>
    public async UniTask LoadJsonUI()
    {
        // Addressables를 통해 TextAsset 로드 및 캐싱
        var data = await AddressableManager.Instance.LoadAssetAndCacheAsync<TextAsset>("UIData");

        if (data == null)
        {
            Logger.LogError("[UIScriptableData] Failed to load UIData from Addressables.");
            return;
        }

        // JSON 역직렬화하여 m_UIDataList 업데이트
        m_UIDataList = JsonConvert.DeserializeObject<List<UIBaseData>>(data.text);

        Logger.Log($"[UIScriptableData] JSON data loaded successfully. Total items: {m_UIDataList?.Count}");
    }
#endif
}