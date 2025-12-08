using Cysharp.Threading.Tasks;

public interface IUserData
{
    //单捞磐 包府
    public void InitData();
    public bool LoadData();
    public bool SaveData();
}

public interface IAsyncUserData
{
    //单捞磐 包府
    public UniTask InitData();
    public UniTask LoadData();
    public UniTask SaveData();
}