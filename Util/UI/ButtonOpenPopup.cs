using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenPopup : UIBaseFormMaker
{
    private Button m_button;
    [SerializeField] private PopupManager.PopupType popupTarget;

    [SerializeField] private object[] popupData;

    private void Awake()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        PopupManager.Instance.ShowPopup(popupTarget, popupData).Forget();
    }
}
