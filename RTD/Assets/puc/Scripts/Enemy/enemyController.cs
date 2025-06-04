using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyController : EnemyBase
{
    [Header("🆔 몬스터 ID (Google Sheets 기준)")]
    public string enemyId;

    [Header("📊 Stats (실시간 표시용)")]
    [SerializeField] private float enemyCurrentHp; // 인스펙터 표시용 변수

    [Header("📺 UI 컴포넌트")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private Image hpBarFill;

    private string id;
    private string enemyName;
    private int level;
    private float maxHp;
    private float moveSpeed;
    private float detectionRange;
    private float attackRadius;
    private float attackDamage;
    private float chaseStopRange;
    private float stopDistance;
    private float angleThreshold;
    private float attackCooldown;
    private int exp;
    private int gold;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        while (EnemyDataLoader.LoadedEnemyData == null)
            yield return null;

        while (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            yield return null;
        }

        var data = EnemyDataLoader.GetEnemyDataById(enemyId);
        if (data == null)
        {
            Debug.LogError($"[EnemyController] ID '{enemyId}' 데이터가 존재하지 않습니다.");
            yield break;
        }

        InitFromData(data);
        ReflectStatsToInspector(data);
        InitializeFSM();
        stateMachine.ChangeState(idleState);

        // ✅ Reflect 이후에 UI 갱신
        UpdateUIElements();
    }

    private void UpdateUIElements()
    {
        if (nameText != null)
        {
            nameText.text = enemyName;
            Debug.Log($"[EnemyController] 이름 설정됨: {enemyName}");
        }

        if (levelText != null)
            levelText.text = $"Lv.{level}";

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpBarFill != null && maxHp > 0)
        {
            hpBarFill.fillAmount = currentHp / maxHp;
        }
    }

    public void InitializeFSM()
    {
        idleState = new EnemyIdleState(this, stateMachine);
        moveState = new EnemyMoveState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);
        damageState = new EnemyDamageState(this, stateMachine, idleState);

        idleState.SetNextState(moveState);
        moveState.SetNextState(attackState);
        attackState.SetNextState(idleState);
    }

    private void ReflectStatsToInspector(EnemyData data)
    {
        id = data.id;
        enemyName = data.name;
        level = data.level;
        maxHp = data.maxHp;
        currentHp = data.maxHp;
        enemyCurrentHp = currentHp;
        moveSpeed = data.moveSpeed;
        detectionRange = data.detectionRange;
        attackRadius = data.attackRadius;
        attackDamage = data.attackDamage;
        chaseStopRange = data.chaseStopRange;
        stopDistance = data.stopDistance;
        angleThreshold = data.angleThreshold;
        attackCooldown = data.attackCooldown;
        exp = data.exp;
        gold = data.gold;
    }

    protected override void Update()
    {
        base.Update();
        enemyCurrentHp = currentHp;
        UpdateHPBar(); // ✅ 매 프레임 HP Bar 갱신

        // ✅ UI가 카메라를 바라보도록 회전
        if (uiCanvas != null && Camera.main != null)
        {
            uiCanvas.transform.rotation = Quaternion.LookRotation(
                uiCanvas.transform.position - Camera.main.transform.position
            );
        }
    }

    protected override void Die()
    {
        base.Die();

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var playerCtrl))
        {            
            playerCtrl.GainExp(exp);     // 경험치
            playerCtrl.GainGold(gold);   // 💰 골드 추가
        }
        else
        {
            Debug.LogWarning("[EnemyController] 사망 시 PlayerController를 찾지 못했습니다. 경험치 미획득.");
        }
    }
}
