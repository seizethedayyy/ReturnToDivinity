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
            // Move ���� �� �ִϸ����� Move=true
            controller.Animator.SetBool("Move", true);
        }

        public override void Execute()
        {
            // 1) ��� �Է�: Shift ������ ��ٿ� ������ Dash ���� ����
            if (!controller.IsDashing &&
                Input.GetKeyDown(KeyCode.LeftShift) &&
                Time.time >= controller.NextDashTime)
            {
                controller.SetState(controller.DashState);
                return;
            }

            // 2) �̵� �Է� ����: Ű �Է� �޾Ƽ� �̵�
            controller.MoveInput.x = Input.GetAxisRaw("Horizontal");
            controller.MoveInput.y = Input.GetAxisRaw("Vertical");
            bool isMoving = controller.MoveInput != Vector2.zero;
            if (!isMoving)
            {
                // �Է��� ����� Idle�� ���ư���
                controller.SetState(controller.IdleState);
                return;
            }

            // 3) Sprite ���� ��ȯ
            if (controller.MoveInput.x != 0)
                controller.SpriteRenderer.flipX = (controller.MoveInput.x < 0);

            // �� ���� �ӵ� ����(FixedUpdate���� ó���ϹǷ�, ���⼭�� �ִϸ����� & ���� ���游)
        }

        public override void Exit()
        {
            base.Exit();
            // Move ���¿��� ���� ��, �ִϸ����� Move �Ķ���� ����
            controller.Animator.SetBool("Move", false);
        }
    }
}
