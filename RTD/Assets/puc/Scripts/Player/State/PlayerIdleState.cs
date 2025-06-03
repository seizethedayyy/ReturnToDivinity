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
            // Idle ���� ��, �̵� �ִϸ��̼� ����
            controller.Animator.SetBool("Move", false);
        }

        public override void Execute()
        {
            // 1) ��� �Է�: Shift ������ ��ٿ� ������ Dash ���·� ����
            if (!controller.IsDashing &&
                Input.GetKeyDown(KeyCode.LeftShift) &&
                Time.time >= controller.NextDashTime)
            {
                controller.SetState(controller.DashState);
                return;
            }

            // 2) �̵� �Է�: ����� �Է��� ������ Move ���·� ����
            controller.MoveInput.x = Input.GetAxisRaw("Horizontal");
            controller.MoveInput.y = Input.GetAxisRaw("Vertical");
            bool isMoving = controller.MoveInput != Vector2.zero;
            if (isMoving)
            {
                controller.SetState(controller.MoveState);
                return;
            }

            // 3) �� ��: �ɾƼ� Idle �ִϸ��̼Ǹ� ����(�ʿ� ��)
            //    �̹� Enter()���� �ִϸ����� Move=false ó�������Ƿ� ���� �ڵ� ����
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
