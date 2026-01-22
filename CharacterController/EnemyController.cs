using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IGamePlayCharacter
{
    public void DieAction();
}

public class EnemyController : MonoBehaviour, IGamePlayCharacter
{
    [SerializeField] protected EnemeyData m_enemyData;

    public bool isDie = false;

    List<Vector2> m_movePathList = new();
    protected int m_currentPathIndex = 0;
    protected float m_moveSpeed = 2f;
    [SerializeField] protected float stopDistance = 0.01f;

    CancellationTokenSource m_cancellation = new();
    protected Action<int, EnemyController> m_disableAction;
    [SerializeField] protected HPController m_hpController;
    [SerializeField] protected CharacterAnimationController m_characterAnimationController;

    private void Awake()
    {
        m_characterAnimationController = GetComponent<CharacterAnimationController>();
    }

    protected virtual void SetPath(List<Vector2Int> pathData)
    {
        foreach(var vec in pathData)
            m_movePathList.Add(vec);

        LateUpdateAsync().Forget();
    }

    protected virtual void EnemyMove()
    {
        if (CheckArraivePath(m_movePathList[m_currentPathIndex]))
        {
            m_currentPathIndex = System.Math.Min(m_movePathList.Count, m_currentPathIndex + 1);
            if (m_movePathList.Count == m_currentPathIndex)
                ArraiveEndPosition();
        }
        else
            transform.position = Vector3.MoveTowards(transform.position, m_movePathList[m_currentPathIndex], m_moveSpeed * Time.deltaTime);
    }

    protected virtual async UniTask LateUpdateAsync()
    {
        while (m_cancellation.IsCancellationRequested == false)
        {
            await UniTask.WaitForFixedUpdate();
            //홈으로 돌아갈때 캔슬 토큰 확인
            if(m_cancellation.IsCancellationRequested == false)
                EnemyMove();
        }
    }

    protected bool CheckArraivePath(Vector2 path)
    {
        if(transform == null) return false;

        return Vector2.Distance(path, transform.position) < stopDistance;
    }

    /// <summary>
    /// 적 유닛을 초기화 해줍니다. (체력, 이동 경로)
    /// </summary>
    public virtual void InitEnemyData(EnemeyData enemyData, List<Vector2Int> movePathList, Action<int,EnemyController> disableAction)
    {
        m_enemyData = enemyData;

        m_movePathList.Clear();
        transform.position = new(movePathList[0].x, movePathList[0].y, 0);
        this.m_disableAction = disableAction;
        m_cancellation = new();

        /// 1. 길 설정 
        SetPath(movePathList);

        m_currentPathIndex = 0;
        gameObject.SetActive(true);
        m_hpController.InitController(m_enemyData.characterState, DieAction_Enemy);
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, false);
        isDie = false;
    }

    public virtual void DieAction_Enemy()
    {
        isDie = true;
        m_cancellation.Cancel();
        m_disableAction?.Invoke(m_enemyData.ID, this);
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, true);
    }

    public virtual void ArraiveEndPosition()
    {
        m_cancellation.Cancel();
        gameObject.SetActive(false);
        m_disableAction?.Invoke(m_enemyData.ID, this);
    }

    public virtual void Hit(int atk)
    {
        m_hpController.UpdateHp(-atk);
        m_characterAnimationController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.HIT);
    }

    private void OnDisable()
    {
        m_cancellation.Cancel();
    }

    public virtual void DieAction()
    {
        gameObject.SetActive(false);
    }
}
