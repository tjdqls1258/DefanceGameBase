using UnityEngine;

public class CharacterListPanel : UIBase
{
    [SerializeField] private CharacterPanelScroll m_characterScrollView;
    [SerializeField] private CharacterDetail m_characterDetail;

    protected override void Awake()
    {
        m_UISequence = UIManager.UISequence.CharacterListPanel;

        m_characterScrollView.OnCellClicked(index =>
        {
            m_characterDetail.OnClickData(index);
        }); 
        //테스트 용
        m_characterScrollView.UpdateContents(GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetDefaultList());
    }
}
