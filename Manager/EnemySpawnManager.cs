using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private List<EnemySpawnData> m_enemySpawnDatas;
    private CancellationTokenSource m_cancellationTokenSource = new();
    private MapData.PathData[] m_pathData;
    private int m_spawnCount = 0;
    private float m_currentTime = 0;

    private Dictionary<int, List<EnemyController>> m_enemyList = new();
    private Dictionary<int, List<EnemyController>> m_disableList = new();

    public void SetEnemyData(List<EnemySpawnData> data, MapData.PathData[] pathDatas)
    {
        m_enemySpawnDatas = data;
        m_pathData = pathDatas;
    }

    public void StartSpawn()
    {
        SpawnStart().Forget();
    }

    private async UniTask SpawnStart()
    {
        await UniTask.WaitForSeconds(3f);
        while (m_cancellationTokenSource.IsCancellationRequested == false)
        {
            await UniTask.WaitForFixedUpdate();
            m_currentTime += Time.fixedDeltaTime;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if(m_spawnCount >= m_enemySpawnDatas.Count)
        {
            m_cancellationTokenSource.Cancel();
            return;
        }
        if (m_currentTime >= m_enemySpawnDatas[m_spawnCount].spawnTime)
        {
            //현재는 테스트용 추후 Enemy 모델 데이터를 받아서 로드후 저장
            EnemyController obj;
            int id = m_enemySpawnDatas[m_spawnCount].enemyData.ID;
            if (m_disableList.ContainsKey(id))
            {
                obj = m_disableList[id].First();
                m_disableList[id].Remove(obj);
            }
            else
            {
                obj = Instantiate(m_enemySpawnDatas[m_spawnCount].enemyData.TestObject);
                if (m_enemyList.ContainsKey(id) == false)
                {
                    m_enemyList.Add(id, new());
                }
                m_enemyList[id].Add(obj);
            }

            var pathindex = m_enemySpawnDatas[m_spawnCount].pathIndex;
            var pathData = m_pathData.FirstOrDefault(x => x.index == pathindex);

            if (pathData != null)
            {
                var vectorList = GameUtil.ConvartSerializableVector2IntToVector2Int_List(pathData.path);
                obj.InitEnemyData(vectorList, DieAction);
            }
            else
            {
                var vectorList = GameUtil.ConvartSerializableVector2IntToVector2Int_List(m_pathData[0].path);
                obj.InitEnemyData(vectorList, DieAction);
            }

            m_spawnCount++;
        }
    }

    private void DieAction(int id, EnemyController enemy)
    {
        m_enemyList[id].Remove(enemy);

        if(m_disableList.ContainsKey(id) == false)
        {
            m_disableList.Add(id, new());
        }
        m_disableList[id].Add(enemy);
    }
}
