using System;
using System.Collections.Generic;
using UnityEngine;

public class CachObject : MonoBehaviour
{
    protected GameObject m_gameObject;
    protected Transform m_transform;
    protected RectTransform m_rectTransform;
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new();

    public GameObject MyObj
    {
        get
        {
            if (m_gameObject == null)
                m_gameObject = gameObject;
            return m_gameObject;
        }
    }

    public Transform MyTr
    {
        get
        {
            if (m_transform == null)
                m_transform = gameObject.transform;
            return m_transform;
        }
    }

    public RectTransform MyRT
    {
        get
        {
            if (m_rectTransform == null)
                m_rectTransform = GetComponent<RectTransform>();
            return m_rectTransform;
        }
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = GameUtil.FindChild(gameObject, names[i], true);
            else
                objects[i] = GameUtil.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Logger.LogError($"Bind Fail ({names[i]})");
        }
    }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if(_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }
}
