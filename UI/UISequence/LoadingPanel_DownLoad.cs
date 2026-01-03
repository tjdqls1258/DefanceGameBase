using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class LoadingPanel : CachObject
{
    [Header("Loading Panel")]
    [SerializeField] private Image m_loadingBar;
    [SerializeField] private TextMeshProUGUI m_loadingText;

    public void LoadingText(string label ,long current, long max)
    {
        Logger.Log($"Download {label} : Loading {Math.Truncate((double)current / max * 100).ToString("N0")}");
        m_loadingText.text = $"Download {label} : {Math.Truncate((double)current/max * 100).ToString("N0")}%";
        m_loadingBar.fillAmount = current / max;
    }
}
