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

        // Animator가 이미 IsAttacking이 true인 상태일 수 있으므로 재설정
        enemy.anim.ResetTrigger("IsHit");
        enemy.anim.SetBool("IsAttacking", false); // 먼저 false 처리하고
        enemy.anim.SetBool("IsAttacking", true);  // 다시 true로 설정해서 트랜지션 유도

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
