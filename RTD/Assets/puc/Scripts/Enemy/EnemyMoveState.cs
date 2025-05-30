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

        // ── 방향 전환: 플레이어 방향 즉시 반영 ──
        Vector2 dir = (enemy.player.position - enemy.transform.position).normalized;
        bool shouldFlip = dir.x < 0;
        if (enemy.sr.flipX != shouldFlip)
            enemy.sr.flipX = shouldFlip;

        // 플레이어가 공격 사거리 내이면 이동 대신 공격 준비
        if (enemy.IsPlayerInAttackRange())
        {
            enemy.SetZeroVelocity();
            enemy.anim.SetBool("IsWalking", false);

            if (enemy.IsAttackCooldownOver())
                stateMachine.ChangeState(nextState);

            return;
        }

        // 이동 처리
        enemy.SetVelocity(dir);
        enemy.anim.SetBool("IsWalking", true);
    }

    public override void Exit()
    {
        enemy.anim.SetBool("IsWalking", false);
        enemy.SetZeroVelocity();
    }
}
