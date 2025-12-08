using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AutoRelease : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> handle;

    Action<string, GameObject> onDestroyAction;
    string Key;

    public void Init(string key, AsyncOperationHandle<GameObject> handle, Action<string, GameObject> action)
    {
        this.Key = key;
        this.handle = handle;

        onDestroyAction = action;
    }

    private void OnDestroy()
    {
        if (onDestroyAction != null)
            onDestroyAction.Invoke(Key, gameObject);

        if (handle.IsValid())
            Addressables.Release(handle);
    }
}
