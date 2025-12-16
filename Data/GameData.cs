using UnityEngine;

public class GameData : Singleton<GameData>
{
    public int MainStage, SubStage;

    public InGameCharacterData[] characterDatas;

    public Vector3 DefaulteCameraPos;

    public void ResetData()
    {
        DefaulteCameraPos = Vector3.zero;
        MainStage = 0;
        SubStage = 0;
        characterDatas = null;
    }
}
