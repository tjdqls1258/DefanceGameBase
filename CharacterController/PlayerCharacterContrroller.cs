using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCharacterContrroller : MonoBehaviour
{
    private List<SkillBase> m_currentSkill;
    private InGameCharacterData m_characterData;
    private PlayerAttackController m_atkController;
    private CharacterAnimationController m_characterAniumationController;

    /// <summary> 유닛이 사망했을 때 호출될 UnityAction 델리게이트입니다. </summary>
    private UnityAction m_unitDieAction;
    private bool m_onClick = false;
    private bool m_isSpawn = false;

    private void Awake()
    {
        m_atkController = GetComponent<PlayerAttackController>();
        m_characterAniumationController = GetComponent<CharacterAnimationController>();
    }

    public void SetCharacter(InGameCharacterData characterData)
    {
        m_characterData = characterData;
        m_atkController.InitCharacterData(m_characterData, m_characterAniumationController);
        //스킬 추가
    }

    public void SetSkill()
    {

    }

    public InGameCharacterData GetCharacterData() => m_characterData;

    public void AddDieAction(UnityAction action)
    {
        m_unitDieAction += action;
    }

    public void OnPointerDownAction()
    {
        m_onClick = true;
        AtkAreaActive(m_onClick);
    }

    public void OnPointerUpAction()
    {
        m_onClick = false;
        AtkAreaActive(m_onClick);
    }

    public void AtkAreaActive(bool Active)
    {
        m_atkController.GetAtkRangeObject().SetActive(Active);
    }

    public void SetSpawn(bool isSpawn)
    {
        m_atkController.enabled = isSpawn;
        m_isSpawn = isSpawn;
    }

    public bool CheckSpawn() => m_isSpawn;
}
