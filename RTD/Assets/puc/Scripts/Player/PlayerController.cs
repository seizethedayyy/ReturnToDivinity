using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Newtonsoft.Json;
using GameData;            // PlayerData 네임스페이스
using Player.States;       // 상태 클래스 네임스페이스
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // ==================================================
    // ① FSM 상태 인스턴스 레퍼런스
    // ==================================================
    private PlayerBaseState currentState;
    [HideInInspector] public PlayerIdleState IdleState;
    [HideInInspector] public PlayerMoveState MoveState;
    [HideInInspector] public PlayerAttackState AttackState;
    [HideInInspector] public PlayerDashState DashState;
    [HideInInspector] public PlayerHitState HitState;
    [HideInInspector] public PlayerDeadState DeadState;

    // ==================================================
    // ② 인스펙터 노출 변수들
    // ==================================================
    public enum CharacterType { Knight, Castle }
    [SerializeField] private CharacterType characterType;

    [Header("Player Data (Inspector에 ID 입력)")]
    [SerializeField] private string playerId; // JSON ID

    [SerializeField] private GameObject missileObject;      // 미사일 프리팹 (Inspector에 연결)
    [SerializeField] private Transform firePos;              // 미사일 발사 위치 (Inspector에 연결)

    [Header("Projectile Settings")]
    [Tooltip("미사일 발사 속도")]
    [SerializeField] private float projectileSpeed = 10f;

    [Header("디버그 메시지 UI")]
    [SerializeField] private float messageDuration = 2f;

    [Header("Dash Settings")]
    [Tooltip("대시 지속 시간")]
    public float DashDuration = 0.2f;
    [Tooltip("대시 쿨다운")]
    public float DashCooldown = 1.0f;
    [Tooltip("대시 속도")]
    public float DashSpeed = 10f;

    [Header("Combo Unlock Levels (Inspector에서 설정)")]
    [Tooltip("콤보2가 해금되는 최소 레벨")]
    public int combo2UnlockLevel = 2;
    [Tooltip("콤보3가 해금되는 최소 레벨")]
    public int combo3UnlockLevel = 3;
    [Tooltip("콤보4가 해금되는 최소 레벨")]
    public int combo4UnlockLevel = 4;

    [Header("변동 데이터")]
    [HideInInspector] public int currentLevel;
    [HideInInspector] public int currentExp;
    [HideInInspector] public int currentHp;
    [HideInInspector] public float currentFury = 0f;

    [Header("스탯 데이터")]
    [SerializeField] private PlayerData playerData;  // JSON으로 로드된 데이터

    // **외부(상태 클래스)에서 접근할 수 있도록 프로퍼티 추가**
    public PlayerData Data => playerData;

    [Header("공격 판정")]
    [SerializeField] private Transform attackCheck;       // 칼 휘두르는 오브젝트(Inspector에 연결)
    [SerializeField] private float attackOffsetX = 1.0f;
    [SerializeField] private float attackOffsetY = 0.0f;
    public LayerMask enemyLayer;                          // Enemy 레이어 (Inspector에 연결)

    public float knockbackForce = 5f;                     // 피격 시 넉백 힘
    public float HitFlashDuration = 0.1f;                  // 피격 시 깜빡임 지속 시간

    public float ComboDelay = 1.0f;                        // 콤보 종료 후 딜레이
    public float ComboInputBufferTime = 0.3f;              // 콤보 입력 버퍼 시간

    // ==================================================
    // ③ 내부 제어용 변수들
    // ==================================================
    private Coroutine messageCoroutine;
    private string systemMessage = "";
    private float systemMessageTimer = 0f;

    [HideInInspector] public bool IsDashing = false;
    [HideInInspector] public bool IsInvulnerable = false;
    [HideInInspector] public float NextDashTime;
    [HideInInspector] public float DashEndTime;

    [HideInInspector] public Animator Animator;
    [HideInInspector] public Rigidbody2D Rigidbody2D;
    [HideInInspector] public SpriteRenderer SpriteRenderer;

    [HideInInspector] public bool IsDead = false;
    [HideInInspector] public bool IsTakingDamage = false;

    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public bool IsAttacking = false;
    [HideInInspector] public bool HasPlayedSfx = false;

    [HideInInspector] public Coroutine FlashCoroutine;
    [HideInInspector] public Coroutine HitCoroutine;
    [HideInInspector] public Color OriginalColor;

    [HideInInspector] public bool IsComboCooldown = false;
    [HideInInspector] public int ComboStep = 0;
    [HideInInspector] public float ComboTimer = 0f;

    [HideInInspector] public bool HasQueuedThisPhase = false;
    [HideInInspector] public bool QueuedAttack = false;
    [HideInInspector] public float QueuedAttackTimer = 0f;

    [HideInInspector] public bool IsFurySkillTriggered = false;

    // ==================================================
    // ④ 외부 상태 클래스가 참조할 public 프로퍼티/필드들
    // ==================================================
    public CharacterType CurrentCharacterType => characterType;
    public GameObject MissileObject => missileObject;
    public Transform FirePos => firePos;
    public float CurrentFuryAmount => currentFury;

    /// <summary>
    /// 현재 레벨에 따라 몇 번째 콤보까지 해금되었는지 반환 (1~4 범위).
    /// </summary>
    public int MaxUnlockedCombo
    {
        get
        {
            if (currentLevel >= combo4UnlockLevel)
                return 4;
            if (currentLevel >= combo3UnlockLevel)
                return 3;
            if (currentLevel >= combo2UnlockLevel)
                return 2;
            return 1; // 콤보1은 항상 해금
        }
    }

    // ==================================================
    // ⑤ Awake(): 데이터 로드 및 상태 인스턴스 생성
    // ==================================================
    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        OriginalColor = SpriteRenderer.color;
        if (OriginalColor.a < 1f) OriginalColor.a = 1f;

        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        AttackState = new PlayerAttackState(this);
        DashState = new PlayerDashState(this);
        HitState = new PlayerHitState(this);
        DeadState = new PlayerDeadState(this);

        currentState = IdleState;

        StartCoroutine(InitializeAfterDataLoaded());
    }

    // 데이터 로드 후 UI와 상태 초기화를 위한 코루틴
    private IEnumerator InitializeAfterDataLoaded()
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[PlayerController] Inspector에서 playerId를 입력해주세요.");
            yield break;
        }

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

        playerData = PlayerDataLoader.GetPlayerDataById(playerId);
        if (playerData == null)
        {
            Debug.LogError($"[PlayerController] playerData == null! ID '{playerId}'에 해당하는 데이터가 없습니다.");
            yield break;
        }

        currentHp = playerData.maxHp;
        currentLevel = playerData.level;
        currentExp = 0;
        currentFury = 0f;
        Debug.Log($"[PlayerController] playerData 로드 성공: ID={playerId}, level={playerData.level}, maxHp={playerData.maxHp}");

        enemyLayer = LayerMask.GetMask("Enemy");

        // 데이터 로드 직후 UI 초기화
        InGameUIManager.Instance?.UpdateLevelText(currentLevel);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, playerData.maxHp);
        InGameUIManager.Instance?.UpdateExpUI(currentExp, playerData.exp);

        // 한 프레임 기다린 뒤 콤보 UI 잠금 상태 갱신
        yield return null;
        InGameUIManager.Instance?.UpdateComboLockUI(
            currentLevel,
            combo2UnlockLevel,
            combo3UnlockLevel,
            combo4UnlockLevel
        );
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

    // ==================================================
    // ⑥ Update(): 상태 실행 및 입력 처리
    // ==================================================
    private void Update()
    {
        if (currentState == DeadState)
            return;

        // 디버그용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnFurySkillEnd();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IsInvulnerable = !IsInvulnerable;
            ShowSystemMessage($"무적 : {(IsInvulnerable ? "On" : "Off")}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (playerData != null)
                TakeDamage(playerData.maxHp);
            ShowSystemMessage("즉시 사망 발동");
        }

        currentState.Execute();

        // 좌클릭 공격 입력 (Idle/Move 상태에서만)
        if (Input.GetMouseButtonDown(0) &&
            (currentState == IdleState || currentState == MoveState))
        {
            ComboStep = 0;  // 콤보가 항상 1부터 시작하도록 초기화
            SetState(AttackState);
        }

        // 우클릭 Fury 스킬 입력 (Idle/Move 상태에서만)
        if (Input.GetMouseButtonDown(1) &&
            (currentState == IdleState || currentState == MoveState))
        {
            if (IsFurySkillTriggered || playerData == null) return;
            int step = Mathf.FloorToInt(currentFury / playerData.furyMax);
            if (step >= 1)
            {
                IsFurySkillTriggered = true;
                Animator.SetTrigger("UseFury");
                StartCoroutine(FurySkillDelay(1.0f));
            }
        }

        // Dash 입력 처리 (Left Shift, Idle/Move 상태에서만)
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            (currentState == IdleState || currentState == MoveState))
        {
            if (Time.time >= NextDashTime)
            {
                SetState(DashState);
            }
        }
    }

    // ==================================================
    // ⑦ FixedUpdate(): 물리 이동 (Move/Dash 상태 관리)
    // ==================================================
    private void FixedUpdate()
    {
        // Dash 상태면 FixedUpdate에서 velocity를 건들지 않음
        if (currentState == DashState)
        {
            return;
        }

        // Hit 상태나 Dead 상태면 이동 속도 0
        if (currentState == HitState || currentState == DeadState)
        {
            Rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        // Move 상태일 때만 속도 세팅
        if (currentState == MoveState && playerData != null)
        {
            Vector2 velocity = MoveInput.normalized * playerData.moveSpeed;
            Rigidbody2D.linearVelocity = velocity;
        }
        else
        {
            Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    // ==================================================
    // ⑧ LateUpdate(): 공격 범위 및 발사 위치 유지
    // ==================================================
    private void LateUpdate()
    {
        if (characterType == CharacterType.Knight && attackCheck != null)
        {
            float direction = (SpriteRenderer.flipX ? -1f : 1f);
            Vector3 offset = new Vector3(attackOffsetX * direction, attackOffsetY, 0f);
            attackCheck.localPosition = offset;
        }

        if (firePos != null)
        {
            Vector3 fireScale = firePos.localScale;
            fireScale.x = (SpriteRenderer.flipX ? -Mathf.Abs(fireScale.x) : Mathf.Abs(fireScale.x));
            firePos.localScale = fireScale;
        }
    }

    // ==================================================
    // ⑨ 상태 전이 메서드 (Enter/Exit 호출 포함)
    // ==================================================
    public void SetState(PlayerBaseState newState)
    {
        if (currentState == newState)
        {
            currentState.Exit();
            Debug.Log($"[PlayerController] Re-enter State: {currentState.StateName}");
            currentState.Enter();
            return;
        }

        currentState.Exit();
        Debug.Log($"[PlayerController] State changed: {currentState.StateName} → {newState.StateName}");
        currentState = newState;
        currentState.Enter();
    }

    // 콤보 재시작용
    public void RestartAttackState()
    {
        if (currentState == AttackState)
        {
            currentState.Exit();
            Debug.Log($"[PlayerController] Re-enter AttackState");
            currentState.Enter();
        }
        else
        {
            SetState(AttackState);
        }
    }

    // ==================================================
    // ⑩ EndAttack(): 콤보 연결 및 해금 검사
    // ==================================================
    public void EndAttack()
    {
        Debug.Log($"[PlayerController] EndAttack() 호출 → ComboStep={ComboStep}, QueuedAttack={QueuedAttack}");

        HasQueuedThisPhase = false;
        IsAttacking = false;
        HasPlayedSfx = false;
        Animator.SetBool("IsAttacking", false);

        bool isMovingNow = MoveInput != Vector2.zero;
        Animator.SetBool("Move", isMovingNow);

        // 4콤보 이상 시 무조건 리셋
        if (ComboStep >= 4)
        {
            Debug.Log("[EndAttack] 4콤보 이상, 콤보 리셋");
            ComboStep = 0;
            QueuedAttack = false;
            QueuedAttackTimer = 0f;
            ComboTimer = 0f;
            InGameUIManager.Instance?.ResetComboSlot();
            StartCoroutine(ComboCooldownCoroutine());
            SetState(isMovingNow ? MoveState : IdleState);
            return;
        }

        // QueuedAttack이 true라면 다음 콤보 해금 여부 재확인
        if (QueuedAttack)
        {
            int nextComboIndex = ComboStep + 1;

            if (nextComboIndex > MaxUnlockedCombo)
            {
                Debug.Log($"[EndAttack] 시도된 콤보 {nextComboIndex} 잠김 → 콤보 종료");
                ComboStep = 0;
                QueuedAttack = false;
                QueuedAttackTimer = 0f;
                ComboTimer = 0f;
                InGameUIManager.Instance?.ResetComboSlot();
                SetState(isMovingNow ? MoveState : IdleState);
                return;
            }

            // 해금된 콤보라면 연속 재시작
            QueuedAttack = false;
            QueuedAttackTimer = 0f;
            ComboTimer = 0f;
            RestartAttackState();
            return;
        }
        else
        {
            // QueuedAttack=false → 콤보 종료
            Debug.Log("[EndAttack] queuedAttack=false → Idle/Move 전이");
            ComboTimer = 0f;
            ComboStep = 0;
            InGameUIManager.Instance?.ResetComboSlot();
            SetState(isMovingNow ? MoveState : IdleState);
        }
    }

    private IEnumerator ComboCooldownCoroutine()
    {
        IsComboCooldown = true;
        if (playerData != null)
            yield return new WaitForSeconds(playerData.comboCooldown);
        IsComboCooldown = false;
    }

    // ==================================================
    // ⑪ DoAttack(): 실제 적 타격 처리 & 미사일 발사 로직
    // ==================================================
    public void DoAttack()
    {
        if (attackCheck == null || playerData == null) return;

        // ─── 적에게 칼 데미지 처리 ───────────────────────────────────
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
            GainFury();
            // 경험치는 몬스터 사망 시점에 GainExp 호출로 처리됨
        }

        // ─── 미사일 발사 로직 (예시: 오른쪽 클릭 시) ─────────────────────────
        // (이 부분은 원래 프로젝트에서 “미사일 발사 키”나 “스킬 버튼”을 검사하던 코드가 있다면 그대로 넣어주세요.)
        // 예) 만약 FirePos와 missileObject가 모두 연결되어 있다면:
        if (Input.GetKeyDown(KeyCode.E)) // 예시: E 키를 누르면 미사일 발사
        {
            if (missileObject != null && firePos != null)
            {
                GameObject missile = Instantiate(missileObject, firePos.position, Quaternion.identity);
                // 예: 미사일 발사 방향 설정
                float dir = SpriteRenderer.flipX ? -1f : 1f;
                missile.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(dir * projectileSpeed, 0f);
                // … 기타 미사일 초기화 로직(데미지 값 설정 등)
            }
        }
    }

    // ==================================================
    // ⑫ GainFury(): Fury 게이지 처리
    // ==================================================
    public void GainFury()
    {
        if (playerData == null) return;
        currentFury += playerData.furyGainPerHit;
        currentFury = Mathf.Clamp(currentFury, 0f, playerData.furyMax);
        InGameUIManager.Instance?.UpdateFuryGauge(currentFury / playerData.furyMax);
    }

    // ==================================================
    // ⑬ GainExp(): 몬스터 사망 시 호출, 경험치 획득 처리
    // ==================================================
    public void GainExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[플레이어] {amount}만큼 경험치 획득 (현재: {currentExp}/{playerData.exp})");
        CheckAndHandleLevelUp();
        InGameUIManager.Instance?.UpdateExpUI(currentExp, playerData.exp);
    }

    // ==================================================
    // ⑭ CheckAndHandleLevelUp(): 레벨업 로직 & UI 갱신
    // ==================================================
    private void CheckAndHandleLevelUp()
    {
        if (playerData == null) return;

        if (currentExp >= playerData.exp)
        {
            string id = playerData.id;
            if (string.IsNullOrEmpty(id) || !id.Contains("_")) return;

            int underscoreIndex = id.LastIndexOf("_");
            string prefix = id.Substring(0, underscoreIndex);
            string numberStr = id.Substring(underscoreIndex + 1);

            if (!int.TryParse(numberStr, out int currentIdNum)) return;
            int nextIdNum = currentIdNum + 1;
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
                InGameUIManager.Instance?.UpdateLevelText(currentLevel);
                InGameUIManager.Instance?.UpdateExpUI(currentExp, playerData.exp);

                // 레벨업 후 콤보 잠금/해제 UI 갱신
                InGameUIManager.Instance?.UpdateComboLockUI(
                    currentLevel,
                    combo2UnlockLevel,
                    combo3UnlockLevel,
                    combo4UnlockLevel
                );
            }
            else
            {
                Debug.LogWarning($"[레벨업 실패] 다음 레벨 데이터 없음: {nextLevelId}");
            }
        }
    }

    // ==================================================
    // ⑮ OnFurySkillEnd(): Fury 스킬 종료 시 호출
    // ==================================================
    public void OnFurySkillEnd()
    {
        Debug.Log($"[검증] ▶ OnFurySkillEnd() 진입 / currentFury(before) = {currentFury}");
        IsFurySkillTriggered = false;
        currentFury = 0f;
        Debug.Log($"[검증] ▶ OnFurySkillEnd() 종료 후 currentFury = {currentFury}");
        InGameUIManager.Instance?.UpdateFuryGauge(0f);
    }

    private IEnumerator FurySkillDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsFurySkillTriggered = false;
        currentFury = 0f;
        InGameUIManager.Instance?.UpdateFuryGauge(0f);
    }

    // ==================================================
    // ⑯ TakeDamage(): 피격 처리
    // ==================================================
    public void TakeDamage(int damage)
    {
        if (IsInvulnerable || playerData == null) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, playerData.maxHp);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, playerData.maxHp);

        if (currentHp <= 0)
        {
            SetState(DeadState);
            return;
        }

        SetState(HitState);
    }

    // ==================================================
    // ⑰ ShowSystemMessage(): 디버그 메시지 출력
    // ==================================================
    private void ShowSystemMessage(string message)
    {
        systemMessage = message;
        systemMessageTimer = messageDuration;
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
}
