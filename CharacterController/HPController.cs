using System;
using UnityEngine;
using UnityEngine.UI;

public class HPController : MonoBehaviour
{
    [Header("Hp Setting")]
    [SerializeField] protected Image m_hpBar;
    [SerializeField] protected float m_maxHP;
    protected float m_currentHp = 0;

    public float currentHp
    {
        protected set
        {
            m_currentHp = value;
            m_hpBar.fillAmount = Math.Min(m_currentHp / m_maxHP, 1);
        }
        get
        {
            return m_currentHp;
        }
    }

    public virtual void InitController(CharacterState characterStateData)
    {
        Logger.Log($"Set Hp {characterStateData.maxHp}");
        m_maxHP = characterStateData.maxHp;
        currentHp = characterStateData.maxHp;
    }

    public void UpdateHp(float addHp)
    {
        Logger.Log($"AddHp {currentHp} + {addHp}");
        currentHp = currentHp + addHp;
    }
}
