using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int id;
    public int cost;
    public CharacterModelData modelData;
    public string characterName;
    public Sprite characterSprite;
    //Todo ½ºÅÝ
}

public class InGameCharacterData
{
    public InGameCharacterData(CharacterData data)
    {
        characterData = data;
        upgradeCount = 0;
    }

    public CharacterData characterData;
    public int upgradeCount;
}
