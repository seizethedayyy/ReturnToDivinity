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
        Debug.Log("[EnemyAttackState] ▶ Enter");
        base.Enter();
        enemy.SetZeroVelocity();
        enemy.anim.SetBool("IsAttacking", true);
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
        Debug.Log("[EnemyAttackState] ▶ OnAttackTrigger 호출");  // ← 이 줄 추가
        if (hasAttacked) return;
        enemy.DealDamage();
        hasAttacked = true;
    }

    // 애니메이션 이벤트에서 호출
    public void EndAttack()
    {
        Vector2 exitDir = (enemy.player.position - enemy.transform.position).normalized;
        enemy.sr.flipX = exitDir.x < 0;

        stateMachine.ChangeState(returnState);
    }


}
