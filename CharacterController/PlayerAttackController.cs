using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어 캐릭터의 공격 로직을 제어하는 컨트롤러입니다.
/// 공격 사거리 내의 적을 감지하고, 가장 가까운 적을 대상으로 주기적인 자동 공격을 수행합니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerAttackController : MonoBehaviour
{
    private readonly string effectName = "EffectPrefabs/Hit_FX01.prefab";

    // ====== Inspector Settings ======
    [Header("Attack Settings")]
    [Tooltip("공격 사거리 (CircleCollider2D의 반지름으로 사용됨).")]
    [SerializeField] private float m_attackDistance = 5f;

    [Tooltip("공격 사이의 지연 시간 (초).")]
    [SerializeField] private float m_attackDelay = 1f;

    // ====== Runtime State & Caches ======

    /// <summary> 공격 사거리 내에 들어온 모든 EnemyController 인스턴스 목록입니다. </summary>
    private List<EnemyController> m_enemyList = new();

    /// <summary> 현재 플레이어가 공격하는 대상입니다. (가장 가까운 적) </summary>
    private EnemyController m_target; // 단일 타겟 모드에서 사용

    /// <summary> 다음 공격까지 경과된 시간입니다. </summary>
    private float m_currentDelay = 0;

    [SerializeField] private GameObject m_atkRangeObject;
    private InGameCharacterData m_characterData;

    // ----------------------------------------------------------------------
    // ## Initialization & Public Interface
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 공격 거리를 감지 콜라이더의 반지름으로 설정하고 트리거 모드로 전환합니다.
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.radius = m_attackDistance;

            m_atkRangeObject.transform.localScale = Vector3.one * m_attackDistance * 2;
        }
    }

    public void InitCharacterData(InGameCharacterData characterData)
    {
        m_characterData = characterData;
        SetEffect().Forget();
    }

    private async UniTask SetEffect()
    {
        if (ObjectPoolManager.Instance.CheckAddKey(effectName))
            return;
        ObjectPoolManager.Instance.AddKey(effectName);
        var effectObject = await AddressableManager.Instance.InstantiateObjectAsync(effectName);
        ObjectPoolManager.Instance.SetPoolObject(effectName, effectObject);
    }

    // ----------------------------------------------------------------------
    // ## Update Logic (Targeting & Attack Timing)
    // ----------------------------------------------------------------------

    private void Update()
    {
        // 1. 적 목록이 비어있는 경우
        if (m_enemyList.Count <= 0)
        {
            // 타이머 초기화 및 타겟 해제
            if (m_currentDelay > 0.001f)
                m_currentDelay = 0;
            m_target = null;
            return;
        }

        SetTarget();

        // 4. 공격 실행
        if (m_target != null)
            CharacterAction(m_target);
    }

    protected virtual void SetTarget()
    {
        // 2. 현재 타겟 검사 및 목록 정리
        if (m_target == null || m_target.isDie)
        {
            // 목록에서 죽은 적을 정리합니다.
            m_enemyList.RemoveAll(e => e.isDie);

            // 3. 타겟 재탐색 (가장 가까운 적을 m_target으로 설정)
            if (m_enemyList.Count > 0)
            {
                m_target = m_enemyList.OrderBy(e => Vector2.Distance(e.transform.position, transform.position)).FirstOrDefault();
            }
            else
            {
                // 목록 정리 후 적이 남아있지 않으면 종료
                m_target = null;
                return;
            }
        }
    }

    // ----------------------------------------------------------------------
    // ## Collision Detection (Target Management)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 공격 사거리(트리거 콜라이더) 내로 적이 진입했을 때 호출됩니다.
    /// </summary>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null && !m_enemyList.Contains(enemy))
        {
            m_enemyList.Add(enemy);
        }
    }

    /// <summary>
    /// 공격 사거리(트리거 콜라이더) 밖으로 적이 벗어났을 때 호출됩니다.
    /// </summary>
    protected void OnTriggerExit2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            m_enemyList.Remove(enemy);

            // 타겟이 이탈했다면 타겟을 null로 설정하여 다음 Update에서 재탐색하도록 유도
            if (enemy == m_target)
                m_target = null;
        }
    }

    // ----------------------------------------------------------------------
    // ## Attack Execution
    // ----------------------------------------------------------------------

    /// <summary>
    /// 공격 딜레이를 계산하고, 딜레이가 충족되면 실제 공격 행동을 실행합니다.
    /// </summary>
    /// <param name="target">공격 대상 EnemyController</param>
    private void CharacterAction(EnemyController target)
    {
        // 딜레이 타이머 업데이트
        if (m_currentDelay <= m_attackDelay)
            m_currentDelay += Time.deltaTime;
        else
        {
            // 딜레이 충족: 공격 실행
            m_currentDelay = 0; // 딜레이 초기화

            // TODO: 실제 공격 로직 (데미지 계산, 애니메이션 재생 등)을 여기에 구현
            Logger.Log($"Action {target.gameObject.name}: ATTACK!");
            var effect = ObjectPoolManager.Instance.AddPoolObject(effectName);
            effect.transform.position = target.transform.position;  
        }
    }

    public GameObject GetAtkRangeObject() => m_atkRangeObject;

    private void OnDestroy()
    {
        ObjectPoolManager.Instance.RemovePoolObject(effectName);
    }
}