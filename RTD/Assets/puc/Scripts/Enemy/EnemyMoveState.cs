using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private EnemyState nextState;

    public EnemyMoveState(EnemyBase enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next) => nextState = next;

    public override void LogicUpdate()
    {
        if (enemy.player == null)
        {
            Debug.LogWarning("[MoveState] player가 null입니다.");
            return;
        }

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);

        if (enemy.IsPlayerInAttackRange())
        {
            enemy.SetZeroVelocity();
            enemy.anim.SetBool("IsWalking", false);

            if (enemy.IsAttackCooldownOver())
                stateMachine.ChangeState(nextState);

            return;
        }

        Vector2 dir = (enemy.player.position - enemy.transform.position).normalized;
        enemy.SetVelocity(dir);

        if (!enemy.IsAttacking())
        {
            bool shouldFlip = dir.x < 0;
            if (Mathf.Abs(dir.x) > 0.2f && enemy.sr.flipX != shouldFlip)
                enemy.sr.flipX = shouldFlip;
        }

        enemy.anim.SetBool("IsWalking", true);
    }

    public override void Exit()
    {
        enemy.anim.SetBool("IsWalking", false);
        enemy.SetZeroVelocity();
    }
}


