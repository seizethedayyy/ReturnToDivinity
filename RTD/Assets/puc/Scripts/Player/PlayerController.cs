// -------------------------------------------------------
// 파일 경로: Assets/puc/Scripts/Player/PlayerController.cs
// -------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using GameData;  // PlayerData를 참조하기 위해 네임스페이스 추가

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public enum CharacterType { Knight, Castle }
    [SerializeField] private CharacterType characterType;

    [Header("Player Data (Inspector에 ID 입력)")]
    [SerializeField] private string playerId; // 인스펙터에서 JSON ID(예: "knight_01")를 직접 입력

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

    [Header("변동 데이터")]
    [SerializeField] private int currentLevel;
    [SerializeField] private int currentExp;
    [SerializeField] private int currentHp;
    [SerializeField] private float currentFury = 0f;

    [Header("스탯 데이터")]
    [SerializeField]
    private PlayerData playerData;  // 인스펙터에 노출. GameData.PlayerData 타입

    private bool isDead = false;

    private Vector2 moveInput;
    private bool isAttacking = false;
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

    // Awake()에서는 아직 LoadedPlayerData가 준비되지 않았을 수 있으므로, 코루틴으로 대기합니다.
    private void Awake()
    {
        StartCoroutine(InitializeAfterDataLoaded());
    }

    /// <summary>
    /// PlayerDataLoader가 JSON을 모두 불러올 때까지 최대 5초간 대기 후,
    /// playerData를 가져와 변동 데이터를 초기화합니다.
    /// </summary>
    private IEnumerator InitializeAfterDataLoaded()
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[PlayerController] Inspector에서 playerId를 입력해주세요.");
            yield break;
        }

        // 최대 5초 동안 LoadedPlayerData가 준비되길 대기
        float timeout = 5f;
        while (PlayerDataLoader.LoadedPlayerData == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (PlayerDataLoader.LoadedPlayerData == null)
        {
            Debug.LogError("[PlayerController] 5초 내에 PlayerDataLoader가 데이터를 로드하지 못했습니다!");
            yield break;
        }

        // 이제 JSON이 파싱되어 LoadedPlayerData에 값이 들어 있음
        playerData = PlayerDataLoader.GetPlayerDataById(playerId);
        if (playerData == null)
        {
            Debug.LogError($"[PlayerController] playerData == null! ID '{playerId}'에 해당하는 데이터가 없습니다.");
            yield break;
        }

        // 변동 데이터 초기화
        currentHp = playerData.maxHp;
        currentLevel = playerData.level;
        currentExp = playerData.exp;
        currentFury = 0f;
        Debug.Log($"[PlayerController] playerData 로드 성공: ID={playerId}, level={playerData.level}, maxHp={playerData.maxHp}");

        // 컴포넌트 참조
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (originalColor.a < 1f) originalColor.a = 1f;
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    private void Start()
    {
        StartCoroutine(InitializeHpUI());
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
        if (playerData != null)
            InGameUIManager.Instance.UpdateHpUI(currentHp, playerData.maxHp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnFurySkillEnd();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isInvulnerable = !isInvulnerable;
            ShowSystemMessage($"무적 : {(isInvulnerable ? "On" : "Off")}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (playerData != null)
                TakeDamage(playerData.maxHp);
            ShowSystemMessage("즉시 사망 발동");
        }

        if (isDead) return;

        // 대시 시작
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
        // 대시 종료
        if (isDashing && (!Input.GetKey(KeyCode.LeftShift) || Time.time >= dashEndTime))
        {
            EndDash();
        }

        if (isDashing || isTakingDamage) return;

        // 이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        bool isMoving = (moveInput != Vector2.zero);
        animator.SetBool("Move", isMoving && !isAttacking);

        if (moveInput.x != 0)
            spriteRenderer.flipX = (moveInput.x < 0);

        // 콤보 타이머
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

        // 마우스 왼쪽 클릭: 공격 시작 or 큐잉
        if (Input.GetMouseButtonDown(0))
        {
            if (isComboCooldown) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.CompareTag("NPC")) return;

            if (!isAttacking)
                StartAttack();
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
                queuedAttack = true;
        }

        // 마우스 오른쪽 클릭: Fury 스킬 사용
        if (Input.GetMouseButtonDown(1))
        {
            if (isFurySkillTriggered || playerData == null) return;
            int step = Mathf.FloorToInt(currentFury / playerData.furyMax);
            if (step >= 1)
            {
                isFurySkillTriggered = true;
                animator.SetTrigger("UseFury");
                StartCoroutine(FurySkillDelay(1.0f));
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
        if (playerData == null) yield break;

        float dashSpeed = playerData.moveSpeed * 4f;
        Vector2 dashDir = (moveInput == Vector2.zero)
            ? (spriteRenderer.flipX ? Vector2.left : Vector2.right)
            : moveInput.normalized;

        while (isDashing)
        {
            // Unity6: rb.linearVelocity 사용
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
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerData != null && !isAttacking)
        {
            // Unity6: 이동 시에도 rb.linearVelocity 사용
            Vector2 velocity = moveInput.normalized * playerData.moveSpeed;
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        if (characterType == CharacterType.Knight && attackCheck != null)
        {
            float direction = (spriteRenderer.flipX ? -1f : 1f);
            Vector3 offset = new Vector3(attackOffsetX * direction, attackOffsetY, 0f);
            attackCheck.localPosition = offset;
        }

        if (firePos != null)
        {
            Vector3 fireScale = firePos.localScale;
            fireScale.x = (spriteRenderer.flipX ? -Mathf.Abs(fireScale.x) : Mathf.Abs(fireScale.x));
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

        string animName = (characterType == CharacterType.Castle) ? "Shoot" : $"Attack{comboStep}";
        if (animator.HasState(0, Animator.StringToHash(animName)))
            animator.Play(animName, 0);

        if (!hasPlayedSfx)
            AudioManager.Instance?.PlaySfx("attack_sfx");
        hasPlayedSfx = true;

        if (playerData != null && characterType == CharacterType.Castle)
        {
            animator.SetBool("IsAttacking", true);
            float delay = (playerData.attackSpeed > 0) ? 1f / playerData.attackSpeed : 0.6f;
            StartCoroutine(DelayedShootAndEnd(delay));
        }
        else if (playerData != null)
        {
            float delay = (playerData.attackSpeed > 0) ? 1f / playerData.attackSpeed : 0.6f;
            Invoke(nameof(EndAttack), delay);
        }
    }

    private IEnumerator DelayedShootAndEnd(float delay)
    {
        yield return new WaitForSeconds(delay);

        ShootMissile();

        if (comboStep < 4)
            EndAttack();
        else
        {
            yield return new WaitForSeconds(0.2f);
            EndAttack();
        }
    }

    private void ShootMissile()
    {
        if (missileObject == null || firePos == null || playerData == null) return;

        missileObject.transform.position = firePos.position;
        Vector2 dir = (spriteRenderer.flipX ? Vector2.left : Vector2.right);

        Missile missile = missileObject.GetComponent<Missile>();
        missile.Init(dir, GetMissileDamage());

        SpriteRenderer missileRend = missileObject.GetComponent<SpriteRenderer>();
        if (missileRend != null)
            missileRend.flipX = spriteRenderer.flipX;

        missileObject.SetActive(true);
    }

    private int GetMissileDamage()
    {
        if (playerData == null) return 0;

        float gaugePercent = currentFury / playerData.furyMax;
        if (gaugePercent >= 3f) return Mathf.FloorToInt(playerData.attackDamage * 3f);
        if (gaugePercent >= 2f) return Mathf.FloorToInt(playerData.attackDamage * 2f);
        return Mathf.FloorToInt(playerData.attackDamage);
    }

    public void EndAttack()
    {
        hasQueuedThisPhase = false;
        isAttacking = false;
        hasPlayedSfx = false;
        animator.SetBool("IsAttacking", false);

        bool isMoving = (moveInput != Vector2.zero);
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
            if (playerData != null && characterType == CharacterType.Castle && comboStep < 4)
                return;

            comboTimer = 0f;
            comboStep = 0;
            InGameUIManager.Instance?.ResetComboSlot();
        }
    }

    private IEnumerator ComboCooldownCoroutine()
    {
        isComboCooldown = true;
        if (playerData != null)
            yield return new WaitForSeconds(playerData.comboCooldown);
        isComboCooldown = false;
    }

    /// <summary>
    /// 공격 판정 (DoAttack)는 애니메이터에서 호출됩니다.
    /// 적을 타격하면 경험치 +1, 레벨업 체크, Fury 증가
    /// </summary>
    public void DoAttack()
    {
        if (attackCheck == null || playerData == null) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackCheck.position, playerData.attackRange, enemyLayer);

        bool didHit = false;
        foreach (Collider2D col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent(out EnemyController enemy))
            {
                enemy.OnHit((int)playerData.attackDamage);
                didHit = true;
            }
        }

        if (didHit)
        {
            // 경험치 1 증가 예시
            currentExp += 1;
            CheckAndHandleLevelUp();

            GainFury();
        }
    }

    /// <summary>
    /// Fury 획득 (적 타격 시 호출)
    /// </summary>
    public void GainFury()
    {
        if (playerData == null) return;

        currentFury += playerData.furyGainPerHit;
        currentFury = Mathf.Clamp(currentFury, 0f, playerData.furyMax);
        float percent = currentFury / playerData.furyMax;
        InGameUIManager.Instance?.UpdateFuryGauge(percent);
    }

    public void OnFurySkillEnd()
    {
        Debug.Log($"[검증] ▶ OnFurySkillEnd() 진입 / currentFury(before) = {currentFury}");
        isFurySkillTriggered = false;
        currentFury = 0f;
        Debug.Log($"[검증] ▶ OnFurySkillEnd() 종료 후 currentFury = {currentFury}");
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.UpdateFuryGauge(0f);
        else
            Debug.LogWarning("[검증] ▶ InGameUIManager.Instance == null");
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || playerData == null) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, playerData.maxHp);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, playerData.maxHp);

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
        hitCoroutine = StartCoroutine(EndHitAfter(playerData.hitStunTime));
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
        InGameUIManager.Instance?.ShowGameOverAndReturnToTitle(2.5f);
        StartCoroutine(RemoveAfterDeath());
    }

    private IEnumerator RemoveAfterDeath()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
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
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.UpperCenter
            };
            Rect rect = new Rect(0, 20, Screen.width, 50);
            GUI.Label(rect, systemMessage, style);
        }
    }

    /// <summary>
    /// 경험치를 확인하여 레벨업 처리
    /// </summary>
    private void CheckAndHandleLevelUp()
    {
        if (playerData == null) return;

        if (currentExp >= playerData.exp)
        {
            string id = playerData.id;
            if (string.IsNullOrEmpty(id) || !id.Contains("_"))
                return;

            int underscoreIndex = id.LastIndexOf("_");
            string prefix = id.Substring(0, underscoreIndex);
            string numberStr = id.Substring(underscoreIndex + 1);

            if (!int.TryParse(numberStr, out int currentIdNum))
                return;

            int nextIdNum = currentIdNum + 1;
            // 두 자리 포맷 예: "01", "02", ...
            string nextLevelId = $"{prefix}_{nextIdNum:D2}";

            PlayerData nextData = PlayerDataLoader.GetPlayerDataById(nextLevelId);
            if (nextData != null)
            {
                Debug.Log($"[레벨업] {playerData.id} → {nextData.id}");
                playerData = nextData;
                currentLevel = playerData.level;
                currentExp = 0;
                currentHp = playerData.maxHp;
                InGameUIManager.Instance?.UpdateHpUI(currentHp, playerData.maxHp);
            }
            else
            {
                Debug.LogWarning($"[레벨업 실패] 다음 레벨 데이터 없음: {nextLevelId}");
            }
        }
    }
}
