using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnClickCharacterPaenl : CachObject
{
    enum Images
    {
        CharacterImage
    }

    enum Buttons
    {
        UpgradButton,
        SkillButton,
        Back,
    }

    enum TextMeshPros
    {
        UpgradText,
        SkillText
    }

    private InGameCharacterData m_currentCharaterData;
    private Action activeAction = null;
    private Action disableAction = null;
    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TextMeshPros));
        Get<TextMeshProUGUI>((int)TextMeshPros.SkillText).text = "SKILL";
        Get<TextMeshProUGUI>((int)TextMeshPros.UpgradText).text = "UPGRAD";
        Get<Button>((int)Buttons.Back).onClick.AddListener(ClosePanel);
    }

    public void OnClickCharacter(InGameCharacterData characterData, Action activeAction = null, Action disableAction = null)
    {
        gameObject.SetActive(true);
        m_currentCharaterData = characterData;

        activeAction?.Invoke();
        this.disableAction = disableAction;
        Time.timeScale = 0f;
        Get<Image>((int)Images.CharacterImage).sprite = characterData.characterData.characterSprite;
        
    }

    private void ClosePanel()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        disableAction?.Invoke();
    }
}
