using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private EnemyState returnState;
    private bool hasAttacked;

    public EnemyAttackState(EnemyBase enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next)
    {
        returnState = next;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetZeroVelocity();                  // 이동 정지
        enemy.anim.SetBool("IsAttacking", true);  // 애니메이션 상태 진입
        enemy.SetAttacking(true);
        enemy.SetLastAttackTime();

        hasAttacked = false;
    }

    public override void LogicUpdate()
    {
        
    }

    public override void Exit()
    {
        base.Exit();

        enemy.anim.SetBool("IsAttacking", false);
        enemy.SetAttacking(false);
    }

    // 애니메이션 이벤트에서 호출
    public void OnAttackTrigger()
    {
        if (hasAttacked) return;

        enemy.DealDamage();
        hasAttacked = true;
    }

    // 애니메이션 이벤트에서 호출
    public void EndAttack()
    {
        stateMachine.ChangeState(returnState);
    }


}
