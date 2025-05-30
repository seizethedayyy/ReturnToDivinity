using UnityEngine;

public class EnemyDamageState : EnemyState
{
    private EnemyState returnState;
    private float timer;

    public EnemyDamageState(EnemyBase enemy, EnemyStateMachine stateMachine, EnemyState returnState)
        : base(enemy, stateMachine)
    {
        this.returnState = returnState;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetZeroVelocity();
        enemy.anim.SetTrigger("IsHit");
        timer = 0.2f; // 피격 경직 시간 (필요 시 조절)
    }

    public override void LogicUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            stateMachine.ChangeState(returnState); // 보통 idleState
        }
    }
}
