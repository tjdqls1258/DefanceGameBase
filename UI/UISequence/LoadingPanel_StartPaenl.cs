using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class LoadingPanel : CachObject
{
    [Space, Header("StartPaenl")]
    [SerializeField] private Button m_onClickButton;

    public void AddOnClickAction(UnityAction action)
    {
        m_onClickButton.onClick.RemoveListener(action);
        m_onClickButton.onClick.AddListener(action);
    }
}
