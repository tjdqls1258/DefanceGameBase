using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    //Key (오브젝트 네임, 어드레서블 주소 혹은 이름 등으로 사용)
    private Dictionary<string, GameObject> m_poolBase = new();
    private Dictionary<string, List<GameObject>> m_activePool = new(); //활성화된 오브젝트 풀 
    private Dictionary<string, List<GameObject>> m_disablePool = new(); //비활성화된 오브젝트 풀

    public bool CheckAddKey(string key)
    {
        return m_poolBase.ContainsKey(key);
    }

    public void AddKey(string key)
    {
        if (CheckAddKey(key))
            return;

        m_poolBase.Add(key, null);
    }

    public void SetPoolObject(string key, GameObject target)
    {
        if (m_poolBase.ContainsKey(key) == false)
            return;

        var pool = target.AddComponent<PoolObejct>();
        pool.key = key;

        m_activePool.Add(key, new());
        m_disablePool.Add(key, new() { pool.gameObject });
        m_poolBase[key] = pool.gameObject;
        pool.gameObject.SetActive(false);
    }

    public GameObject AddPoolObject(string key, Transform parent = null)
    {
        if (m_disablePool.ContainsKey(key) == false)
        {
            return null;
        }

        GameObject result;
        if (m_disablePool[key].Count > 0)
        {
            result = m_disablePool[key].First();
            m_disablePool[key].Remove(result);
        }
        else
            result = Instantiate(m_poolBase[key].gameObject, parent);

        m_activePool[key].Add(result);
        if(parent != null)
            result.transform.parent = parent;
        result.gameObject.SetActive(true);

        return result;
    }

    public void DisablePool(string key, GameObject target)
    {
        if (m_disablePool.ContainsKey(key))
        {
            m_disablePool[key].Add(target);
            if (m_activePool.ContainsKey(key) == false)
            {
                Logger.LogError("Not Has Key"); 
                return;
            }

            m_activePool[key].Remove(target);
        }
        else
            m_disablePool.Add(key, new List<GameObject>() { target });
    }

    public void DestroyObject(string key, GameObject target)
    {
        if (m_disablePool.ContainsKey(key) && m_disablePool[key].Contains(target))
            m_disablePool[key].Remove(target);

        if (m_activePool.ContainsKey(key) && m_activePool[key].Contains(target))
            m_activePool[key].Remove(target);
    }

    
    public void RemovePoolObject(string key)
    {
        if (m_disablePool.ContainsKey(key))
        {
            for (int i = m_disablePool[key].Count - 1; i > 0; i--)
            {
                Destroy(m_disablePool[key][i]);
            }
            m_disablePool[key].Clear();
            m_disablePool.Remove(key);
        }

        if (m_activePool.ContainsKey(key))
        {
            for(int i = m_activePool[key].Count - 1; i > 0; i--)
            {
                Destroy(m_activePool[key][i]);
            }
            m_activePool[key].Clear();
            m_activePool.Remove(key);
        }

        if (m_poolBase.ContainsKey(key))
        {
            if(m_poolBase[key] != null)
                Destroy(m_poolBase[key]);

            m_poolBase.Remove(key);
        }
    }
}

public class PoolObejct : MonoBehaviour
{
    public string key = "";

    private void OnDisable()
    {
        ObjectPoolManager.Instance.DisablePool(key, gameObject);
    }

    private void OnDestroy()
    {
        ObjectPoolManager.Instance.DestroyObject(key, gameObject);
    }
}
