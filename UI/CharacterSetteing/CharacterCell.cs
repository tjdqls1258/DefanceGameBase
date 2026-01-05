using FancyScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCell : FancyGridViewCell<CharacterData, CharacterPanelContext>
{
    private CharacterData m_data;
    [SerializeField] private Image m_characterImage;
    [SerializeField] private TextMeshProUGUI m_characterName;
    [SerializeField] private TextMeshProUGUI m_characterLevel;

    public override void UpdateContent(CharacterData itemData)
    {
        m_data = itemData;
        m_characterImage.sprite = itemData.GetCharacterSprite();
        m_characterName.text = itemData.characterName;
    }

    public void OnClick()
    {
        Context.OnCellClicked.Invoke(m_data);
    }
}
