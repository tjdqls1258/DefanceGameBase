using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PlayerPrefasHelper
{
    public enum PrefabsKey
    {
        HasSettingData,
        UserSettingOption,
    }

    #region Use PrefabsKey
    private static readonly Dictionary<PrefabsKey, string> keyValueDic = 
        new Dictionary<PrefabsKey, string> 
        {
            { PrefabsKey.HasSettingData, "HAS_SAVEDATA" },
            { PrefabsKey.UserSettingOption, "USERSETTING_OPTION" },
        };

    public static bool SetString(PrefabsKey key, string value = "")
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return false;
        }

        PlayerPrefs.SetString(keyValueDic[key], value);
        PlayerPrefs.Save();
        return true;
    }

    public static string GetString(PrefabsKey key, string defaultValue = "")
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return string.Empty;
        }

        return PlayerPrefs.GetString(keyValueDic[key], defaultValue);
    }

    public static bool SetFloat(PrefabsKey key, float value)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return false;
        } 

        PlayerPrefs.SetFloat(keyValueDic[key], value);
        PlayerPrefs.Save();
        return true;
    }

    public static float GetFloat(PrefabsKey key, float defaultValue = 0f)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return 0;
        }

        return PlayerPrefs.GetFloat(keyValueDic[key], defaultValue);
    }

    public static bool SetInt(PrefabsKey key, int value)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return false;
        }
        PlayerPrefs.SetInt(keyValueDic[key], value);
        PlayerPrefs.Save();
        return true;
    }

    public static int GetInt(PrefabsKey key, int defaultValue = 0)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"Not Has {key}");
            return 0;
        }

        return PlayerPrefs.GetInt(keyValueDic[key], defaultValue);
    }
    #endregion

    #region Default
    public static void SetString(string key, string value = "")
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    { 
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    #endregion
}
