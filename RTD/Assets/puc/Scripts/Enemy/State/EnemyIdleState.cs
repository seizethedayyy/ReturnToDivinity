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
        if (dist <= enemy.stats.detectionRange)
        {
            stateMachine.ChangeState(nextState);
        }
    }
}
