using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static MapData;

/// <summary>
/// 맵 에디터 UI 관리자 (MapEditorUI)
/// 맵 에디터 화면의 사용자 인터페이스(타일 팔레트, 타입 설정 등)를 관리하고
/// 사용자의 입력을 받아 MapEditorManager로 전달하는 역할을 수행합니다.
/// </summary>
public class MapEditorUI : MonoBehaviour
{
    // ====== Inspector References ======

    [Header("Manager Link")]
    [Tooltip("핵심 로직 및 데이터 관리를 담당하는 MapEditorManager입니다.")]
    [SerializeField] private MapEditorManager m_manager;

    [Header("UI Elements")]
    [Tooltip("타일 버튼들이 배치될 UI Context (RectTransform)입니다. (타일 팔레트)")]
    [SerializeField] private RectTransform m_context;

    [Tooltip("타일 팔레트에 사용할 버튼 프리팹입니다. (TileItemButton 스크립트 필요)")]
    [SerializeField] private TileItemButton m_tileItme;

    [Tooltip("현재 선택된 맵 오브젝트 타입(예: Wall, Floor)을 표시하는 텍스트입니다.")]
    [SerializeField] private Text m_currentTypeText;

    [Tooltip("Path 모드일때만 활성화 되는 UI입니다.")]
    [SerializeField] private GameObject m_pathModeObject;

    // ====== Runtime State & Caches ======

    private SpriteAtlas m_atlas; // MapEditorManager로부터 전달받는 SpriteAtlas 캐시

    /// <summary> 현재 사용자가 선택한 타일의 스프라이트 이름입니다. </summary>
    private string m_currentSpriteName = string.Empty;

    /// <summary> 현재 사용자가 선택한 맵 오브젝트의 타입입니다. (기본값: Wall) </summary>
    private MapObject m_currentObjectType = MapObject.Wall;

    // 선택 강조 기능 추가 필드
    /// <summary> 타일 팔레트에서 이전에 선택되어 강조되었던 버튼 인스턴스를 추적합니다. </summary>
    private TileItemButton m_selectedTileButton = null;

    /// <summary>
    /// Path 모드 관련 Flag
    public bool pathMode 
    {
        get;
        private set;
    } = false;

    public int pathIndex
    {
        get;
        private set;
    } = 0;

