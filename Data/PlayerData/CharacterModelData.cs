using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "CharacterModelData", menuName = "Scriptable Objects/CharacterModelData")]
public class CharacterModelData : ScriptableObject
{
    public string modelObjectName;
}
