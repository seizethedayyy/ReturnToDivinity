using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private bool triggerCalled;
    private EnemyState idleState;

    public EnemyAttackState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public void SetNextState(EnemyState next)
    {
        idleState = next;
    }

    public override void Enter()
    {
        Debug.Log("[AttackState] Enter 상태 진입");

        triggerCalled = false;
        enemy.SetZeroVelocity();
        enemy.SetAttacking(true);

        // ✅ 이동 애니메이션 확실히 OFF
        enemy.anim.SetBool("IsWalking", false);
        enemy.anim.SetBool("IsAttacking", true);
    }

    public override void LogicUpdate()
    {
        if (triggerCalled)
        {
            Debug.Log("[AttackState] 트리거 완료 → Idle 전환");

            enemy.anim.SetBool("IsAttacking", false);
            enemy.anim.SetBool("IsWalking", false); // ✅ 이 줄 추가해서 공격 직후 Walk 방지

            enemy.SetAttacking(false);
            stateMachine.ChangeState(idleState);
        }
    }


    public void OnAttackTrigger()
    {
        Debug.Log("[EnemyAttackState] TryDealDamage 트리거 호출됨");
        enemy.DealDamage();
        enemy.SetLastAttackTime(); // ✅ 공격 타이밍 저장
        triggerCalled = true;
    }
}
