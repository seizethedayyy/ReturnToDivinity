using UnityEngine;
using System.Collections;

public class EnemyController : EnemyBase
{
    [Header("🆔 몬스터 ID (Google Sheets 기준)")]
    public string enemyId;

    [Header("📊 몬스터 정보 (읽기 전용)")]
    [SerializeField] private string id;
    [SerializeField] private string enemyName;
    [SerializeField] private int level;

    [SerializeField] private float maxHp;
    [SerializeField] private float enemyCurrentHp;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float detectionRange;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackDamage;
    [SerializeField] private float chaseStopRange;
    [SerializeField] private float stopDistance;
    [SerializeField] private float angleThreshold;
    [SerializeField] private float attackCooldown;

    [SerializeField] private int exp;
    [SerializeField] private int gold;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(LoadEnemyData());
    }

    private IEnumerator WaitUntilReady()
    {
        // ✅ 1. EnemyData 준비 대기
        while (EnemyDataLoader.LoadedEnemyData == null)
            yield return null;

        // ✅ 2. Player 존재 대기
        while (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            yield return null;
        }

        // ✅ 3. 데이터 가져오기
        var data = EnemyDataLoader.GetEnemyDataById(enemyId);
        if (data == null)
        {
            Debug.LogError($"[EnemyController] ID '{enemyId}'에 해당하는 몬스터 데이터를 찾을 수 없습니다.");
            yield break;
        }

        InitFromData(data);
        SetupFSM();
        ReflectStatsToInspector(data);
    }

    void Update()
    {
        if (stateMachine != null)
        {
            Debug.Log($"[FSM] 현재 상태: {stateMachine.CurrentState?.GetType().Name}");
        }
    }

    private IEnumerator LoadEnemyData()
    {
        // ✅ 데이터 로딩 대기
        while (EnemyDataLoader.LoadedEnemyData == null)
            yield return null;

        // ✅ player 참조 연결 대기
        while (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[EnemyController] Player 참조 연결 완료");
            }

            yield return null;
        }

        // ✅ EnemyData 가져오기
        var data = EnemyDataLoader.GetEnemyDataById(enemyId);
        if (data == null)
        {
            Debug.LogError($"[EnemyController] ID '{enemyId}'에 해당하는 몬스터 데이터를 찾을 수 없습니다.");
            yield break;
        }

        // ✅ 데이터 반영
        InitFromData(data);
        SetupFSM();
        ReflectStatsToInspector(data);
    }

    private void ReflectStatsToInspector(EnemyData data)
    {
        id = data.id;
        enemyName = data.name;
        level = data.level;
        maxHp = data.maxHp;
        enemyCurrentHp = this.currentHp;

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

        Debug.Log($"[DEBUG] attackCooldown: {attackCooldown}, attackRadius: {attackRadius}, moveSpeed: {moveSpeed}");
    }

    // ✅ 애니메이션 이벤트에서 호출: 공격 타격 타이밍
    public void OnAttackTrigger()
    {
        if (stateMachine.CurrentState is EnemyAttackState atk)
        {
            atk.OnAttackTrigger();
        }
    }

    // ✅ 애니메이션 이벤트에서 호출: 공격 애니메이션 종료 시점
    public void EndAttack()
    {
        if (stateMachine.CurrentState is EnemyAttackState atk)
        {
            atk.EndAttack();
        }
    }
}
