using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    public enum AnimationTrigger
    {
        None,
        ATK,
        HIT,
        SKILL
    }    

    public enum AnimationBool
    {
        None,
        DIE
    }

    private Animator m_animator;
    private UnityAction m_atteckAction;
    private UnityAction m_hitAction;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void PlayAnimation_Trigger(AnimationTrigger triggerName)
    {
        m_animator.SetTrigger(triggerName.ToString());
    }

    public void PlayAnimation_Bool(AnimationBool aniName, bool value)
    {
        m_animator.SetBool(aniName.ToString(), value);
    }

    public void EventAtteckAnimation()
    {
        if (m_atteckAction != null) 
            m_atteckAction.Invoke();
    }

    public void EventHitAnimation()
    {
        if(m_hitAction != null)
            m_hitAction.Invoke();
    }

    public void SetAction(UnityAction atteckAction, UnityAction hitAction)
    {
        m_atteckAction = atteckAction;
        m_hitAction = hitAction;
    }

    public void AddAction(UnityAction atteckAction, UnityAction hitAction)
    {
        if (atteckAction != null)
            m_atteckAction += atteckAction;
        if(hitAction != null)
            m_hitAction += hitAction;
    }

    public void RemoveAction(UnityAction atteckAction, UnityAction hitAction)
    {
        if (atteckAction != null)
            m_atteckAction -= atteckAction;
        if (hitAction != null)
            m_hitAction -= hitAction;
    }
}
