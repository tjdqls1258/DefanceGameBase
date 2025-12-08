using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSceneMove : UIBaseFormMaker
{
    private Button m_button;
    [SerializeField] private SceneInfo.SceneType m_loadScene;
    [SerializeField] private AutoUIManager.UIType m_uiType;

    Action m_sceneLoadAction = null;

    private void Awake()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
        m_sceneLoadAction += () =>
        {
            UIManager.Instance.AutoUIManager.SetUIType(m_uiType);
        };
    }

    public void AddSceneLoadAction(Action action)
    {
        m_sceneLoadAction += action;
    }

    private void OnClickButton()
    {
        SceneLoadManager.Instance.SceneLoad(m_loadScene, m_sceneLoadAction).Forget();
    }
}
