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
            // 1) ���� ���� �ʱ�ȭ
            controller.IsDead = true;
            controller.IsDashing = false;
            controller.IsAttacking = false;
            controller.IsTakingDamage = false;

            // 2) �̵� ����
            controller.Rigidbody2D.linearVelocity = Vector2.zero;
            controller.SpriteRenderer.color = controller.OriginalColor;

            // 3) �ִϸ�����: ��� Ʈ����
            controller.Animator.SetBool("IsDashing", false);
            controller.Animator.SetBool("Move", false);
            controller.Animator.SetTrigger("Die");

            Debug.Log("[Player] �� ��� ó�� �Ϸ�"); // �� (PlayerDeadState.cs, Enter �޼���, Debug.Log ��ġ)

            // 4) ���ӿ��� UI
            InGameUIManager.Instance?.ShowGameOverAndReturnToTitle(2.5f);

            // 5) ��� �� ������Ʈ ��Ȱ��ȭ
            controller.StartCoroutine(RemoveAfterDeath());
        }

        public override void Execute()
        {
            // Dead ���¿����� �ƹ� �Էµ� ���� �ʰ�, �̵�/���� ��� ���õǹǷ�,
            // ���� ���� ����
        }

        public override void Exit()
        {
            base.Exit();
            // ���� Dead ���¿����� Exit�� ȣ������ �����Ƿ�, ���� ó�� ����
        }

        private IEnumerator RemoveAfterDeath()
        {
            yield return new WaitForSeconds(1.5f);
            controller.gameObject.SetActive(false);
        }
    }
}
