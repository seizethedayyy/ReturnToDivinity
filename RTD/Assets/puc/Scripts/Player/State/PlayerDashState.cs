using System.Collections;
using UnityEngine;

namespace Player.States
{
    public class PlayerDashState : PlayerBaseState
    {
        public override string StateName => "Dash";

        public PlayerDashState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();

            // 1) Dash 상태 진입 플래그 설정
            controller.IsDashing = true;
            controller.IsInvulnerable = true;
            controller.NextDashTime = Time.time + controller.DashCooldown;
            controller.DashEndTime = Time.time + controller.DashDuration;

            // 2) Dash 중에는 Move 애니메이션이 나오지 않도록 강제로 끔
            controller.Animator.SetBool("Move", false);

            // 3) Dash 파라미터 켜고 반투명 처리
            controller.Animator.SetBool("IsDashing", true);
            controller.SpriteRenderer.color = new Color(
                controller.OriginalColor.r,
                controller.OriginalColor.g,
                controller.OriginalColor.b,
                0.5f
            );

            // 4) DashLoop 코루틴 시작
            controller.StartCoroutine(DashLoop());
        }

        public override void Execute()
        {
            // Dash 상태에서는 별도 입력을 받지 않습니다.
            // 대시 도중에도 MoveInput은 계속 갱신되지만, 
            // 물리 이동은 DashLoop()에서 처리합니다.
        }

        public override void Exit()
        {
            base.Exit();
            // Dash 종료 후 별도 처리할 내용은 모두 DashLoop 내부에서 처리했으므로,
            // 여기는 비워 둡니다.
        }

        private IEnumerator DashLoop()
        {
            // 1) 최초 대시 방향 계산: 이동 입력이 없으면 바라보는 방향
            Vector2 direction = controller.MoveInput != Vector2.zero
                ? controller.MoveInput.normalized
                : (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            // 2) Shift 키가 풀리거나, 지정된 DashDuration 시간이 지나면 빠져나오도록
            while (Time.time < controller.DashEndTime && Input.GetKey(KeyCode.LeftShift))
            {
                // 2-1) 매 프레임 이동 입력이 있으면, 대시 방향 갱신
                if (controller.MoveInput != Vector2.zero)
                {
                    direction = controller.MoveInput.normalized;
                }

                // 2-2) 대시 속도 적용
                controller.Rigidbody2D.linearVelocity = direction * controller.DashSpeed;
                yield return null;
            }

            // 3) 대시 종료 처리
            controller.IsDashing = false;
            controller.IsInvulnerable = false;
            controller.Rigidbody2D.linearVelocity = Vector2.zero;
            controller.SpriteRenderer.color = controller.OriginalColor;
            controller.Animator.SetBool("IsDashing", false);

            // 4) 대시 종료 후 상태 전이
            bool isMovingNow = controller.MoveInput != Vector2.zero && !Input.GetKey(KeyCode.LeftShift);
            // → Shift를 떼서 종료된 경우에도, MoveInput이 남아 있으면 Move로, 
            //    이동 입력이 없으면 Idle로 복귀
            controller.SetState(isMovingNow ? controller.MoveState : controller.IdleState);
        }
    }
}
