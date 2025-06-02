using System.Collections;
using UnityEngine;

namespace Player.States
{
    public class PlayerDeadState : PlayerBaseState
    {
        public override string StateName => "Dead";

        public PlayerDeadState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            // 1) 각종 상태 초기화
            controller.IsDead = true;
            controller.IsDashing = false;
            controller.IsAttacking = false;
            controller.IsTakingDamage = false;

            // 2) 이동 정지
            controller.Rigidbody2D.linearVelocity = Vector2.zero;
            controller.SpriteRenderer.color = controller.OriginalColor;

            // 3) 애니메이터: 사망 트리거
            controller.Animator.SetBool("IsDashing", false);
            controller.Animator.SetBool("Move", false);
            controller.Animator.SetTrigger("Die");

            Debug.Log("[Player] ▶ 사망 처리 완료"); // ← (PlayerDeadState.cs, Enter 메서드, Debug.Log 위치)

            // 4) 게임오버 UI
            InGameUIManager.Instance?.ShowGameOverAndReturnToTitle(2.5f);

            // 5) 사망 후 오브젝트 비활성화
            controller.StartCoroutine(RemoveAfterDeath());
        }

        public override void Execute()
        {
            // Dead 상태에서는 아무 입력도 받지 않고, 이동/공격 모두 무시되므로,
            // 별도 로직 없음
        }

        public override void Exit()
        {
            base.Exit();
            // 보통 Dead 상태에서는 Exit를 호출하지 않으므로, 별도 처리 없음
        }

        private IEnumerator RemoveAfterDeath()
        {
            yield return new WaitForSeconds(1.5f);
            controller.gameObject.SetActive(false);
        }
    }
}
