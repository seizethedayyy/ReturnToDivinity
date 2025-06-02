using UnityEngine;
using System.Collections;

public class EnemyController : EnemyBase
{
    [Header("🆔 몬스터 ID (Google Sheets 기준)")]
    public string enemyId;

    [Header("📊 Stats (실시간 표시용)")]
    [SerializeField] private float enemyCurrentHp; // 인스펙터 표시용 변수

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
        enemyCurrentHp = currentHp; // 실시간 표시용 갱신
    }

    protected override void Die()
    {
        // 1) 먼저 부모 클래스에서 사망 처리를 수행
        base.Die();

        // 2) 플레이어에게 경험치 전달
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<PlayerController>(out var playerCtrl))
        {
            playerCtrl.GainExp(exp);
        }
        else
        {
            Debug.LogWarning("[EnemyController] 사망 시 PlayerController를 찾지 못했습니다. 경험치 미획득.");
        }
    }

}
