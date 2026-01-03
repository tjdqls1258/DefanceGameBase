using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameOptionPanel : UIBaseFormMaker
{
    enum Buttons
    {
        OptionButton,
    }

    enum CanvasGroups
    {
        OptionPanel,
    }

    private Sequence m_activeOptionSequence;
    private Sequence m_deactiveOptionSequence;

    protected override void Awake()
    {
        base.Awake();
        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Button>(typeof(Buttons));

        Get<Button>(0).onClick.AddListener(OnClickOption);

        m_activeOptionSequence = DOTween.Sequence().Append(Get<CanvasGroup>(0).DOFade(1, 0.3f)).SetAutoKill(false).SetUpdate(true);
        m_deactiveOptionSequence = DOTween.Sequence().Append(Get<CanvasGroup>(0).DOFade(0, 0.3f)).SetAutoKill(false).OnComplete(CanvaseClose_TweenEnd).SetUpdate(true);

        m_activeOptionSequence.Pause();
        m_deactiveOptionSequence.Pause();

        void CanvaseClose_TweenEnd()
        {
            Get<CanvasGroup>(0).gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void CanvasAtive(bool active)
    {
        float activeValue = active ? 1f : 0f;
        Get<CanvasGroup>(0).alpha = active ? 0f : 1f;

        if (active)
        {
            Time.timeScale = 0f;
            Get<CanvasGroup>(0).gameObject.SetActive(active);
            m_activeOptionSequence.Restart();
        }
        else
        {
            m_deactiveOptionSequence.Restart();
        }
    }

    private void OnDestroy()
    {
        m_activeOptionSequence.Kill();
        m_deactiveOptionSequence.Kill();
    }

    #region OnClick Func

    public void OnClickBack()
    {
        CanvasAtive(false);
    }

    private void OnClickOption()
    {
        CanvasAtive(true);
    }

    public void OnClickHome()
    {
        GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).ExitGame();
        SceneLoadManager.Instance.SceneLoad(SceneInfo.SceneType.HomeScene).Forget();
        ObjectPoolManager.Instance.ClearNullPoolObject();
        CanvasAtive(false);
    }

    #endregion
}
