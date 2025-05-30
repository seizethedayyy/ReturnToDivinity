using UnityEngine;

public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit(); // null-safe exit
        CurrentState = newState;
        CurrentState?.Enter(); // null-safe enter
    }

    public void LogicUpdate()
    {
        CurrentState?.LogicUpdate();
    }

    public void PhysicsUpdate()
    {
        CurrentState?.PhysicsUpdate();
    }

    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState?.Enter();
    }
}
