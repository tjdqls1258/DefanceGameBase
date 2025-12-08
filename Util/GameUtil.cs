using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MapData;

/// <summary>
/// 게임 전반에서 재사용 가능한 유틸리티 기능을 제공하는 정적 클래스입니다.
/// 메인 카메라 캐싱 및 GameObject 계층 구조 내에서 하위 컴포넌트나 객체를 검색하는 기능을 포함합니다.
/// </summary>
public static class GameUtil
{
    // ====== Main Camera Access ======

    private static Camera m_mainCamera;

    /// <summary>
    /// 게임 내 메인 카메라 인스턴스를 캐싱하여 반환합니다.
    /// 첫 접근 시 Scene에서 카메라를 검색하며, 이후에는 캐시된 인스턴스를 사용합니다.
    /// </summary>
    public static Camera mainCamera
    {
        get
        {
            if (m_mainCamera.IsUnityNull())
                // Scene에서 활성화된 Camera 컴포넌트를 찾습니다.
                m_mainCamera = GameObject.FindAnyObjectByType<Camera>();

            return m_mainCamera;
        }
    }

    // ====== Child Finding Utilities (Generic) ======

    /// <summary>
    /// 지정된 GameObject의 하위(Child) 계층 구조에서 특정 타입의 컴포넌트 T를 검색하여 반환합니다.
    /// </summary>
    /// <typeparam name="T">찾으려는 컴포넌트 타입 (UnityEngine.Object를 상속)</typeparam>
    /// <param name="go">검색을 시작할 부모 GameObject</param>
    /// <param name="name">찾으려는 객체의 이름 (null 또는 Empty면 이름 검사 생략)</param>
    /// <param name="recursive">true면 자식의 자식까지 모두 검색 (GetComponentsInChildren 사용), false면 1단계 자식만 검색</param>
    /// <returns>찾은 첫 번째 T 타입 컴포넌트 또는 null</returns>
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            // 1단계 자식만 검색 (이름 일치 및 컴포넌트 T가 존재하는지 확인)
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            // 모든 하위 자식까지 검색 (GetComponentsInChildren 사용)
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                // T가 Transform인 경우 (FindChild<Transform> 호출 시) component.name은 Transform의 이름이 됩니다.
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    // ====== Child Finding Utilities (GameObject Overload) ======

    /// <summary>
    /// 지정된 GameObject의 하위 계층 구조에서 특정 이름 또는 임의의 GameObject를 검색하여 반환합니다.
    /// (내부적으로 FindChild<Transform>을 사용하여 GameObject를 찾습니다.)
    /// </summary>
    /// <param name="go">검색을 시작할 부모 GameObject</param>
    /// <param name="name">찾으려는 객체의 이름</param>
    /// <param name="recursive">true면 자식의 자식까지 모두 검색</param>
    /// <returns>찾은 GameObject 또는 null</returns>
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        // Transform을 검색하여 해당 GameObject를 반환합니다.
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static List<Vector2Int> ConvartSerializableVector2IntToVector2Int_List(List<SerializeableVector2Int> list)
    {
        List<Vector2Int> result = new();
        foreach(var vecInt in list)
        {
            result.Add(new(vecInt.x, vecInt.y));
        }

        return result;
    }
}