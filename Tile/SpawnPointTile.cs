using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class SpawnPointTile : TileBase
{
    public int sapwnIndex = 0;
    private List<EnemySpawnData> m_enemeyData = new();
    private bool m_enemyDataSetDone = false;
    private CancellationTokenSource m_stopToken = new CancellationTokenSource();
    private float currentSpawnTime = 0;
    public void SetEnemyData(List<EnemySpawnData> enemeyDatas)
    {
        m_enemeyData.Clear();
        m_enemeyData.AddRange(enemeyDatas.FindAll((x) => x.pathIndex == sapwnIndex));

        m_enemyDataSetDone = true;
        currentSpawnTime = 0;
    }

    public void StartSpawn()
    {
        SpawnLoop().Forget();
    }

    private async UniTask SpawnLoop()
    {
        foreach(var item in m_enemeyData)
        {
            currentSpawnTime = item.spawnTime - currentSpawnTime;
            await UniTask.WaitForSeconds(currentSpawnTime, cancellationToken:m_stopToken.Token);
            //DoSapwn
        }
    }

    public void FailStageOrClearStage()
    {
        m_stopToken?.Cancel();
        m_stopToken?.Dispose();
    }
}
