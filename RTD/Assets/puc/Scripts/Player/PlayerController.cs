using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using Newtonsoft.Json;
using GameData;  // PlayerData 네임스페이스
using Player.States;  // 상태 클래스 네임스페이스

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------
    // ① FSM 상태 인스턴스 레퍼런스
    // -------------------------------------------------------
    private PlayerBaseState currentState;
    [HideInInspector] public PlayerIdleState IdleState;
    [HideInInspector] public PlayerMoveState MoveState;
    [HideInInspector] public PlayerAttackState AttackState;
    [HideInInspector] public PlayerDashState DashState;
    [HideInInspector] public PlayerHitState HitState;
    [HideInInspector] public PlayerDeadState DeadState;

    // -------------------------------------------------------
    // ② 인스펙터 노출 변수들 (기존 그대로 유지)
    // -------------------------------------------------------
    public enum CharacterType { Knight, Castle }
    [SerializeField] private CharacterType characterType;

    [Header("Player Data (Inspector에 ID 입력)")]
    [SerializeField] private string playerId; // JSON ID

    [SerializeField] private GameObject missileObject;
    [SerializeField] private Transform firePos;

    [Header("디버그 메시지 UI")]
    [SerializeField] private float messageDuration = 2f;

    [Header("Dash Settings")]
    [Tooltip("대시 지속 시간")]
    public float DashDuration = 0.2f;
    [Tooltip("대시 쿨다운")]
    public float DashCooldown = 1.0f;
    [Tooltip("대시 속도")]
    public float DashSpeed = 10f;

    [Header("변동 데이터")]
    [SerializeField] private int currentLevel;
    [SerializeField] private int currentExp;
    [SerializeField] private int currentHp;
    [SerializeField] private float currentFury = 0f;

    [Header("스탯 데이터")]
    [SerializeField] private PlayerData playerData;  // JSON으로 로드된 데이터

    // **① 수정**: 외부 상태 클래스에서 읽기 전용으로 접근할 수 있도록 프로퍼티 추가
    public PlayerData PlayerData => playerData;

    [Header("공격 판정")]
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackOffsetX = 1.0f;
    [SerializeField] private float attackOffsetY = 0.0f;
    public LayerMask enemyLayer;

    public float knockbackForce = 5f;
    public float HitFlashDuration = 0.1f;

    public float ComboDelay = 1.0f;
    public float ComboInputBufferTime = 0.3f;

    // -------------------------------------------------------
    // ③ 내부 제어용 변수들 (기존 로직 그대로)
    // -------------------------------------------------------
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
    [HideInInspector] public bool IsFurySkillTriggered = false;
    [HideInInspector] public bool QueuedAttack = false;
    [HideInInspector] public float QueuedAttackTimer = 0f;

    // ==================================================
    // ④ 외부 상태(State) 클래스가 참조할 public 프로퍼티/필드들
    // ==================================================

    // 1) 캐릭터 타입: 상태 클래스에서 controller.CurrentCharacterType 으로 읽어올 수 있도록
    public CharacterType CurrentCharacterType => characterType;

    // 2) PlayerData: 상태 클래스에서 controller.Data(hitStunTime 등)를 읽기 위해
    public PlayerData Data => playerData;

    // 3) MissileObject, FirePos: 상태 클래스에서 “미사일 발사” 시 사용
    public GameObject MissileObject => missileObject;
    public Transform FirePos => firePos;

    // 4) CurrentFury: 상태 클래스에서 “미사일 데미지 계산” 시 사용
    public float CurrentFuryAmount => currentFury;

    // =======================================================
    // ④ Awake(): 데이터 로드 및 상태 인스턴스 생성
    // =======================================================
    private void Awake()
    {
        // (1) JSON 로드 대기 코루틴 실행
        StartCoroutine(InitializeAfterDataLoaded());

        // (2) 컴포넌트 참조
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        OriginalColor = SpriteRenderer.color;
        if (OriginalColor.a < 1f) OriginalColor.a = 1f;

        // (3) 상태 인스턴스 생성 (각 상태 클래스에 this 전달)
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        AttackState = new PlayerAttackState(this);
        DashState = new PlayerDashState(this);
        HitState = new PlayerHitState(this);
        DeadState = new PlayerDeadState(this);

        // (4) 초기 상태: Idle
        currentState = IdleState;
    }

    public void StartAttack()
    {
        // Debug.Log("[PlayerController] AnimationEvent StartAttack() 호출됨");
        DoAttack();
    }

    public void Dash()
    {
        // Debug.Log("[PlayerController] AnimationEvent Dash() 호출됨");
        // 기존 Anim Event 방식이 아니라, Update에서 직접 SetState(DashState)를 호출하므로
        // 이 메서드는 주석 처리해도 됩니다.
        // SetState(DashState);
    }

    public void OnDashEnd()
    {
        // Anim Event로 호출되던 부분을 사용하지 않습니다.
        // bool isMovingNow = MoveInput != Vector2.zero;
        // SetState(isMovingNow ? MoveState : IdleState);
    }

    /// <summary>
    /// JSON 데이터를 최대 5초간 대기 후 가져오는 코루틴
    /// </summary>
    private IEnumerator InitializeAfterDataLoaded()
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[PlayerController] Inspector에서 playerId를 입력해주세요.");
            yield break;
        }

        // 최대 5초 동안 JSON 로드 대기
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

        // JSON에서 playerData 가져오기
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

        // → 데이터 로드 직후, UI에 초기 레벨 표시
        InGameUIManager.Instance?.UpdateLevelText(currentLevel);
        InGameUIManager.Instance?.UpdateHpUI(currentHp, playerData.maxHp);
        InGameUIManager.Instance?.UpdateExpUI(currentExp, playerData.exp);

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

    // =======================================================
    // ⑤ Update(): 상태 실행 및 입력 처리
    // =======================================================
    private void Update()
    {
        // (1) Dead 상태면 입력을 완전히 무시
        if (currentState == DeadState)
            return;

        // (2) 디버그용 키 입력 (기존 로직 유지)
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

        // (3) 현재 상태의 Execute() 호출
        currentState.Execute();

        // (4) 좌클릭 공격 입력 (Idle/Move 상태에서만)
        if (Input.GetMouseButtonDown(0) &&
            (currentState == IdleState || currentState == MoveState))
        {
            if (IsComboCooldown) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.CompareTag("NPC")) return;

            SetState(AttackState);
        }

        // (5) 우클릭 Fury 스킬 입력 (Idle/Move 상태에서만)
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

        // (6) Dash 입력 처리 (Left Shift 키를 누를 때, Idle/Move 상태에서만)
        if (Input.GetKeyDown(KeyCode.LeftShift) && (currentState == IdleState || currentState == MoveState))
            SetState(DashState);
        {
            if (Time.time >= NextDashTime)
            {
                SetState(DashState);
            }
        }
    }

    // =======================================================
    // ⑥ FixedUpdate(): 물리 이동 (Move 상태에서만)
    // =======================================================
    private void FixedUpdate()
    {
        // 1) Dash 상태면 여기에서 아무것도 하지 않고 바로 반환 → DashLoop()가 velocity를 직접 설정하도록 함
        if (currentState == DashState)
        {
            return;
        }

        // 2) Hit 상태나 Dead 상태면 이동 속도 0
        if (currentState == HitState || currentState == DeadState)
        {
            Rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        // 3) Move 상태일 때만 속도 세팅
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

    // =======================================================
    // ⑦ LateUpdate(): 공격 범위 및 발사 위치 유지
    // =======================================================
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

    // =======================================================
    // ⑧ 상태 전이 메서드 (Enter/Exit 호출 포함)
    // =======================================================
    public void SetState(PlayerBaseState newState)
    {
        if (currentState == newState)
        {
            // 1) 현재 상태의 Exit() 호출
            currentState.Exit();

            // 2) 디버그 로그 (원한다면 상태 이름이 같은 경우에도 찍어 줍니다)
            Debug.Log($"[PlayerController] Re-enter State: {currentState.StateName}");

            // 3) 다시 Enter() 호출
            currentState.Enter();
            return;
        }

        currentState.Exit();
        Debug.Log($"[PlayerController] State changed: {currentState.StateName} → {newState.StateName}");
        currentState = newState;
        currentState.Enter();
    }

    // =======================================================
    // ⑨ 공격 관련 메서드 (EndAttack, TakeDamage, LevelUp 등)
    //     – 기존 PlayerController 로직에서 복사·유지
    // =======================================================
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

    public void EndAttack()
    {
        Debug.Log($"[PlayerController] EndAttack() 호출 → ComboStep={ComboStep}, QueuedAttack={QueuedAttack}, MoveInput={MoveInput}");

        HasQueuedThisPhase = false;
        IsAttacking = false;
        HasPlayedSfx = false;
        Animator.SetBool("IsAttacking", false);

        bool isMovingNow = MoveInput != Vector2.zero;
        Animator.SetBool("Move", isMovingNow);

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

        if (QueuedAttack)
        {
            QueuedAttack = false;
            QueuedAttackTimer = 0f;
            ComboTimer = 0f;
            RestartAttackState();
            return;
        }
        else
        {
            if (Data != null && CurrentCharacterType == PlayerController.CharacterType.Castle && ComboStep < 4)
            {
                Debug.Log("[EndAttack] Castle 타입 4콤보 미만, 애니메이션만 유지");
                return;
            }
            Debug.Log("[EndAttack] queuedAttack=false 이므로 Idle/Move로 전이");
            ComboTimer = 0f;
            ComboStep = 0;
            InGameUIManager.Instance?.ResetComboSlot();
            SetState(MoveInput != Vector2.zero ? MoveState : IdleState);
        }
    }

    private IEnumerator ComboCooldownCoroutine()
    {
        IsComboCooldown = true;
        if (playerData != null)
            yield return new WaitForSeconds(playerData.comboCooldown);
        IsComboCooldown = false;
    }

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
            GainFury();
        }
    }

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
            }
            else
            {
                Debug.LogWarning($"[레벨업 실패] 다음 레벨 데이터 없음: {nextLevelId}");
            }
        }
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[플레이어] {amount}만큼 경험치 획득 (현재: {currentExp}/{playerData.exp})");
        CheckAndHandleLevelUp();

        //경험치 변화마다 UI 갱신
        InGameUIManager.Instance?.UpdateExpUI(currentExp, playerData.exp);

    }

    // =======================================================
    // ⑫ 사망, Hit 상태 처리 후 Remove 루틴은 각 상태 클래스 내부에 구현되어 있으므로,
    //     PlayerController 내에서는 별도 Die()/EndHitAfter() 호출 불필요
    // =======================================================

    // =======================================================
    // ⑬ 시스템 메시지 출력 (OnGUI)
    // =======================================================
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
