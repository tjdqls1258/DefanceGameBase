using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName="ScriptableObject/MapData")]
[Serializable]
public class MapData : ScriptableObject
{
    [Header("MainStageData"), JsonIgnore]
    [SerializeField] SpriteAtlas m_atlas;

    [SerializeField] private string m_atlasName;
    [Space, Header("MapData")]
    public int m_mainStage = 1;
    public int m_subStage = 1;
    public int m_width = 10;
    public int m_height = 10;

    [Space]
    public TileData[] tileDatas;

    [Space]
    public PathData[] pathDatas;

    [Serializable]
    public enum MapObject
    {
        None = -1,
        Wall,
        Spawn,
        Path,
        EnemySpawnPoint,
        PlayerEndPoint,

        Delete = 999,
    }

    [Serializable]
    public class TileData
    {
        public int x, y;
        public MapObject type;
        public string spriteName;
    }

    [Serializable]
    public class PathData
    {
        public int index;
        public List<SerializeableVector2Int> path = new();
    }

    [Serializable]
    public struct SerializeableVector2Int
    {
        public int x, y;

        public Vector2Int GetVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }

    public void SetImageSetting(SpriteAtlas atlas)
    {
        m_atlas = atlas;
        m_atlasName= atlas.name;
    }

#if UNITY_EDITOR
    private readonly string MAPDATA_PATH = $"Assets/ScriptableObjectData/MapData";
    public void SaveToJson()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        StageData stageData = new();

        var sAssetGuid = AssetDatabase.FindAssets("t:ScriptableObject", new[] { MAPDATA_PATH });
        var sAssetPathList = Array.ConvertAll<string, string>(sAssetGuid, AssetDatabase.GUIDToAssetPath);

        foreach (var sAssetPath in sAssetPathList)
        {
            MapData mapData = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(MapData)) as MapData;
            
            if (mapData != null)
            {
                stageData.AddStageData(mapData.m_mainStage);
            }
        }

        System.IO.File.WriteAllText($"{Application.dataPath}/TextAsset/StageList.json", JsonConvert.SerializeObject(stageData));
        Logger.Log($"{JsonConvert.SerializeObject(stageData)},,  path : {Application.dataPath}/TextAsset/StageList.json");
    }
#endif
}
