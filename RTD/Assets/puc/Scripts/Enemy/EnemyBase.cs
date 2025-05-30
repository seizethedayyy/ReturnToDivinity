using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private float hitStunDuration = 0.5f; // 경직 시간

    public EnemyStats stats;
    public Transform attackCheck;
    public float attackOffsetX = 1.0f;
    public float attackOffsetY = 0.0f;

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

    // 🔒 사망 중복 방지용
    private bool isDead = false;

    // 🔴 점멸 효과용
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

        idleState = new EnemyIdleState(this, stateMachine);
        moveState = new EnemyMoveState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);

        idleState.SetNextState(moveState);
        moveState.SetNextState(attackState);
        attackState.SetNextState(idleState);

        stateMachine.Initialize(idleState);

        currentHp = stats.maxHp;

        // 점멸 초기색 저장
        originalColor = sr.color;
        if (originalColor.a < 1f) originalColor.a = 1f;
    }

    protected virtual void Start()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsWalking", false);
        anim.ResetTrigger("IsHit");
    }

    protected virtual void Update()
    {
        stateMachine.LogicUpdate();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();

        if (attackCheck != null)
        {
            int dir = sr.flipX ? -1 : 1;
            attackCheck.localPosition = new Vector3(attackOffsetX * dir, attackOffsetY, 0f);
        }
    }

    public void SetVelocity(Vector2 dir)
    {
        if (isAttacking)
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
        var hits = Physics2D.OverlapCircleAll(
            attackCheck.position,
            stats.attackRadius,
            LayerMask.GetMask("Player")
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out PlayerController pc))
            {
                pc.PrepareHitEffect();
                pc.TakeDamage((int)stats.attackDamage);
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"[Enemy] TakeDamage() 호출됨 - 현재 HP: {currentHp}, 받는 데미지: {damage}");

        PrepareHitEffect();

        // ✅ 경직 시간 동안 공격 불가하도록 공격 타이머 초기화
        lastAttackTime = Time.time + hitStunDuration - stats.attackCooldown;

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

    public virtual void OnHit(int damage)
    {
        TakeDamage(damage);
    }

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
}
