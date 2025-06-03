using UnityEngine;

namespace Player.States
{
    public class PlayerMoveState : PlayerBaseState
    {
        public override string StateName => "Move";

        public PlayerMoveState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Move 진입 시 애니메이터 Move=true
            controller.Animator.SetBool("Move", true);
        }

        public override void Execute()
        {
            // 1) 대시 입력: Shift 누르고 쿨다운 지나면 Dash 상태 전이
            if (!controller.IsDashing &&
                Input.GetKeyDown(KeyCode.LeftShift) &&
                Time.time >= controller.NextDashTime)
            {
                controller.SetState(controller.DashState);
                return;
            }

            // 2) 이동 입력 유지: 키 입력 받아서 이동
            controller.MoveInput.x = Input.GetAxisRaw("Horizontal");
            controller.MoveInput.y = Input.GetAxisRaw("Vertical");
            bool isMoving = controller.MoveInput != Vector2.zero;
            if (!isMoving)
            {
                // 입력이 끊기면 Idle로 돌아가기
                controller.SetState(controller.IdleState);
                return;
            }

            // 3) Sprite 방향 전환
            if (controller.MoveInput.x != 0)
                controller.SpriteRenderer.flipX = (controller.MoveInput.x < 0);

            // → 실제 속도 세팅(FixedUpdate에서 처리하므로, 여기서는 애니메이터 & 방향 변경만)
        }

        public override void Exit()
        {
            base.Exit();
            // Move 상태에서 나올 때, 애니메이터 Move 파라미터 끄기
            controller.Animator.SetBool("Move", false);
        }
    }
}
