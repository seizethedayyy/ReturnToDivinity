using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [Header("스탯 데이터")]
    [SerializeField] private CharacterStats characterStats;

    private int currentHp;
    private float currentFury = 0f;

    private Vector2 moveInput;
    private bool isAttacking = false;
    private bool isHit = false;
    private bool isTakingDamage = false;
    private bool hasPlayedSfx = false;

    private Coroutine flashCoroutine;
    private Coroutine hitCoroutine;
    private Color originalColor;

    [Header("공격 판정")]
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackOffsetX = 1.0f;
    [SerializeField] private float attackOffsetY = 0.0f;
    public LayerMask enemyLayer;

    public float knockbackForce = 5f;
    public float hitFlashDuration = 0.1f;

    private bool isComboCooldown = false;
    private int comboStep = 0;
    private float comboTimer = 0f;
    public float comboDelay = 1.0f;

    private bool hasQueuedThisPhase = false;
    private bool queuedAttack = false;
    private float queuedAttackTimer = 0f;
    public float inputBufferTime = 0.3f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHp = characterStats.maxHp;
        originalColor = spriteRenderer.color;
        if (originalColor.a < 1f) originalColor.a = 1f;

        enemyLayer = LayerMask.GetMask("Enemy");
    }

    private void Start()
    {
        Debug.Log($"[Player] 위치 초기화: {transform.position} / 레이어: {gameObject.layer}");
        StartCoroutine(InitializeHpUI());
        InGameUIManager.Instance?.ApplyCharacterInfo(SelectedCharacterData.Instance.selectedCharacter);

        currentFury = 0f;
        InGameUIManager.Instance?.UpdateFuryGauge(0f);
    }

    private IEnumerator InitializeHpUI()
    {
        yield return new WaitUntil(() =>
            InGameUIManager.Instance != null &&
            InGameUIManager.Instance.hpText != null &&
            InGameUIManager.Instance.hpBarFillImage != null
        );

        InGameUIManager.Instance.UpdateHpUI(currentHp, characterStats.maxHp);
    }

    void ForceColliderWake()
    {
        rb.WakeUp();
        rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (isTakingDamage) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("Move", isMoving && !isAttacking);

        if (moveInput.x != 0)
            spriteRenderer.flipX = moveInput.x < 0;

        if (comboStep > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer > comboDelay && !isAttacking)
            {
                comboStep = 0;
                comboTimer = 0f;
                InGameUIManager.Instance?.ResetComboSlot();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isComboCooldown) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.CompareTag("NPC"))
                return;

            if (!isAttacking)
            {
                StartAttack();
            }
            else if (!hasQueuedThisPhase)
            {
                queuedAttackTimer = inputBufferTime;
                hasQueuedThisPhase = true;
            }
        }

        if (queuedAttackTimer > 0f)
        {
            queuedAttackTimer -= Time.deltaTime;
            if (!queuedAttack && queuedAttackTimer > 0f)
            {
                queuedAttack = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isTakingDamage)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!isAttacking)
        {
            Vector2 moveDelta = moveInput.normalized * characterStats.moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveDelta);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        if (attackCheck != null)
        {
            float direction = spriteRenderer.flipX ? -1f : 1f;
            Vector3 offset = new Vector3(attackOffsetX * direction, attackOffsetY, 0f);
            attackCheck.localPosition = offset;
        }
    }

    private void StartAttack()
    {
        if (isAttacking) return;

        CancelInvoke(nameof(EndAttack));

        comboStep = comboStep switch
        {
            0 => 1,
            1 when comboTimer <= comboDelay => 2,
            2 when comboTimer <= comboDelay => 3,
            3 when comboTimer <= comboDelay => 4,
            _ => 1
        };

        // ✅ 콤보 슬롯 점등
        InGameUIManager.Instance?.UpdateComboSlot(comboStep);

        isAttacking = true;

        string animName = comboStep switch
        {
            1 => "Attack1",
            2 => "Attack2",
            3 => "Attack3",
            4 => "Attack4",
            _ => "Attack1"
        };

        if (animator.HasState(0, Animator.StringToHash(animName)))
        {
            animator.Play(animName, 0);
        }

        if (!hasPlayedSfx)
        {
            AudioManager.Instance?.PlaySfx("testSfx");
            hasPlayedSfx = true;
        }

        Invoke(nameof(EndAttack), characterStats.attackSpeed > 0 ? 1f / characterStats.attackSpeed : 0.6f);
    }

    public void EndAttack()
    {
        hasQueuedThisPhase = false;
        isAttacking = false;
        hasPlayedSfx = false;
        animator.SetBool("IsAttacking", false);

        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("Move", isMoving);
                

        if (comboStep >= 4)
        {
            comboStep = 0;
            queuedAttack = false;
            queuedAttackTimer = 0f;
            comboTimer = 0f;

            InGameUIManager.Instance?.ResetComboSlot(); // ✅ 콤보 종료 시 초기화
            StartCoroutine(ComboCooldownCoroutine());
            return;
        }

        if (queuedAttack)
        {
            queuedAttack = false;
            queuedAttackTimer = 0f;
            comboTimer = 0f;
            StartAttack();
        }
        else
        {
            comboTimer = 0f;
            comboStep = 0;

            InGameUIManager.Instance?.ResetComboSlot(); // ✅ 콤보 종료 시 초기화
        }
    }

    private void GainFury()
    {
        currentFury += characterStats.furyGainPerHit;
        currentFury = Mathf.Clamp(currentFury, 0f, characterStats.furyMax);

        float percent = currentFury / characterStats.furyMax;
        InGameUIManager.Instance?.UpdateFuryGauge(percent);
    }

    private IEnumerator ComboCooldownCoroutine()
    {
        isComboCooldown = true;
        yield return new WaitForSeconds(characterStats.comboCooldown);
        isComboCooldown = false;
    }

    public void DoAttack()
    {
        if (attackCheck == null) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackCheck.position, characterStats.attackRange, enemyLayer);

        bool didHit = false;

        foreach (Collider2D col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent(out EnemyController enemy))
            {
                enemy.OnHit((int)characterStats.attackDamage);
                didHit = true; 
            }
        }

        if (didHit)
        {
            GainFury();
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[Player] 데미지 {damage} 받음!");
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, characterStats.maxHp);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, characterStats.maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void PrepareHitEffect()
    {
        animator.Play("Hit", 0, 0f);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRed());

        isTakingDamage = true;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(EndHitAfter(characterStats.hitStunTime));
    }

    private IEnumerator EndHitAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTakingDamage = false;
        hitCoroutine = null;
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("[Player] 사망 처리");
    }

    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => characterStats.maxHp;

    private void OnDrawGizmosSelected()
    {
        if (attackCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCheck.position, characterStats.attackRange);
    }
}
