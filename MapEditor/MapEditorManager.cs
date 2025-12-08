using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;
using static MapData;

/// <summary>
/// 맵 에디터 관리자 (MapEditorManager)
/// Unity 에디터 환경에서 타일 기반 맵의 생성, 로드, 저장 및 시각적 편집을 관리하는 MonoBehaviour입니다.
/// 맵 데이터는 ScriptableObject (MapData)와 JSON 파일로 관리됩니다.
/// </summary>
public class MapEditorManager : MonoBehaviour
{
#if UNITY_EDITOR
    // ====== Constants ======

    /// <summary> MapData ScriptableObject 파일 저장 경로 포맷입니다. </summary>
    private readonly string AssetPathFormat = "Assets/ScriptableObjectData/MapData/{0}.asset";

    // ====== Inspector Settings & References ======

    [Header("Data Settings")]
    [Tooltip("현재 편집 중인 MapData ScriptableObject 인스턴스입니다.")]
    [SerializeField] public MapData m_currentMapData;

    [Tooltip("맵 파일 이름에 사용될 메인 스테이지 번호입니다.")]
    [SerializeField] private int m_mainStage;

    [Tooltip("맵 파일 이름에 사용될 서브 스테이지 번호입니다.")]
    [SerializeField] private int m_subStage;

    [Header("Map Dimensions")]
    [Tooltip("맵의 가로 크기입니다.")]
    [SerializeField] private int m_width;

    [Tooltip("맵의 세로 크기입니다.")]
    [SerializeField] private int m_height;

    [Header("Editor References")]
    [Tooltip("맵 프리뷰를 위해 사용되는 카메라입니다.")]
    [SerializeField] private Camera cam;

    [Tooltip("맵 타일 편집을 위해 인스턴스화되는 기본 프리팹입니다.")]
    [SerializeField] private TileEdtiorBase m_baseEditorTile;
    [Tooltip("패스 프리팹입니다.")]
    [SerializeField] private PathDataObejctMono m_basePathDataObject;

    [Tooltip("타일 스프라이트를 담고 있는 SpriteAtlas입니다.")]
    [Header("Image Set")]
    public SpriteAtlas m_atlas;

    // ====== Internal State & Caches ======

    private MapEditorUI m_ui; // UI 상호 작용을 위한 레퍼런스

    private List<GameObject> m_tileObjects = new(); // 인스턴스화된 타일 오브젝트 목록 (현재는 사용되지 않음)

    /// <summary> 화면에 배치된 타일 오브젝트(TileEdtiorBase)를 위치(Vector2Int)별로 저장하는 딕셔너리입니다. </summary>
    private Dictionary<Vector2Int, TileEdtiorBase> m_tileBase = new();

    /// <summary> 편집 중인 실제 타일 데이터(TileData)를 위치(Vector2Int)별로 저장하는 딕셔너리입니다. </summary>
    private Dictionary<Vector2Int, TileData> m_tileData;

    private Dictionary<int,PathData> m_pathList = new();
    private List<PathDataObejctMono> m_pathDataObjectList = new();

    // ----------------------------------------------------------------------
    // ## UI Integration
    // ----------------------------------------------------------------------

    /// <summary>
    /// 맵 에디터 UI를 설정하여 상호작용할 수 있도록 합니다.
    /// </summary>
    public void SetUI(MapEditorUI ui)
    { m_ui = ui; }

    // ----------------------------------------------------------------------
    // ## Data Loading & Creation
    // ----------------------------------------------------------------------

    /// <summary>
    /// 현재 MapData가 null일 경우, 새로운 MapData를 생성합니다.
    /// (주로 에디터 진입 시 데이터 존재 여부를 확인하는 용도로 사용될 수 있음)
    /// </summary>
    public void UpdateMapData()
    {
        if (m_currentMapData == null)
        {
            CreateMapData();
        }
    }

