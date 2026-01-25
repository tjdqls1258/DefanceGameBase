using UnityEditor;
using UnityEngine;

public class CacheOpenner
{
    [MenuItem("Adressable/ClearCache")]
    public static void ClearCache()
    {
        Caching.ClearCache();
    }
}
