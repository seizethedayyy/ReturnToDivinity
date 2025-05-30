using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private EnemyState nextState;

    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next) => nextState = next;

    public override void LogicUpdate()
    {
        if (enemy.player == null)
        {
            Debug.Log("[IdleState] Player�� null�Դϴ�.");
            return;
        }

        float dist = Vector2.Distance((Vector2)enemy.transform.position, (Vector2)enemy.player.position);
        Debug.Log($"[IdleState] �Ÿ� ���: {dist}");

        if (dist <= enemy.stats.detectionRange)
        {
            Debug.Log("[IdleState] MoveState ���� �õ�");
            stateMachine.ChangeState(nextState);
        }
    }

}
