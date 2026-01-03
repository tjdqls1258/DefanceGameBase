using UnityEngine;

public class CharacterListPanel : UIBase
{
    [SerializeField] private CharacterData[] m_characterDatasTest;
    [SerializeField] private CharacterPanelScroll m_characterScrollView;
    [SerializeField] private CharacterDetail m_characterDetail;

    protected override void Awake()
    {
        m_UISequence = UIManager.UISequence.CharacterListPanel;

        m_characterScrollView.OnCellClicked(index =>
        {
            m_characterDetail.OnClickData(index);
        }); 
        m_characterScrollView.UpdateContents(m_characterDatasTest);
    }
}
