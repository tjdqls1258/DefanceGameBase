using System;
using UnityEngine;

[Serializable]
public class EnemeyData
{
    public int ID;
    public string modleName;
#if UNITY_EDITOR
    public EnemyController TestObject;
#endif
}

[Serializable]
public class EnemySpawnData
{
    public int pathIndex;
    public EnemeyData enemyData;
    public float spawnTime;
}
