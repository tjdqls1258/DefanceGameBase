using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerData : Singleton<PlayerData>
{
    public void JoinGame() //
    {
        ResetData();
    }

    public void ExitGame()
    {
        ResetData();
    }

    private void ResetData()
    {
        cost = 0;
    }

    public uint cost { private set; get; } = 0;


    public bool UseCost(uint cost)
    {
        if (this.cost < cost)
            return false;

        this.cost -= cost;
        return true;
    }

    public bool CheckCost(uint cost)
    {
        return this.cost >= cost;
    }

    async UniTask UpdateCost()
    {
        await UniTask.WaitForSeconds(1);
        cost++;
    }
}
