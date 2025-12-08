using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 개별 타일 아이템 버튼 컴포넌트입니다.
/// 맵 에디터 UI의 타일 팔레트에서 각 타일 스프라이트를 표시하고,
/// 클릭 시 해당 타일 정보를 MapEditorUI로 전달하여 선택 상태를 관리합니다.
/// </summary>
public class TileItemButton : MonoBehaviour
{
    // ====== Private Fields ======

    /// <summary> 이 버튼이 나타내는 타일 스프라이트의 이름입니다. </summary>
    private string m_spriteName;

    /// <summary> 타일 이미지를 표시하는 Image 컴포넌트입니다. </summary>
    private Image m_currentImage;

    /// <summary> 버튼 클릭 시 실행될 콜백 액션입니다. (MapEditorUI의 OnClickAction과 연결) </summary>
    /// <remarks> 스프라이트 이름과 자기 자신(TileItemButton) 인스턴스를 인자로 전달합니다. </remarks>
    private UnityAction<string, TileItemButton> m_onClickSetting;

    // ----------------------------------------------------------------------
    // ## Lifecycle & Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // Image 컴포넌트 캐시: Awake에서 한 번만 검색하여 성능 최적화
        if (m_currentImage == null)
            m_currentImage = GetComponent<Image>();
    }

    /// <summary>
    /// 버튼의 이미지와 클릭 이벤트를 설정합니다. (MapEditorUI.Load에서 호출)
    /// </summary>
    /// <param name="sprite">버튼에 표시할 Sprite 인스턴스입니다.</param>
    /// <param name="action">버튼 클릭 시 호출될 콜백 함수입니다.</param>
    public void SetImage(Sprite sprite, UnityAction<string, TileItemButton> action)
    {
        if (m_currentImage == null)
            m_currentImage = GetComponent<Image>();

        // Sprite Atlas에서 로드된 스프라이트 이름의 불필요한 부분(예: (Clone))을 제거합니다.
        // 스프라이트 이름은 첫 번째 '(' 이전의 문자열만 사용합니다.
        m_spriteName = sprite.name.Split('(')[0];

        m_currentImage.sprite = sprite;
        m_onClickSetting = action;
    }

    // ----------------------------------------------------------------------
    // ## Public Actions
    // ----------------------------------------------------------------------

    /// <summary>
    /// Unity Button 컴포넌트의 OnClick 이벤트에 연결하여 사용합니다.
    /// 설정된 콜백 함수를 호출하며, 현재 스프라이트 이름과 자기 자신 인스턴스를 전달합니다.
    /// </summary>
    public void OnClick()
    {
        // 콜백 함수가 null이 아닐 경우에만 호출합니다.
        m_onClickSetting?.Invoke(m_spriteName, this);
    }

    /// <summary>
    /// 버튼의 시각적 강조 상태를 설정합니다. (MapEditorUI에서 호출)
    /// </summary>
    /// <param name="highlight">강조(true) 또는 해제(false) 여부입니다.</param>
    public void SetHighlight(bool highlight)
    {
        // 강조 시 Image 색상을 '회색'으로, 해제 시 '흰색'으로 변경합니다.
        m_currentImage.color = highlight ? Color.gray : Color.white;
    }

    /// <summary>
    /// 현재 버튼이 가진 스프라이트 이름을 반환합니다. (MapEditorUI에서 검색 및 확인용)
    /// </summary>
    /// <returns>설정된 스프라이트 이름</returns>
    public string GetSpriteName() => m_spriteName;
}