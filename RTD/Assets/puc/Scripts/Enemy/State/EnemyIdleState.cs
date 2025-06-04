using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private EnemyState nextState;

    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }
        
    public void SetNextState(EnemyState next)
    {
        nextState = next;
    }

    public override void LogicUpdate()
    {
        if (enemy.player == null)
        {
            Debug.LogWarning("[IdleState] player가 null입니다.");
            return;
        }

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);

        // 💡 감지 범위 이내이거나 피격 상태라면 추적 시작
        if (dist <= enemy.stats.detectionRange || enemy.hasBeenHitRecently)
        {
            stateMachine.ChangeState(nextState);
            enemy.ClearHitFlag();  // 추적 상태 진입 시 피격 플래그 초기화
        }
    }
}
