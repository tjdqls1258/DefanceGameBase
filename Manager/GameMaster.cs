using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 마스터 (GameMaster)
/// 게임의 초기화 순서(Initialization Pipeline), 주요 관리자 접근, 
/// 기본 리소스 로드 및 유저 데이터 로드를 책임지는 최상위 MonoSingleton 클래스입니다.
/// </summary>
public class GameMaster : MonoSingleton<GameMaster>
{
    // ====== Managers Access (Read-Only Properties) ======

    // 다른 관리자 인스턴스에 접근하기 위한 간결한 속성 정의
    public SoundManager soundManager => SoundManager.Instance;
    public AddressableManager addressableManager => AddressableManager.Instance;
    public SceneLoadManager sceneLoadManager => SceneLoadManager.Instance;
    public UIManager uiManager => UIManager.Instance;
    public PopupManager popupManager => PopupManager.Instance;
    // Todo: UserDataManager와 같은 다른 핵심 관리자도 여기에 추가하면 좋습니다.

    // ----------------------------------------------------------------------
    // ## Initialization Pipeline
    // ----------------------------------------------------------------------

    /// <summary>
    /// GameMaster 초기화 시작점입니다. InitAscy를 비동기로 시작하고 결과를 무시합니다 (Forget).
    /// </summary>
    public override void Init()
    {
        base.Init();
        InitAscy().Forget();
    }

    /// <summary>
    /// 게임의 모든 관리자 초기화, 리소스 로드, 데이터 로드, 씬 로드를 순차적으로 처리하는 비동기 초기화 파이프라인입니다.
    /// </summary>
    private async UniTask InitAscy()
    {
        // 1. Addressables 초기화 및 다운로드 확인
        // 필수 리소스 다운로드 및 초기화가 완료될 때까지 대기합니다.
        await addressableManager.InitAsync();

        // 2. 관리자 초기화 (동기적/빠른 초기화)
        // Addressables 초기화 이후에 UIManager, PopupManager 등이 필요하므로 순서가 중요합니다.
        soundManager.Init();
        sceneLoadManager.Init();
        uiManager.Init();
        popupManager.Init();

        // 3. 핵심 리소스 로드 (Master Canvas 등)
        await LoadBaseResource();

        // 4. 유저 데이터 로드 및 초기 설정
        await AsyncLoadUserData();

        // 5. 초기 UI 레이아웃 설정
        // AutoUIManager가 MasterCanvas 내에서 JSON 기반 UI 배치를 수행합니다.
        await uiManager.AutoUIManager.LoadJsonAsync();

        // 6. 초기 씬 로드
        await sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene);

        // 7. 최종 UI 화면 전환 (HomeScene에 맞는 UI 타입 활성화)
        uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.main, true);
    }

    // ----------------------------------------------------------------------
    // ## Core Resource Loading
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임 시작에 필요한 핵심 리소스(예: MasterCanvas)를 비동기로 로드합니다.
    /// </summary>
    private async UniTask LoadBaseResource()
    {
        List<UniTask> loadingTasks = new();

        // Master Canvas 로드 및 AutoUIManager 초기화
        var masterCanvasLoader = uiManager.LoadMasterCanvasAsync(transform);
        loadingTasks.Add(masterCanvasLoader);

        // 모든 핵심 리소스 로드가 완료될 때까지 대기
        await UniTask.WhenAll(loadingTasks);
    }

    // ----------------------------------------------------------------------
    // ## User Data Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 유저 데이터 관리자 초기화 및 데이터 로드를 비동기로 처리합니다.
    /// 저장된 데이터가 없으면 기본 데이터를 초기화합니다.
    /// </summary>
    private async UniTask AsyncLoadUserData()
    {
        // UserDataManager가 MonoSingleton이 아니라고 가정하고 인스턴스 접근 후 Init() 호출
        UserDataManager.Instance.Init();

        if (UserDataManager.Instance.hasSaveData)
            UserDataManager.Instance.LoadUserData(); // 저장된 데이터 로드 (동기 또는 빠른 비동기)
        else
            UserDataManager.Instance.InitDefaultData(); // 기본 데이터 생성 및 초기화

        // 필요한 경우, 비동기적으로 추가 유저 데이터를 로드합니다.
        await UserDataManager.Instance.AsyncLoadUserData();
    }

    // ----------------------------------------------------------------------
    // ## Game Exit Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임 종료 확인 팝업을 띄우고, 팝업 닫기 이벤트에 ExitGame 액션을 연결합니다.
    /// </summary>
    public async UniTask ExitGamePopup()
    {
        // 팝업 생성 및 표시 (PopupMsg 타입)
        var popup = await PopupManager.Instance.ShowPopup(PopupManager.PopupType.PopupMsg);

        // 팝업이 닫힐 때(예: 확인 버튼 클릭) 실행할 액션 구독
        // closeAction은 UIBase/PopupBase에 정의되어 있어야 합니다.
        popup.closeAction += ExitGame;
    }

    /// <summary>
    /// 실제 게임 종료 로직을 실행합니다. (에디터/빌드 환경에 따라 동작)
    /// </summary>
    private void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        // 에디터에서 실행 중인 경우 플레이 모드를 종료합니다.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}