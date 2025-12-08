using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterContrroller : MonoBehaviour
{
    private List<SkillBase> m_currentSkill;
    private CharacterData m_characterData;
    private PlayerAttackController m_atkController;

    private void Awake()
    {
        m_atkController = GetComponent<PlayerAttackController>();    
    }

    public void SetCharacter(CharacterData characterData)
    {
        m_characterData = characterData;
        m_atkController.InitCharacterData(characterData);
        //스킬 추가
    }

    public void SetSkill()
    {

    }

    public CharacterData GetCharacterData() => m_characterData;
}
