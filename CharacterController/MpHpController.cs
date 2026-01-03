using System;
using UnityEngine;
using UnityEngine.UI;

public class MpHpController : HPController
{
    [Header("Mp Setting")]
    [SerializeField] protected Image m_mpBar;
    [SerializeField] protected float m_maxMP;
    protected float m_currentMp;

    public float currentMp
    {
        private set
        {
            m_currentMp = value;
            m_mpBar.fillAmount = Math.Min(m_currentMp / m_maxMP, 1);
        }
        get
        {
            return m_currentMp;
        }
    }

    public override void InitController(CharacterState characterState)
    {
        base.InitController(characterState);
        var mpCharacter = characterState as MpCharacterState;

        m_maxMP = mpCharacter.maxMp;
        currentMp = mpCharacter.maxMp;
    }

    public void UpDateMp(float addMp)
    {
        currentMp = currentMp + addMp;
    }
}
