using UnityEngine;

public class EnemyController : EnemyBase
{       

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine);
        moveState = new EnemyMoveState(this, stateMachine);
        attackState = new EnemyAttackState(this, stateMachine);

        // 상태 간 연결
        ((EnemyIdleState)idleState).SetNextState(moveState);
        ((EnemyMoveState)moveState).SetNextState(attackState);
        ((EnemyAttackState)attackState).SetNextState(idleState);

        stateMachine.ChangeState(idleState);
    }

    public void TryDealDamage()
    {
        Debug.Log("[Enemy] TryDealDamage 호출됨");

        if (stateMachine.CurrentState is EnemyAttackState atk)
        {
            Debug.Log("[Enemy] 현재 상태는 EnemyAttackState - 트리거 실행");
            atk.OnAttackTrigger();
        }
        else
        {
            Debug.LogWarning($"[Enemy] 상태가 Attack이 아님: {stateMachine.CurrentState?.GetType().Name}");
        }
    }

    public void EndAttack()
    {
        // 아무 작업 안 함
    }

}
