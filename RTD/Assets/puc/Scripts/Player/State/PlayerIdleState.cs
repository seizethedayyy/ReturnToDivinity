using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        public override string StateName => "Idle";

        public PlayerIdleState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // Idle 진입 시, 이동 애니메이션 끄기
            controller.Animator.SetBool("Move", false);
        }

        public override void Execute()
        {
            // 1) 대시 입력: Shift 누르고 쿨다운 지나면 Dash 상태로 전이
            if (!controller.IsDashing &&
                Input.GetKeyDown(KeyCode.LeftShift) &&
                Time.time >= controller.NextDashTime)
            {
                controller.SetState(controller.DashState);
                return;
            }

            // 2) 이동 입력: 양방향 입력이 들어오면 Move 상태로 전이
            controller.MoveInput.x = Input.GetAxisRaw("Horizontal");
            controller.MoveInput.y = Input.GetAxisRaw("Vertical");
            bool isMoving = controller.MoveInput != Vector2.zero;
            if (isMoving)
            {
                controller.SetState(controller.MoveState);
                return;
            }

            // 3) 그 외: 앉아서 Idle 애니메이션만 유지(필요 시)
            //    이미 Enter()에서 애니메이터 Move=false 처리했으므로 별도 코드 없음
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
