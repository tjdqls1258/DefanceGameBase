using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class AwakeScene : MonoBehaviour
{
    [SerializeField] private LoadingPanel m_loadingPanel;

    private void Start()
    {
        StartAscy().Forget();
    }

    async UniTask StartAscy()
    {
        GameMaster.Instance.Init();
        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.DownloadPaenl);
        await GameMaster.Instance.InitAddress(m_loadingPanel.LoadingText, () =>
        {
            InitStart().Forget();
        });
    }

    async UniTask InitStart()
    {
        await GameMaster.Instance.InitAscy();
        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.StartPaenl);

        m_loadingPanel.AddOnClickAction(() =>
        {
            TaskHelp().Forget();

            async UniTask TaskHelp()
            {
                await GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene);
                GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.main, true);
            }
        });
    }
}
