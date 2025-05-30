using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    public EnemyData stats;

    public Transform attackCheck;
    public float attackOffsetX = 1.0f;
    public float attackOffsetY = 0.0f;

    [Header("🔍 공격 대상 레이어")]
    public LayerMask attackLayer;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public Transform player;

    public EnemyStateMachine stateMachine;

    public EnemyIdleState idleState;
    public EnemyMoveState moveState;
    public EnemyAttackState attackState;

    protected float currentHp;
    private float lastAttackTime = Mathf.NegativeInfinity;
    private bool isAttacking = false;
    private bool isDead = false;

    private Coroutine flashCoroutine;
    private Color originalColor;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        stateMachine = new EnemyStateMachine();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj) player = playerObj.transform;

        originalColor = sr.color;
        if (originalColor.a < 1f) originalColor.a = 1f;
    }

    protected virtual void Start()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsWalking", false);
        anim.ResetTrigger("IsHit");
    }

    void Update()
    {
        Debug.Log($"[FSM] 현재 상태: {stateMachine.CurrentState?.GetType().Name ?? "NULL"}");
        stateMachine.LogicUpdate();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();

        if (attackCheck != null)
        {
            int dir = transform.localScale.x < 0 ? -1 : 1;
            attackCheck.localPosition = new Vector3(attackOffsetX * dir, attackOffsetY, 0f);
        }
    }

    public void InitFromData(EnemyData data)
    {
        stats = data;
        currentHp = stats.maxHp;
        Debug.Log($"[EnemyBase] InitFromData 적용됨 - HP: {currentHp}, Speed: {stats.moveSpeed}");
    }

    public void SetupFSM()
    {
        idleState = new EnemyIdleState(this, stateMachine);
        moveState = new EnemyMoveState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);

        ((EnemyIdleState)idleState).SetNextState(moveState);
        ((EnemyMoveState)moveState).SetNextState(attackState);
        ((EnemyAttackState)attackState).SetNextState(idleState);

        stateMachine.ChangeState(idleState);
    }

    public void SetVelocity(Vector2 dir)
    {
        if (player == null || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * stats.moveSpeed;

        // 위치 반영 보강
        Physics2D.SyncTransforms();
    }

    public void SetZeroVelocity() => rb.linearVelocity = Vector2.zero;

    public bool IsPlayerInAttackRange()
    {
        Physics2D.SyncTransforms();

        var hits = Physics2D.OverlapCircleAll(
            attackCheck.position,
            stats.attackRadius,
            LayerMask.GetMask("Player")
        );

        Debug.Log($"[Enemy] 공격 판정 수: {hits.Length} @ {attackCheck.position}");
        return hits.Length > 0;
    }

    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCheck.position, stats.attackRadius, attackLayer);
        Debug.Log($"[Enemy] 공격 판정 수: {hits.Length} @ {attackCheck.position}");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[Enemy] 피격 대상: {hit.name}");

            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>().TakeDamage((int)stats.attackDamage);
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"[Enemy] TakeDamage() 호출됨 - 현재 HP: {currentHp}, 받는 데미지: {damage}");

        PrepareHitEffect();
        lastAttackTime = Time.time + 0.5f - stats.attackCooldown;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("IsHit");
            stateMachine.ChangeState(new EnemyDamageState(this, stateMachine, idleState));
        }
    }

    public virtual void OnHit(int damage) => TakeDamage(damage);

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Enemy] Die() 호출됨 - 사망 애니메이션 시작");

        stateMachine?.ChangeState(null);
        SetZeroVelocity();
        anim.SetBool("IsDead", true);
    }

    public void DestroySelf()
    {
        if (!isDead) return;
        Debug.Log("[Enemy] DestroySelf() 호출됨");
        Destroy(gameObject);
    }

    public void PrepareHitEffect()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    public bool IsAttackCooldownOver() => Time.time >= lastAttackTime + stats.attackCooldown;
    public void SetLastAttackTime() => lastAttackTime = Time.time;

    public void SetAttacking(bool value) => isAttacking = value;
    public bool IsAttacking() => isAttacking;

    private void OnDrawGizmosSelected()
    {
        if (attackCheck == null || player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCheck.position, stats != null ? stats.attackRadius : 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackCheck.position, player.position);
    }

}
