using UnityEngine;

public class Enemy : EnemyBase
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
}
