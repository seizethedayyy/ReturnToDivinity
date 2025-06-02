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
            // Hit 상태에서는 입력이 차단되므로, 별도 로직 없음
        }

        public override void Exit()
        {
            base.Exit();
            // 스턴 끝난 뒤 Exit에서는 색상만 복원(FlashRed 코루틴에서 색 복원됨)
        }

        private IEnumerator EndHitAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            controller.IsTakingDamage = false;
            controller.HitCoroutine = null;
            // 스턴 종료 후 이동 입력에 따라 Idle 또는 Move 상태 전이
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
