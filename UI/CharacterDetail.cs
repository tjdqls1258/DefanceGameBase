using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetail : CachObject
{
    enum Images
    {
        CharacterImage,
    }
    
    enum Texts
    {
        StateLV_Text,
        State_Text
    }

    enum Buttons
    {
        Close
    }

    private CanvasGroup m_group;
    private float m_fadeTime = 0.3f;

    private CharacterData m_characterData;

    private void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.Close).onClick.AddListener(Close);
        //비활성화 필요
        m_group.alpha = 0;
        gameObject.SetActive(false);
    }

    public void OnClickData(CharacterData data)
    {
        gameObject.SetActive(true);
        m_group.DOFade(1, m_fadeTime);
        m_characterData = data;

        Get<Image>((int)Images.CharacterImage).sprite = m_characterData.characterSprite;
        Get<TextMeshProUGUI>((int)Texts.StateLV_Text).text = "1";
        Get<TextMeshProUGUI>((int)Texts.State_Text).text = $"{m_characterData.characterName} data Not Ready";
    }

    public void Close()
    {
        m_group.DOFade(0, m_fadeTime).OnComplete(()=>gameObject.SetActive(false));
    }
}
