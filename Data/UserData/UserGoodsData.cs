using Cysharp.Threading.Tasks;
using System;

public class UserGoodsData : IAsyncUserData
{
    public UniTask InitData()
    {
        Logger.Log($"{GetType()}::Set Init");

        //ToDO 웹 통신
       
        return UniTask.CompletedTask;
    }

    public UniTask LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");

        try
        {
            //TODO 서버
        }
        catch (Exception e)
        {
            Logger.LogError($"Save Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }

    public UniTask SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");

        try
        {
            //TODO 서버
        }
        catch (Exception e)
        {
            Logger.LogError($"Save Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }
}