    /// <summary>
    /// 메인/서브 스테이지 번호에 해당하는 MapData 파일을 로드하거나,
    /// 파일이 없으면 새 MapData를 생성하고 맵 프리뷰를 생성합니다.
    /// </summary>
    public void LoadMapData()
    {
        var filename = $"MapData-{m_mainStage}-{m_subStage}";
        var path = string.Format(AssetPathFormat, filename);

        // AssetDatabase를 사용하여 ScriptableObject 로드 시도
        var load = AssetDatabase.LoadAssetAtPath(path, typeof(MapData));

        if (load == null)
        {
            Debug.Log($"[MapEditor] MapData not found at {path}. Creating new data.");
            CreateMapData();
        }
        else
        {
            m_currentMapData = load as MapData;
            Debug.Log($"[MapEditor] Successfully loaded MapData: {filename}");
        }

        // 로드 또는 생성된 데이터를 기반으로 맵 시각화 (타일 오브젝트 생성)
        CreateMap();
    }

    /// <summary>
    /// 현재의 MapData 인스턴스를 생성하고 초기화합니다.
    /// </summary>
    public void CreateMapData()
    {
        // 1. 현재 편집 중인 TileData를 배열로 변환
        List<TileData> ti = new();
        if (m_tileData != null)
        {
            foreach (var item in m_tileData.Values)
                ti.Add(item);
        }

        // 2. 새로운 MapData 객체 생성 및 속성 설정
        MapData data = ScriptableObject.CreateInstance<MapData>(); // ScriptableObject 인스턴스 생성
        data.m_width = m_width;
        data.m_height = m_height;
        data.m_mainStage = m_mainStage;
        data.m_subStage = m_subStage;
        data.tileDatas = ti.ToArray();
        data.SetImageSetting(m_atlas); // 스프라이트 아틀라스 정보 설정

        List<PathData> pathDatas = new();
        for(int i = 0; i  < m_pathList.Keys.Count; i++)
        {
            pathDatas.Add(m_pathList[i]);
        }
        data.pathDatas = pathDatas.ToArray();
        m_currentMapData = data;

        // 3. AssetDatabase에 ScriptableObject 파일 생성
        var filename = $"MapData-{m_mainStage}-{m_subStage}";
        var path = string.Format(AssetPathFormat, filename);
        AssetDatabase.CreateAsset(m_currentMapData, path);

        Debug.Log($"[MapEditor] New MapData asset created at: {path}");
    }

    // ----------------------------------------------------------------------
    // ## Data Saving
    // ----------------------------------------------------------------------

    /// <summary>
    /// 현재 편집 중인 모든 타일 정보를 MapData ScriptableObject에 저장하고, 
    /// JSON 파일로도 저장합니다. (MapData.SaveToJson() 함수가 있다고 가정)
    /// </summary>
    public void SaveMapData()
    {
        // 1. 현재 편집된 m_tileData를 MapData.tileDatas 배열에 업데이트
        List<TileData> ti = new();
        if (m_tileData != null)
        {
            foreach (var item in m_tileData.Values)
                ti.Add(item);
        }

        m_currentMapData.tileDatas = ti.ToArray();
        m_currentMapData.SetImageSetting(m_atlas);
        List<PathData> pathDatas = new();
        for (int i = 0; i < m_pathList.Keys.Count; i++)
        {
            pathDatas.Add(m_pathList[i]);
        }
        m_currentMapData.pathDatas = pathDatas.ToArray();

        // 2. MapData 내부에 정의된 JSON 저장 함수 호출
        m_currentMapData.SaveToJson();

        // 3. Unity Editor에 ScriptableObject 변경 사항을 기록 및 저장
        EditorUtility.SetDirty(m_currentMapData); // 변경 사항을 SetDirty로 표시
        AssetDatabase.Refresh(); // 에셋 데이터베이스 새로고침 (JSON 파일 생성 등을 반영)
        AssetDatabase.SaveAssets(); // 디스크에 저장

        Debug.Log($"[MapEditor] MapData saved and assets refreshed.");
    }

    // ----------------------------------------------------------------------
    // ## Map Visualization & Editor Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 맵에 배치된 모든 타일 오브젝트를 즉시 파괴하여 초기화합니다.
    /// </summary>
    [ContextMenu("Delete")]
    public void DeleteAll()
    {
        // Transform 자식 오브젝트를 역순으로 순회하며 즉시 파괴 (Editor에서만 사용)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 내부 딕셔너리 캐시 정리
        if (m_tileData != null)
            m_tileData.Clear();
        if (m_tileBase != null)
            m_tileBase.Clear();

        m_tileObjects.Clear();
    }

