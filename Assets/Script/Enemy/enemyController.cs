using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHp = 30f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRadius = 2.0f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float chaseStopRange = 7f;
    [SerializeField] private float stopDistance = 1.0f;
    [SerializeField] private float angleThreshold = 90f;

    private float currentHp;
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D enemyCollider;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool hasDealtDamageThisAttack = false;

    private Vector2 currentDirection;
    private float patrolTimer = 0f;
    private float patrolCooldown = 0f;
    private bool isPatrolling = false;

    private string lastStateName = "";
    private int facingDir = 1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>();
        currentHp = maxHp;

        player = GameObject.FindWithTag("Player")?.transform;
        Debug.Log($"[Awake] Player 찾음? → {(player != null ? "성공" : "실패")}");

        facingDir = transform.localScale.x < 0 ? -1 : 1;
    }

    private void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            Debug.LogWarning("[Update] player가 null임 → 공격 로직 미실행");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool inRange = IsPlayerInAttackArea();

        Debug.Log($"[Update] 거리: {distanceToPlayer:F2}, inRange: {inRange}, facingDir: {facingDir}");

        string currentState = GetCurrentAnimStateName();
        if (currentState != lastStateName)
        {
            if (currentState == "Attack")
                hasDealtDamageThisAttack = false;

            lastStateName = currentState;
        }

        if (inRange)
        {
            Debug.Log("[Update] 공격 범위 조건 만족 → 공격 상태 진입");
            rb.linearVelocity = Vector2.zero;

            if (!isAttacking)
            {
                isAttacking = true;
                SetAnimState(false, true);
            }

            if (!hasDealtDamageThisAttack && IsInAnimation("Attack"))
            {
                TryDealDamage();
            }
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer <= chaseStopRange)
        {
            isAttacking = false;
            SetAnimState(true, false);
            MoveTowardsPlayer(distanceToPlayer);
        }
        else
        {
            isAttacking = false;
            SetAnimState(isPatrolling, false);
            RandomPatrol();
        }

        if (!IsInAnimation("Attack"))
        {
            hasDealtDamageThisAttack = false;
        }
    }

    private void SetAnimState(bool isWalking, bool isAttacking)
    {
        Debug.Log($"[SetAnimState] isWalking: {isWalking}, isAttacking: {isAttacking}");
        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("IsAttacking", isAttacking);
    }

    private bool IsPlayerInAttackArea()
    {
        if (player == null) return false;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);
        float angle = Vector2.Angle(Vector2.right * facingDir, dirToPlayer);

        Debug.Log($"[IsPlayerInAttackArea] distance: {distance:F2}, angle: {angle:F2}");
        return distance <= attackRadius && angle <= angleThreshold;
    }

    private void TryDealDamage()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        float angle = Vector2.Angle(Vector2.right * facingDir, (player.position - transform.position).normalized);

        Debug.Log($"[TryDealDamage] distance: {distance:F2}, angle: {angle:F2}");

        if (distance <= attackRadius && angle <= angleThreshold)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.Damage(attackDamage);
                hasDealtDamageThisAttack = true;
                Debug.Log("[공격 성공] 플레이어에게 데미지 줌");
            }
            else
            {
                Debug.LogWarning("[공격 실패] Player 스크립트를 찾지 못함");
            }
        }
    }

    private void MoveTowardsPlayer(float distanceToPlayer)
    {
        if (player == null) return;

        if (distanceToPlayer > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;

            if (dir.x != 0 && ((dir.x > 0 && facingDir == -1) || (dir.x < 0 && facingDir == 1)))
            {
                Flip();
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Flip()
    {
        facingDir *= -1;
        Vector3 scale = transform.localScale;
        scale.x = facingDir;
        transform.localScale = scale;
    }

    private void RandomPatrol()
    {
        if (isPatrolling)
        {
            if (IsObstacleAhead())
            {
                isPatrolling = false;
                rb.linearVelocity = Vector2.zero;
                patrolCooldown = 1f;
                return;
            }

            patrolTimer += Time.deltaTime;
            rb.linearVelocity = currentDirection * moveSpeed;

            if (patrolTimer >= 2f)
            {
                isPatrolling = false;
                rb.linearVelocity = Vector2.zero;
                patrolTimer = 0f;
                patrolCooldown = 1f;
            }
        }
        else
        {
            patrolCooldown -= Time.deltaTime;

            if (patrolCooldown <= 0f)
            {
                currentDirection = GetRandomDirection();
                isPatrolling = true;
                patrolTimer = 0f;

                if ((currentDirection.x > 0 && facingDir == -1) || (currentDirection.x < 0 && facingDir == 1))
                {
                    Flip();
                }
            }
        }
    }

    private Vector2 GetRandomDirection()
    {
        int dir = Random.Range(0, 4);
        switch (dir)
        {
            case 0: return Vector2.left;
            case 1: return Vector2.right;
            case 2: return Vector2.up;
            case 3: return Vector2.down;
            default: return Vector2.zero;
        }
    }

    private bool IsObstacleAhead()
    {
        Vector2 origin = (Vector2)transform.position + currentDirection * 0.1f;
        LayerMask obstacleMask = LayerMask.GetMask("Ground", "Obstacle");

        RaycastHit2D hit = Physics2D.Raycast(origin, currentDirection, 1.5f, obstacleMask);
        return hit.collider != null;
    }

    private bool IsInAnimation(string name)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    private string GetCurrentAnimStateName()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack")) return "Attack";
        if (stateInfo.IsName("Move")) return "Move";
        if (stateInfo.IsName("Idle")) return "Idle";
        if (stateInfo.IsName("Hit")) return "Hit";
        if (stateInfo.IsName("Death")) return "Death";
        return "";
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("IsHit");
        }
    }

    public void OnHit(int damage)
    {
        TakeDamage(damage);
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsDead", true);
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsAttacking", false);
        Destroy(gameObject, 2f);
    }

    public void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("IsAttacking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}