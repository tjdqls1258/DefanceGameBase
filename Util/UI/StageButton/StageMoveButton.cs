using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class StageMoveButton : CachObject
{
    enum Texts
    {
        StageText
    }

    int main, sub;
    Button m_myButton;
    public void Init(int mainStage, int subStage)
    {
        main = mainStage;
        sub = subStage;

        m_myButton = GetComponent<Button>();
        Bind<TextMeshProUGUI>(typeof(Texts));

        m_myButton.onClick.AddListener(SceneMove);
        Get<TextMeshProUGUI>((int)Texts.StageText).text = $"{main}-{sub}";
    }

    void SceneMove()
    {
        GameData.Instance.MainStage = main;
        GameData.Instance.SubStage = sub;

        SceneLoadManager.Instance.SceneLoad(SceneInfo.SceneType.GameScene, () =>
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).SetInGameDataTest();
            GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.inGame);
        }).Forget();
    }
}
