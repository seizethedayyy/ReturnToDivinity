using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private EnemyState nextState;

    public EnemyMoveState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next) => nextState = next;

    public override void LogicUpdate()
    {
        if (enemy.player == null) return;

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);

        if (enemy.IsPlayerInAttackRange())
        {
            enemy.SetZeroVelocity();
            enemy.anim.SetBool("IsWalking", false);

            if (enemy.IsAttackCooldownOver())
            {
                stateMachine.ChangeState(nextState);
            }

            return;
        }

        Vector2 dir = (enemy.player.position - enemy.transform.position).normalized;
        enemy.SetVelocity(dir);

        // ✅ 공격 중에는 Flip 유지
        if (!enemy.IsAttacking())
        {
            enemy.sr.flipX = dir.x < 0;
        }
    }

    public override void Enter()
    {
        base.Enter();
        enemy.anim.SetBool("IsWalking", true);
    }


    public override void Exit()
    {
        base.Exit();
        enemy.anim.SetBool("IsWalking", false);
        enemy.SetZeroVelocity();
    }
}
