using UnityEngine;

public class EnemyDamageState : EnemyState
{
    private EnemyState returnState;
    private float elapsed;
    private float duration = 0.3f;

    public EnemyDamageState(EnemyBase enemy, EnemyStateMachine stateMachine, EnemyState returnState)
        : base(enemy, stateMachine)
    {
        this.returnState = returnState;
    }

    public override void Enter()
    {
        elapsed = 0f;
        enemy.SetZeroVelocity();
    }

    public override void LogicUpdate()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            stateMachine.ChangeState(returnState);
        }
    }
}
