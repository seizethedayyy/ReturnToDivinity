using UnityEngine;

public class EnemyDamageState : EnemyState
{
    private EnemyState returnState;
    private float timer;

    public EnemyDamageState(EnemyBase enemy, EnemyStateMachine stateMachine, EnemyState returnState) : base(enemy, stateMachine)
    {
        this.returnState = returnState;
    }

    public override void Enter()
    {
        base.Enter();
        timer = 0.2f;
        enemy.anim.SetTrigger("IsHit");
        enemy.SetZeroVelocity();
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            stateMachine.ChangeState(returnState);
        }
    }
}