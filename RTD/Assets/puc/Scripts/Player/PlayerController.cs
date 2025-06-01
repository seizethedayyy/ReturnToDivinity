using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System.Reflection;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class PlayerController : MonoBehaviour
{
    public enum CharacterType { Knight, Castle }
    [SerializeField] private CharacterType characterType;

    [SerializeField] private GameObject missileObject;
    [SerializeField] private Transform firePos;

    [Header("디버그 메시지 UI")]    
    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageCoroutine;

    private string systemMessage = "";
    private float systemMessageTimer = 0f;
    private readonly float systemMessageDuration = 2f;

    [Header("Dash Settings")]
    [Tooltip("대시 지속 시간")]
    public float dashDuration = 0.2f;
    [Tooltip("대시 쿨다운")]
    public float dashCooldown = 1.0f;

    private bool isDashing;
    private bool isInvulnerable;
    private float nextDashTime;
    private float dashEndTime;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [Header("스탯 데이터")]
    [SerializeField] private CharacterStats characterStats;

    [SerializeField] private int currentHp;
    [SerializeField] private float currentFury = 0f;

    private bool isDead = false;

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
    private bool isFurySkillTriggered = false;
    private bool queuedAttack = false;
    private float queuedAttackTimer = 0f;
    public float inputBufferTime = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        currentHp = characterStats.maxHp;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnFurySkillEnd();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isInvulnerable = !isInvulnerable;
            ShowSystemMessage($"무적 : {(isInvulnerable ? "On" : "Off")}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TakeDamage(characterStats.maxHp);
            ShowSystemMessage("즉시 사망 발동");
        }

        if (isDead) return;

        if (!isDashing && Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime)
        {
            isDashing = true;
            isInvulnerable = true;
            nextDashTime = Time.time + dashCooldown;
            dashEndTime = Time.time + dashDuration;
            animator.SetBool("IsDashing", true);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            StartCoroutine(DashLoop());
        }

        if (isDashing && (!Input.GetKey(KeyCode.LeftShift) || Time.time >= dashEndTime))
        {
            EndDash();
        }

        if (isDashing || isTakingDamage) return;

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

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.CompareTag("NPC")) return;

            if (!isAttacking) StartAttack();
            else if (!hasQueuedThisPhase)
            {
                queuedAttackTimer = inputBufferTime;
                hasQueuedThisPhase = true;
            }
        }

        if (queuedAttackTimer > 0f)
        {
            queuedAttackTimer -= Time.deltaTime;
            if (!queuedAttack && queuedAttackTimer > 0f) queuedAttack = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (isFurySkillTriggered) return;

            int step = Mathf.FloorToInt(currentFury / characterStats.furyMax); // 1~3
            if (step >= 1)
            {
                isFurySkillTriggered = true;
                animator.SetTrigger("UseFury");

                // 애니메이션이 끝날 시간만큼 딜레이 후 FuryGauge 초기화
                StartCoroutine(FurySkillDelay(1.0f)); // 길이는 애니메이션 클립 길이에 맞춰 조정
            }
        }
    }

    private IEnumerator FurySkillDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isFurySkillTriggered = false;
        currentFury = 0f;
        InGameUIManager.Instance?.UpdateFuryGauge(0f);
    }

    private IEnumerator DashLoop()
    {
        float dashSpeed = characterStats.moveSpeed * 4f;
        Vector2 dashDir = moveInput == Vector2.zero ? (spriteRenderer.flipX ? Vector2.left : Vector2.right) : moveInput.normalized;

        while (isDashing)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            yield return null;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        isInvulnerable = false;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = originalColor;
        animator.SetBool("IsDashing", false);
    }

    private void FixedUpdate()
    {
        if (isDead || isTakingDamage || isDashing)
        {
            return;
        }

        if (!isAttacking)
        {
            Vector2 moveDelta = moveInput.normalized * characterStats.moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveDelta);
        }
    }

    private void LateUpdate()
    {
        if (characterType == CharacterType.Knight && attackCheck != null)
        {
            float direction = spriteRenderer.flipX ? -1f : 1f;
            Vector3 offset = new Vector3(attackOffsetX * direction, attackOffsetY, 0f);
            attackCheck.localPosition = offset;
        }

        if (firePos != null)
        {
            // Flip 방향에 따라 위치와 회전 조정
            Vector3 fireScale = firePos.localScale;
            fireScale.x = spriteRenderer.flipX ? -Mathf.Abs(fireScale.x) : Mathf.Abs(fireScale.x);
            firePos.localScale = fireScale;
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

        InGameUIManager.Instance?.UpdateComboSlot(comboStep);
        isAttacking = true;

        string animName = characterType == CharacterType.Castle ? "Shoot" : $"Attack{comboStep}";
        if (animator.HasState(0, Animator.StringToHash(animName)))
            animator.Play(animName, 0);

        if (!hasPlayedSfx)
        {
            AudioManager.Instance?.PlaySfx("testSfx");
            hasPlayedSfx = true;
        }

        if (characterType == CharacterType.Castle)
        {
            animator.SetBool("IsAttacking", true);

            // 미사일 발사를 약간 지연 (애니메이션에 맞게 설정)
            float delay = characterStats.attackSpeed > 0 ? 1f / characterStats.attackSpeed : 0.6f;
            StartCoroutine(DelayedShootAndEnd(delay));
        }
        else
        {
            // Knight는 항상 경직
            Invoke(nameof(EndAttack), characterStats.attackSpeed > 0 ? 1f / characterStats.attackSpeed : 0.6f);
        }
    }

    private IEnumerator DelayEndAttackAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndAttack();
    }

    private IEnumerator DelayedShootAndEnd(float delay)
    {
        yield return new WaitForSeconds(delay);

        ShootMissile();

        if (comboStep < 4)
        {
            EndAttack(); // 즉시 이동 가능
        }
        else
        {
            // 4타는 경직 필요 → 약간 더 지연 주거나 현재 상태 유지
            yield return new WaitForSeconds(0.2f);
            EndAttack();
        }
    }

    private void ShootMissile()
    {
        if (missileObject == null || firePos == null) return;

        missileObject.transform.position = firePos.position;

        // 방향 설정
        Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // Missile 스크립트 초기화
        Missile missile = missileObject.GetComponent<Missile>();
        missile.Init(dir, GetMissileDamage());

        // Flip 방향에 따라 좌우 반전 (Sprite 기준)
        SpriteRenderer missileRenderer = missileObject.GetComponent<SpriteRenderer>();
        if (missileRenderer != null)
            missileRenderer.flipX = spriteRenderer.flipX;

        missileObject.SetActive(true);
    }

    private int GetMissileDamage()
    {
        float gaugePercent = currentFury / characterStats.furyMax;

        if (gaugePercent >= 3f)
            return (int)(characterStats.attackDamage * 3f);
        if (gaugePercent >= 2f)
            return (int)(characterStats.attackDamage * 2f);
        return (int)characterStats.attackDamage;
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

            InGameUIManager.Instance?.ResetComboSlot();
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
            // ❗ Castle일 경우 comboStep을 유지 (단, comboTimer는 유지)
            if (characterType == CharacterType.Castle && comboStep < 4)
            {
                // 다음 공격 대기 상태로 유지
                return;
            }

            // 그 외에는 초기화
            comboTimer = 0f;
            comboStep = 0;
            InGameUIManager.Instance?.ResetComboSlot();
        }
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

    public void GainFury()
    {
        currentFury += characterStats.furyGainPerHit;
        currentFury = Mathf.Clamp(currentFury, 0f, characterStats.furyMax);

        float percent = currentFury / characterStats.furyMax;
        InGameUIManager.Instance?.UpdateFuryGauge(percent);
    }

    public void OnFurySkillEnd()
    {
        Debug.Log("[검증] ▶ OnFurySkillEnd() 진입 / currentFury(before) = " + currentFury);
        isFurySkillTriggered = false;
        currentFury = 0f;
        Debug.Log("[검증] ▶ OnFurySkillEnd() 종료 후 currentFury = " + currentFury);

        if (InGameUIManager.Instance != null)
        {
            Debug.Log("[검증] ▶ UIManager.UpdateFuryGauge(0) 호출");
            InGameUIManager.Instance.UpdateFuryGauge(0f);
        }
        else
        {
            Debug.LogWarning("[검증] ▶ InGameUIManager.Instance == null");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, characterStats.maxHp);

        InGameUIManager.Instance?.UpdateHpUI(currentHp, characterStats.maxHp);

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        PrepareHitEffect();
    }

    public void PrepareHitEffect()
    {
        animator.SetTrigger("IsHit");

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRed());

        isTakingDamage = true;

        if (hitCoroutine != null) StopCoroutine(hitCoroutine);
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
    if (isDead) return;

    isDead = true;
    isDashing = false;
    isAttacking = false;
    isTakingDamage = false;

    rb.linearVelocity = Vector2.zero;
    spriteRenderer.color = originalColor;

    animator.SetBool("IsDashing", false);
    animator.SetBool("Move", false);
    animator.SetTrigger("Die");

    Debug.Log("[Player] ▶ 사망 처리 완료");

    // 게임 오버 연출 및 씬 전환
    InGameUIManager.Instance?.ShowGameOverAndReturnToTitle(2.5f); // 2.5초 뒤 Title로

    StartCoroutine(RemoveAfterDeath());
}

    private IEnumerator RemoveAfterDeath()
    {
        yield return new WaitForSeconds(1.5f); // 애니메이션 길이만큼
        gameObject.SetActive(false); // 또는 Destroy(gameObject);
    }

    private void ShowSystemMessage(string message)
    {
        systemMessage = message;
        systemMessageTimer = systemMessageDuration;
    }

    private void OnGUI()
    {
        if (systemMessageTimer > 0f)
        {
            systemMessageTimer -= Time.deltaTime;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 32;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.UpperCenter;

            Rect rect = new Rect(0, 20, Screen.width, 50);
            GUI.Label(rect, systemMessage, style);
        }
    }
        
}
