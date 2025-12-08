using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int id;
    public int cost;
    public CharacterModelData modelData;
    public string characterName;

    //Todo ½ºÅÝ
}
