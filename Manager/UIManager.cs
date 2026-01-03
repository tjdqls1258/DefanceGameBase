using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 패널 중앙 관리 및 풀링 시스템
/// 게임의 개별 UI 패널(Panel)들을 로드, 표시 및 풀링을 통해 관리하는 중앙 관리자 Singleton 클래스입니다.
/// AutoUIManager를 사용하여 캔버스 계층 구조를 제어하며, UI를 'Open Pool'과 'Close Pool (비활성 보관소)'로 나누어 재활용합니다.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    // ====== Constants and Enums ======

    // Addressable 경로 포맷: "UIPanel/{0}.prefab"
    private const string UIPATH_FORMAT = "UIPanel/{0}.prefab";

    /// <summary>
    /// UIManager가 관리하는 개별 UI 패널의 타입 목록입니다.
    /// </summary>
    public enum UISequence
    {
        None = -1,
        StageSeletePanel, // 예시: 스테이지 선택 패널
        CharacterListPanel,
        ShopPanel
        // Todo: 다른 UI 패널 타입 추가 예정
    }

    // ====== Cached Managers and References ======

    /// <summary>
    /// Master Canvas GameObject의 Transform (UI의 최상위 부모).
    /// </summary>
    public Transform MasterCanvas { private set; get; }

    /// <summary>
    /// Canvas Group 전환 및 Reparenting을 담당하는 AutoUIManager 컴포넌트입니다.
    /// </summary>
    public AutoUIManager AutoUIManager { private set; get; }

    // ====== UI Object Pooling Dictionaries ======

    /// <summary>
    /// 현재 화면에 **표시 중이거나 활성화된** 상태의 UI 패널 객체 풀입니다. (**Open Pool**)
    /// </summary>
    public readonly Dictionary<UISequence, GameObject> m_openUIPool = new();

    /// <summary>
    /// 현재 **비활성화되어 보관된** 상태의 UI 패널 객체 풀입니다. (**Close Pool**).
    /// 이 객체들은 AutoUIManager의 ClosetCanvas (비활성 보관소)에 Reparenting되어 있습니다.
    /// </summary>
    public readonly Dictionary<UISequence, GameObject> m_closeUIPool = new();

    public override void Init()
    {
        base.Init();
    }

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// Addressables를 사용하여 MasterCanvas를 로드하고, AutoUIManager를 초기화합니다.
    /// </summary>
    /// <param name="parent">MasterCanvas를 인스턴스화할 부모 Transform</param>
    public async UniTask LoadMasterCanvasAsync(Transform parent)
    {
        // "MasterCanvas" 프리팹을 로드 및 인스턴스화
        GameObject masterCanvasObj = await AddressableManager.Instance.InstantiateObjectAsync("MasterCanvas", parent);
        if (masterCanvasObj == null)
        {
            Logger.LogError("[UIManager] Failed to load MasterCanvas.");
            return;
        }

        MasterCanvas = masterCanvasObj.transform;
        AutoUIManager = MasterCanvas.GetComponent<AutoUIManager>();

        if (AutoUIManager == null)
        {
            Logger.LogError("[UIManager] MasterCanvas is missing AutoUIManager component.");
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 타입의 UI 패널을 화면에 표시합니다. (풀링/로드 후 활성화)
    /// </summary>
    /// <param name="type">표시할 UI 패널 타입 (UISequence)</param>
    /// <param name="uiType">UI 패널이 배치될 AutoUIManager의 부모 캔버스 타입</param>
    public async UniTask ShowUI(UISequence type, UIBaseData.UIType uiType = UIBaseData.UIType.MainUI)
    {
        // 1. Close Pool에서 재사용 가능한지 확인
        if (m_closeUIPool.ContainsKey(type))
        {
            // 재사용: 비활성 보관소에서 꺼내와 지정된 캔버스에 배치
            GameObject ui = m_closeUIPool[type];
            AutoUIManager.PushUI(uiType, ui.transform); // Reparenting (활성화된 캔버스 그룹으로 이동)
            ChangeItem(m_closeUIPool, m_openUIPool, type); // 풀 이동: Close -> Open
        }
        // 2. 새로운 UI 객체 로드
        else
        {
            Logger.Log($"Loading UI panel: {type.ToString()}");

            string path = string.Format(UIPATH_FORMAT, type.ToString());

            // Addressables를 통해 UI 프리팹을 로드 및 인스턴스화 (AutoUIManager의 부모 아래에 배치)
            GameObject ui = await AddressableManager.Instance.InstantiateObjectAsync(path, AutoUIManager.GetParent(uiType));

            if (ui == null)
            {
                Logger.LogError($"[UIManager] Failed to instantiate UI panel at: {path}");
                return;
            }

            m_openUIPool.Add(type, ui); // Open Pool에 추가
        }

        // 3. UI 활성화 로직 호출
        m_openUIPool[type].GetComponent<UIBase>()?.ShowUI();
    }

    /// <summary>
    /// 지정된 타입의 UI 패널을 닫고 비활성화 풀(Close Pool)로 이동시킵니다.
    /// </summary>
    /// <param name="type">닫을 UI 패널 타입 (UISequence)</param>
    public void CloseUI(UISequence type)
    {
        if (m_openUIPool.ContainsKey(type))
        {
            GameObject ui = m_openUIPool[type];

            // 비활성 보관소 캔버스로 이동 (Reparenting)
            AutoUIManager.PopUI(ui.transform);

            // 풀 이동: Open -> Close
            ChangeItem(m_openUIPool, m_closeUIPool, type);
        }
        else
        {
            Logger.LogError($"{GetType().Name}::Cannot find UI panel in Open Pool: {type}");
        }
    }

    // ----------------------------------------------------------------------
    // ## Helper Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// Dictionary 간에 특정 항목을 이동시키는 헬퍼 함수입니다. (풀링 관리)
    /// </summary>
    /// <param name="where">현재 항목이 있는 Dictionary (제거 대상)</param>
    /// <param name="from">항목을 추가할 Dictionary (추가 대상)</param>
    /// <param name="Item">이동시킬 항목의 키</param>
    private void ChangeItem(Dictionary<UISequence, GameObject> where, Dictionary<UISequence, GameObject> from, UISequence Item)
    {
        // 1. 'from' (목적지 풀)에 항목 추가
        from.Add(Item, where[Item]);

        // 2. 'where' (출발지 풀)에서 항목 제거
        where.Remove(Item);
    }
}