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

            // 1) Dash ���� ���� �÷��� ����
            controller.IsDashing = true;
            controller.IsInvulnerable = true;
            controller.NextDashTime = Time.time + controller.DashCooldown;
            controller.DashEndTime = Time.time + controller.DashDuration;

            // 2) Dash �߿��� Move �ִϸ��̼��� ������ �ʵ��� ������ ��
            controller.Animator.SetBool("Move", false);

            // 3) Dash �Ķ���� �Ѱ� ������ ó��
            controller.Animator.SetBool("IsDashing", true);
            controller.SpriteRenderer.color = new Color(
                controller.OriginalColor.r,
                controller.OriginalColor.g,
                controller.OriginalColor.b,
                0.5f
            );

            // 4) DashLoop �ڷ�ƾ ����
            controller.StartCoroutine(DashLoop());
        }

        public override void Execute()
        {
            // Dash ���¿����� ���� �Է��� ���� �ʽ��ϴ�.
            // ��� ���߿��� MoveInput�� ��� ���ŵ�����, 
            // ���� �̵��� DashLoop()���� ó���մϴ�.
        }

        public override void Exit()
        {
            base.Exit();
            // Dash ���� �� ���� ó���� ������ ��� DashLoop ���ο��� ó�������Ƿ�,
            // ����� ��� �Ӵϴ�.
        }

        private IEnumerator DashLoop()
        {
            // 1) ���� ��� ���� ���: �̵� �Է��� ������ �ٶ󺸴� ����
            Vector2 direction = controller.MoveInput != Vector2.zero
                ? controller.MoveInput.normalized
                : (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            // 2) Shift Ű�� Ǯ���ų�, ������ DashDuration �ð��� ������ ������������
            while (Time.time < controller.DashEndTime && Input.GetKey(KeyCode.LeftShift))
            {
                // 2-1) �� ������ �̵� �Է��� ������, ��� ���� ����
                if (controller.MoveInput != Vector2.zero)
                {
                    direction = controller.MoveInput.normalized;
                }

                // 2-2) ��� �ӵ� ����
                controller.Rigidbody2D.linearVelocity = direction * controller.DashSpeed;
                yield return null;
            }

            // 3) ��� ���� ó��
            controller.IsDashing = false;
            controller.IsInvulnerable = false;
            controller.Rigidbody2D.linearVelocity = Vector2.zero;
            controller.SpriteRenderer.color = controller.OriginalColor;
            controller.Animator.SetBool("IsDashing", false);

            // 4) ��� ���� �� ���� ����
            bool isMovingNow = controller.MoveInput != Vector2.zero && !Input.GetKey(KeyCode.LeftShift);
            // �� Shift�� ���� ����� ��쿡��, MoveInput�� ���� ������ Move��, 
            //    �̵� �Է��� ������ Idle�� ����
            controller.SetState(isMovingNow ? controller.MoveState : controller.IdleState);
        }
    }
}
