using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private EnemyState returnState;
    private bool hasAttacked;

    public EnemyAttackState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next) => returnState = next;

    public override void Enter()
    {
        base.Enter();

        enemy.anim.SetBool("IsAttacking", true);
        enemy.SetAttacking(true);
        enemy.SetZeroVelocity();
        enemy.SetLastAttackTime();

        hasAttacked = false;
    }

    public override void Exit()
    {
        base.Exit();

        enemy.anim.SetBool("IsAttacking", false);
        enemy.SetAttacking(false);
    }

    public void OnAttackTrigger()
    {
        if (hasAttacked) return;
        enemy.DealDamage();
        hasAttacked = true;
    }

    public void EndAttack()
    {
        stateMachine.ChangeState(returnState);
    }
}
