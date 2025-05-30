using UnityEngine;

public abstract class EnemyState
{
    protected EnemyBase enemy;
    protected EnemyStateMachine stateMachine;

    public EnemyState(EnemyBase enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
}