    /// <summary>
    /// 현재 설정된 Width, Height에 맞게 타일 오브젝트를 생성하고 카메라 위치를 조정합니다.
    /// </summary>
    [ContextMenu("InitMap")]
    public void InitMap()
    {
        // 비동기 함수 이름을 가졌으나 실제로는 동기 코드로 구현되어 있습니다.
        InitMapSync();

        // 카메라 위치를 맵의 중앙으로 이동
        cam.gameObject.transform.position = new Vector3((m_width * 0.5f) - 0.5f, (m_height * 0.5f) - 0.5f, -10);

        Debug.Log($"[MapEditor] Map initialized: {m_width}x{m_height}");
    }

    /// <summary>
    /// 맵 초기화 및 타일 오브젝트를 생성합니다. (InitMap에서 호출)
    /// </summary>
    public void InitMapSync() // 함수 이름 변경 권고: InitMapAsync -> InitMapSync
    {
        DeleteAll(); // 기존 타일 오브젝트 및 데이터 초기화

        m_tileData = new Dictionary<Vector2Int, TileData>(); // 새 TileData 딕셔너리 생성

        // 지정된 크기(Width x Height)만큼 타일 오브젝트를 생성합니다.
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                var obj = Instantiate(m_baseEditorTile, transform); // 타일 프리팹 인스턴스화
                Setting(obj.gameObject, x, y);
            }
        }

        // 로컬 함수: 타일 오브젝트의 초기 속성을 설정하고 딕셔너리에 등록합니다.
        void Setting(GameObject obj, int x, int y)
        {
            // m_tileObjects.Add(obj); // 현재 사용되지 않음

            // 컴포넌트 레퍼런스 및 위치 설정
            var tileEditor = obj.GetComponent<TileEdtiorBase>();
            Vector2Int postition = new Vector2Int(x, y);

            tileEditor.currentPos = postition;
            // 타일을 클릭했을 때 호출될 콜백 함수 설정
            tileEditor.onclickEnter = GetTileData;
            m_tileBase.Add(postition, tileEditor);

            // 맵 좌표계에 맞게 위치 설정
            obj.transform.localPosition = new Vector3(x, y, 0);

            obj.SetActive(true);

            // 생성 시점에 기본 TileData를 딕셔너리에 추가 (아직 내용이 채워지지 않은 상태)
            TileData initialData = new TileData() { x = x, y = y };
            m_tileData.Add(postition, initialData);
        }
    }

    /// <summary>
    /// 로드된 MapData를 기반으로 맵 프리뷰를 생성하고 타일 상태를 복원합니다.
    /// </summary>
    public void CreateMap()
    {
        InitMap(); // 맵 크기에 맞게 모든 기본 타일 오브젝트를 생성합니다.

        // 로드된 TileData를 순회하며 기존 타일 오브젝트의 상태를 복원합니다.
        foreach (var item in m_currentMapData.tileDatas)
        {
            Setting(item);
        }

        int currentIndex = 0;
        foreach(var path in m_currentMapData.pathDatas)
        {
            m_pathList.Add(currentIndex, path);
            currentIndex++;
        }

        // 로컬 함수: 로드된 TileData를 기반으로 화면의 타일 오브젝트(TileEdtiorBase)를 설정합니다.
        void Setting(TileData tileData)
        {
            Vector2Int key = new Vector2Int(tileData.x, tileData.y);

            if (!m_tileBase.ContainsKey(key))
            {
                // 로드된 데이터가 현재 맵 크기(m_width, m_height)를 벗어날 경우를 대비한 방어 로직
                Debug.LogWarning($"[MapEditor] Loaded TileData ({key}) is outside the current map bounds. Skipping.");
                return;
            }

            // 1. 스프라이트 설정
            var sp = m_tileBase[key].gameObject.GetComponent<SpriteRenderer>();
            Sprite sprite = m_atlas.GetSprite(tileData.spriteName);

            if (sprite != null)
                sp.sprite = sprite;
            else if (!string.IsNullOrEmpty(tileData.spriteName))
                Debug.LogWarning($"[MapEditor] Sprite '{tileData.spriteName}' not found in Atlas.");

            // 2. TileEdtiorBase 컴포넌트에 로드된 데이터 전달
            m_tileBase[key].InitTileEdtiorBase(tileData);

            // 3. 편집 데이터 딕셔너리(m_tileData)에 로드된 데이터를 업데이트/추가
            if (m_tileData.ContainsKey(key))
                m_tileData[key] = tileData;
            else
                m_tileData.Add(key, tileData);
        }

        Debug.Log($"[MapEditor] Map preview created from loaded data. Total tiles: {m_tileData.Count}");
    }

    // ----------------------------------------------------------------------
    // ## Editor Interaction
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일 클릭 시 (TileEdtiorBase의 onclickEnter 콜백) 호출되어 
    /// 선택된 스프라이트 이름과 타입을 해당 타일 위치에 저장합니다.
    /// </summary>
    /// <param name="key">타일의 Vector2Int 좌표</param>
    public void GetTileData(Vector2Int key)
    {
        if (m_ui.pathMode)
        {
            if (m_ui.pathRemoveMode && m_pathList[m_ui.pathIndex].path.Any(x=>x.GetVector2Int() == key))
            {
                m_pathList[m_ui.pathIndex].path.RemoveAll(x=> x.GetVector2Int() == key);
                if (m_pathDataObjectList.Any(x => x.PathPos == key))
                {
                    var data = m_pathDataObjectList.Find(x => x.PathPos == key);
                    data.gameObject.SetActive(false);
                }
                for (int i = 0; i < m_pathDataObjectList.Count; i++)
                {
                    if (m_pathDataObjectList.Count <= i) break;
                    m_pathDataObjectList[i].SetIndex(i);
                }
                return;
            }
            else if (m_ui.pathRemoveMode == false)
            {
                if (m_pathDataObjectList.Count > m_pathList[m_ui.pathIndex].path.Count)
                {
                    m_pathDataObjectList[m_pathList[m_ui.pathIndex].path.Count].gameObject.SetActive(true);
                    m_pathDataObjectList[m_pathList[m_ui.pathIndex].path.Count].SetPathData(m_pathList[m_ui.pathIndex].path.Count, key);
                }
                else
                {
                    var pathObject = Instantiate(m_basePathDataObject, position: new(key.x, key.y, 0), Quaternion.identity);
                    pathObject.SetPathData(m_pathDataObjectList.Count, key);
                    m_pathDataObjectList.Add(pathObject);
                }

                m_pathList[m_ui.pathIndex].path.Add(new() { x = key.x, y = key.y});
            }
            return;
        }

        // 1. 해당 위치에 TileData가 없으면 새로 생성하여 딕셔너리에 추가
        if (m_tileData.ContainsKey(key) == false)
        {
            var tile = new TileData() { x = key.x, y = key.y };
            m_tileData.Add(key, tile);
        }

        // 2. 현재 UI에서 선택된 스프라이트 정보와 타일 타입을 데이터에 반영
        m_tileData[key].spriteName = m_ui.GetCurrentSpriteName();
        m_tileData[key].type = m_ui.GetCurrentType();
    }

    public void RemovePathData(int pathData)
    {
        if (m_pathList.ContainsKey(pathData) == false) return;

        m_pathList.Remove(pathData);
        PathModeOn(System.Math.Max(pathData - 1, 0));
    }

    public void PathModeOff()
    {
        foreach(var pathData in m_pathDataObjectList)
        {
            pathData.gameObject.SetActive(false);
        }
    }

    public void PathModeOn(int pathIndex)
    {
        PathModeOff();
        if(m_pathList.ContainsKey(pathIndex) == false)
        {
            m_pathList.Add(pathIndex, new() { index = pathIndex });
        }
        for(int i = 0; i < m_pathList[pathIndex].path.Count; i++)
        {
            if(m_pathDataObjectList.Count > i)
            {
                m_pathDataObjectList[i].SetPathData(i, new() { x = m_pathList[pathIndex].path[i].x, y = m_pathList[pathIndex].path[i].y });
            }
            else
            {
                var obj = Instantiate(m_basePathDataObject);
                obj.SetPathData(i, new() { x = m_pathList[pathIndex].path[i].x, y = m_pathList[pathIndex].path[i].y });
            }
        }
    }
    
#endif
}