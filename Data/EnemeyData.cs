using System;
using UnityEngine;

[Serializable]
public class EnemeyData
{
    public int ID;
    public string modleName;
    public CharacterState characterState;
    public EnemyController TestObject;
}

[Serializable]
public class EnemySpawnData
{
    public int pathIndex;
    public EnemeyData enemyData;
    public float spawnTime;
}

[Serializable]
public class CharacterState
{
    public float maxHp = 1;
    public int atkPower;
    public int defPower;
    public int atkSpeed;
}

[Serializable]
public class MpCharacterState : CharacterState
{
    public float maxMp = 1 ;
}