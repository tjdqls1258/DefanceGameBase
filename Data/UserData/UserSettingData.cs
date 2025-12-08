using Newtonsoft.Json;
using System;

public class UserSettingData : Singleton<UserSettingData>, IUserData
{
    [Serializable]
    public struct UserSettingOption
    {
        public bool masterSound;
        public float masterSoundValue;

        public bool effectSound;
        public float effectSoundValue;

        public bool bgmSound;
        public float bgmSoundValue;

        public void SetDefault()
        {
            masterSound = true;
            effectSound = true;
            bgmSound = true;

            masterSoundValue = 0.5f;
            effectSoundValue = 0.5f;
            bgmSoundValue = 0.5f;
        }
    }

    public UserSettingOption userSettingOption;
    public void InitData()
    {
        Logger.Log($"{GetType()}::Set Init");

        userSettingOption = new();
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");
        try
        {
            var getDataJson = PlayerPrefasHelper.GetString(PlayerPrefasHelper.PrefabsKey.UserSettingOption, string.Empty);

            if (string.Empty == getDataJson) 
            {
                userSettingOption = new();
                userSettingOption.SetDefault();
                SaveData();
                return true;
            }

            userSettingOption = JsonConvert.DeserializeObject<UserSettingOption>(getDataJson);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Load Error : {e.ToString()}");
            return false;
        }
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");
        try
        {
            var data = JsonConvert.SerializeObject(userSettingOption);
            PlayerPrefasHelper.SetString(PlayerPrefasHelper.PrefabsKey.UserSettingOption, data);
            return true;
        }
        catch (Exception e) 
        {
            Logger.LogError($"Save Error : {e.ToString()}");

            return false;
        }

    }
}
