using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private EnemyState moveState;

    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next)
    {
        moveState = next;
    }

    public override void Enter()
    {
        enemy.anim.SetBool("IsWalking", false);
        enemy.SetZeroVelocity();
    }

    public override void LogicUpdate()
    {
        if (enemy.player == null) return;

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);
        Debug.Log($"[IdleState] 플레이어 거리: {dist:F2}");

        if (dist <= enemy.stats.detectionRange)
        {
            Debug.Log("[IdleState] 감지 범위 진입 → MoveState 전환");
            stateMachine.ChangeState(moveState);
        }
    }
}
