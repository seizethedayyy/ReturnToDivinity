using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private EnemyState attackState;

    public EnemyMoveState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next)
    {
        attackState = next;
    }

    public override void Enter()
    {
        enemy.anim.SetBool("IsWalking", true);
    }

    public override void Exit()
    {
        enemy.anim.SetBool("IsWalking", false);
        enemy.SetZeroVelocity();
    }

    public override void LogicUpdate()
    {
        if (enemy.IsPlayerInAttackRange())
        {
            enemy.SetZeroVelocity();
            enemy.anim.SetBool("IsWalking", false); // ✅ Idle로 유지

            if (enemy.IsAttackCooldownOver())
            {
                stateMachine.ChangeState(attackState);
            }

            return;
        }

        // 공격 범위 바깥일 경우 이동
        Vector2 dir = (enemy.player.position - enemy.transform.position).normalized;
        enemy.SetVelocity(dir);
        enemy.sr.flipX = dir.x < 0;
        enemy.anim.SetBool("IsWalking", true);
    }
}
