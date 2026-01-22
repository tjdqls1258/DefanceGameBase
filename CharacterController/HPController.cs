using System;
using UnityEngine;
using UnityEngine.UI;

public class HPController : MonoBehaviour
{
    [Header("Hp Setting")]
    [SerializeField] protected Image m_hpBar;
    [SerializeField] protected float m_maxHP;
    protected float m_currentHp = 0;

    protected Action m_dieAction;

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

    public virtual void InitController(CharacterState characterStateData, Action dieAction)
    {
        Logger.Log($"Set Hp {characterStateData.maxHp}");
        m_maxHP = characterStateData.maxHp;
        currentHp = characterStateData.maxHp;

        m_dieAction = dieAction;
    }

    public void UpdateHp(float addHp)
    {
        Logger.Log($"AddHp {currentHp} + {addHp}");
        currentHp = currentHp + addHp;
        if (currentHp <= 0 && m_dieAction != null)
            m_dieAction.Invoke();
    }
}
