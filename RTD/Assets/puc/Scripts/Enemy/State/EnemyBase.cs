using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public Animator anim { get; private set; }
    protected Rigidbody2D rb;
    public SpriteRenderer sr;

    public Transform attackCheck;

    protected bool isDead = false;
    protected bool isAttacking = false;

    public EnemyData stats;
    protected float currentHp;


    [Header("AI Settings")]
    [Tooltip("플레이어와 X축 거리가 이 값보다 커야 flip 처리")]
    public float flipDistanceThreshold = 0.5f;  // ← 기본 0.5 유닛

    public Transform player;

    public LayerMask attackLayer;

    protected EnemyStateMachine stateMachine = new EnemyStateMachine();

    protected EnemyIdleState idleState;
    protected EnemyMoveState moveState;
    protected EnemyAttackState attackState;
    protected EnemyDamageState damageState;

    public bool hasBeenHitRecently = false;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        if (isDead) return;
        stateMachine?.LogicUpdate();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine?.PhysicsUpdate();
    }

    public void InitFromData(EnemyData data)
    {
        stats = data;
        currentHp = data.maxHp;
    }

    public void SetVelocity(Vector2 dir)
    {
        if (isAttacking || isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * stats.moveSpeed;
    }

    public void SetZeroVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public bool IsPlayerInAttackRange()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= stats.attackRadius;
    }

    public bool IsAttackCooldownOver()
    {
        return Time.time >= lastAttackTime + stats.attackCooldown;
    }

    private float lastAttackTime;
    public void SetLastAttackTime()
    {
        lastAttackTime = Time.time;
    }

    public bool IsAttacking() => isAttacking;
    public void SetAttacking(bool active) => isAttacking = active;

    public void OnAttackTrigger()
    {
        Debug.Log("[EnemyBase] ▶ OnAttackTrigger 전달");  // ← 이 줄 추가
        if (stateMachine.CurrentState is EnemyAttackState atk)
            atk.OnAttackTrigger();
    }

    public void EndAttack()
    {
        if (stateMachine.CurrentState is EnemyAttackState atk)
            atk.EndAttack();
    }

    public virtual void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackCheck.position, stats.attackRadius, attackLayer
        );

        Debug.Log($"[EnemyBase] 공격 범위 내 감지된 대상 수: {hits.Length}");

        foreach (var hit in hits)
        {
            Debug.Log($"[EnemyBase] 감지된 대상: {hit.name}, 태그: {hit.tag}");

            if (hit.CompareTag("Player"))
            {
                var player = hit.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    Debug.Log($"[EnemyBase] PlayerController 데미지 적용");
                    player.TakeDamage((int)stats.attackDamage);
                }
                else
                {
                    Debug.LogWarning($"[EnemyBase] PlayerController를 찾을 수 없습니다: {hit.name}");
                }
            }
        }
    }

    public virtual void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHp -= dmg;

        if (currentHp <= 0)
            Die();
        else
        {
            if (player == null)
            {
                Debug.LogWarning("[EnemyBase] TakeDamage: player가 설정되지 않았습니다.");
            }

            stateMachine.ChangeState(damageState);
        }
    }

    public void OnHit(int damage, Transform attacker)
    {
        player = attacker;               // 공격자 등록
        hasBeenHitRecently = true;       // 피격 상태 표시
        TakeDamage(damage);
    }

    public void ClearHitFlag()
    {
        hasBeenHitRecently = false;
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("IsDead");
        SetZeroVelocity();
        stateMachine.ClearCurrentState();
    }

    public void OnDeathAnimationComplete()
    {
        Debug.Log("[EnemyBase] 사망 애니메이션 완료 → 오브젝트 제거");
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheck.position, stats.attackRadius);
        }
    }
}
