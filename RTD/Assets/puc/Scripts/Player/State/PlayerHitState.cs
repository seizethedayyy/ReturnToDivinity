using System.Collections;
using UnityEngine;

namespace Player.States
{
    public class PlayerHitState : PlayerBaseState
    {
        public override string StateName => "Hit";

        public PlayerHitState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            controller.Animator.SetTrigger("IsHit");
            if (controller.FlashCoroutine != null)
                controller.StopCoroutine(controller.FlashCoroutine);
            controller.FlashCoroutine = controller.StartCoroutine(FlashRed());

            controller.IsTakingDamage = true;
            if (controller.HitCoroutine != null)
                controller.StopCoroutine(controller.HitCoroutine);
            controller.HitCoroutine = controller.StartCoroutine(EndHitAfter(controller.Data.hitStunTime));
        }

        public override void Execute()
        {
            // Hit ���¿����� �Է��� ���ܵǹǷ�, ���� ���� ����
        }

        public override void Exit()
        {
            base.Exit();
            // ���� ���� �� Exit������ ���� ����(FlashRed �ڷ�ƾ���� �� ������)
        }

        private IEnumerator EndHitAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            controller.IsTakingDamage = false;
            controller.HitCoroutine = null;
            // ���� ���� �� �̵� �Է¿� ���� Idle �Ǵ� Move ���� ����
            bool isMoving = controller.MoveInput != Vector2.zero;
            controller.SetState(isMoving ? controller.MoveState : controller.IdleState);
        }

        private IEnumerator FlashRed()
        {
            controller.SpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(controller.HitFlashDuration);
            controller.SpriteRenderer.color = controller.OriginalColor;
        }
    }
}