    public bool pathRemoveMode
    {
        get;
        private set;
    } = false;

    
    // ----------------------------------------------------------------------
    // ## Initialization & Lifecycle
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // Manager에 이 UI 인스턴스를 등록하여 Manager -> UI 통신이 가능하도록 설정
        m_manager.SetUI(this);
        // 초기 맵 타입을 화면에 표시
        m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
    }

    // ----------------------------------------------------------------------
    // ## Manager Actions (UI 버튼 연결)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 맵 초기화 버튼에 연결되어, 맵 에디터를 재설정합니다.
    /// </summary>
    public void Init()
    {
        m_manager.InitMap();
    }

    /// <summary>
    /// 맵 초기화/삭제 버튼에 연결되어, 맵의 모든 타일을 제거합니다.
    /// </summary>
    public void Clear()
    {
        m_manager.DeleteAll();
    }

    /// <summary>
    /// 맵 데이터 로드 및 타일 팔레트를 생성/업데이트합니다.
    /// (Load 버튼에 연결)
    /// </summary>
    [ContextMenu("TestCreate")]
    public void Load()
    {
        // 1. MapManager를 통해 MapData를 로드하거나 생성합니다.
        m_manager.LoadMapData();

        // 2. Manager로부터 SpriteAtlas를 가져와 캐시합니다.
        m_atlas = m_manager.m_atlas;
        if (m_atlas == null)
        {
            Debug.LogError("[MapEditorUI] SpriteAtlas is not assigned in MapEditorManager.");
            return;
        }

        // 3. 아틀라스의 모든 스프라이트를 가져옵니다.
        Sprite[] spList = new Sprite[m_atlas.spriteCount];
        m_atlas.GetSprites(spList);

        // 4. 타일 팔레트 UI를 생성하거나 업데이트합니다.
        for (int i = 0; i < m_atlas.spriteCount; i++)
        {
            TileItemButton currentButton;

            // 기존 오브젝트 재사용 (Pool-like)
            if (m_context.childCount > i)
            {
                currentButton = m_context.GetChild(i).GetComponent<TileItemButton>();
                // 기존 버튼을 찾아 이미지와 액션을 업데이트
                currentButton.SetImage(spList[i], OnClickAction);
                currentButton.gameObject.SetActive(true); // 혹시 비활성화되어 있었다면 활성화
            }
            // 새 오브젝트 인스턴스화
            else
            {
                currentButton = Instantiate(m_tileItme, m_context);
                currentButton.SetImage(spList[i], OnClickAction);
                currentButton.gameObject.SetActive(true);
            }
        }

        // 5. 스프라이트 개수보다 많은 기존 오브젝트는 비활성화하여 재활용 풀에 둡니다.
        for (int i = m_atlas.spriteCount; i < m_context.childCount; i++)
        {
            m_context.GetChild(i).gameObject.SetActive(false);
        }

        // 로드 후, 기본값으로 첫 번째 타일을 선택하거나 이전에 저장된 currentSpriteName을 선택할 수 있습니다.
    }

    /// <summary>
    /// 맵 데이터 저장 버튼에 연결되어, 현재 편집 상태를 저장합니다.
    /// </summary>
    public void Save()
    {
        m_manager.SaveMapData();
    }

    // ----------------------------------------------------------------------
    // ## Getters & Setters
    // ----------------------------------------------------------------------

    /// <summary>
    /// 현재 선택된 타일의 스프라이트 인스턴스를 반환합니다.
    /// </summary>
    public Sprite GetCurrentSprite()
    {
        if (m_atlas == null) return null;
        return m_atlas.GetSprite(m_currentSpriteName);
    }

    /// <summary>
    /// 현재 선택된 타일의 스프라이트 이름을 MapEditorManager에 제공합니다.
    /// </summary>
    public string GetCurrentSpriteName() => m_currentSpriteName;

    /// <summary>
    /// 현재 선택된 맵 오브젝트 타입(MapObject)을 MapEditorManager에 제공합니다.
    /// </summary>
    public MapObject GetCurrentType() => m_currentObjectType;

    /// <summary>
    /// 드롭다운 또는 버튼을 통해 맵 오브젝트 타입을 설정합니다.
    /// </summary>
    /// <param name="type">선택된 MapObject의 정수 값 (Dropdown.value 등)</param>
    public void SetCurrentType(int type)
    {
        m_currentObjectType = (MapObject)type;

        pathMode = false;
        m_manager.PathModeOff();
        // UI 텍스트 업데이트
        m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
    }

    /// <summary>
    /// 몬스터의 이동 경로를 설정 모드 On/Off 버튼입니다.
    /// </summary>
    public void SetPathButton()
    {
        pathMode = !pathMode;

        m_pathModeObject.SetActive(pathMode);
        if (pathMode)
        {
            m_currentTypeText.text = $"Current Type : PathMode PATH_{pathIndex}";
            m_manager.PathModeOn(pathIndex);
        }
        else
        {
            m_currentTypeText.text = $"Current Type : {m_currentObjectType}";
            m_manager.PathModeOff();
        }
    }

    public void AddPathCount(int count)
    {
        pathIndex = Math.Max(pathIndex + count, 0);
        m_currentTypeText.text = $"Current Type : PathMode PATH_{pathIndex}";
        m_manager.PathModeOn(pathIndex);
    }

    public void RemovePath()
    {
        m_manager.RemovePathData(pathIndex);
    }

    public void PathRemoevMode()
    {
        pathRemoveMode = !pathRemoveMode;
    }

    // ----------------------------------------------------------------------
    // ## UI Callbacks
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일 팔레트의 버튼 클릭 시 호출되는 콜백 함수입니다.
    /// </summary>
    /// <param name="spriteName">클릭된 타일 버튼이 가진 스프라이트의 이름입니다.</param>
    private void OnClickAction(string spriteName, TileItemButton tileItemButton)
    {
        // 1. 현재 선택된 스프라이트 이름 업데이트
        m_currentSpriteName = spriteName;

        // 2. 타일 버튼 강조 로직        
        // Note: 성능 최적화를 위해 context.GetChild(i).GetComponent<TileItemButton>()을 캐싱할 수 있습니다.

        // 이전 선택된 버튼 강조 해제
        if (m_selectedTileButton != null)
        {
            m_selectedTileButton.SetHighlight(false); // 강조 해제
        }

        // 새로 선택된 버튼을 찾아 강조 설정
        tileItemButton.SetHighlight(true); // 강조 설정
        m_selectedTileButton = tileItemButton; // 현재 선택된 버튼 저장
    }
}