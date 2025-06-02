using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.States
{
    public class PlayerAttackState : PlayerBaseState
    {
        public override string StateName => "Attack";

        // ���� ������ SFX �ߺ� ��� ������ ���� �÷���
        private bool hasPlayedSfxLocal;

        public PlayerAttackState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();                       

            controller.HasQueuedThisPhase = false;
            controller.QueuedAttack = false;

            controller.IsAttacking = true;
            hasPlayedSfxLocal = false;

            controller.ComboStep++;
            Debug.Log($"[AttackState] Enter �� ComboStep={controller.ComboStep}");

            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            hasPlayedSfxLocal = false;
            controller.IsAttacking = true;

            controller.Animator.SetBool("IsAttacking", true);
            controller.Animator.SetInteger("ComboStep", controller.ComboStep);

            // �޺� �ܰ� ��� (���� ������ ComboTimer, ComboStep ����)
            controller.ComboStep = controller.ComboStep switch
            {
                0 => 1,
                1 when controller.ComboTimer <= controller.ComboDelay => 2,
                2 when controller.ComboTimer <= controller.ComboDelay => 3,
                3 when controller.ComboTimer <= controller.ComboDelay => 4,
                _ => 1
            };
                          
            
            // �ΰ��� UI�� �޺� ���� ������Ʈ
            // ����: controller.InGameUIManager?.UpdateComboSlot(controller.ComboStep);
            // ����: �̱����� ���� ���� ȣ��
            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            Debug.Log($"[AttackState] �޺� �ܰ� ��� �� ComboStep={controller.ComboStep}");

            // ��������������������������������������������������������������������������������������������������������������
            // 2) �ִϸ��̼� Play �� SFX ���
            // ��������������������������������������������������������������������������������������������������������������
            // ĳ���� Ÿ�Կ� ���� ����� �ִϸ��̼� �̸� ����
            string animName = (controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
                ? "Shoot"
                : $"Attack{controller.ComboStep}";

            if (controller.Animator.HasState(0, Animator.StringToHash(animName)))
            {
                controller.Animator.Play(animName, 0);
            }

            // SFX�� �� ���� ���
            if (!hasPlayedSfxLocal)
            {
                AudioManager.Instance?.PlaySfx("attack_sfx");
                hasPlayedSfxLocal = true;
            }

            // ��������������������������������������������������������������������������������������������������������������
            // 3) ���� �ӵ�(attackSpeed)�� ���� EndAttack ȣ�� ���� ����
            // ��������������������������������������������������������������������������������������������������������������
            if (controller.Data != null && controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
            {
                // Castle Ÿ��(�̻��� �߻�)
                controller.Animator.SetBool("IsAttacking", true);

                // attackSpeed�� ���� ������ ��� (0.6�� �⺻)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // ���� �� �̻��� �߻� �� EndAttack ����
                controller.StartCoroutine(DelayedShootAndEnd(delay));
            }
            else if (controller.Data != null)
            {
                // Knight Ÿ��(���� ����)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // Invoke�� EndAttack ȣ��
                controller.Invoke(nameof(controller.EndAttack), delay);
            }

            // ��������������������������������������������������������������������������������������������������������������
            // �� ����� �α� �߰� ����:
            //   ���� �ִϸ��̼��� ����� ������ �������� �Ϸ��� �̰��� Debug.Log�� �߰��� �� �ֽ��ϴ�.
            // Debug.Log($"[PlayerAttackState] Enter �� ComboStep={controller.ComboStep}, Anim={animName}");
            // ��������������������������������������������������������������������������������������������������������������
        }


        public override void Execute()
        {
            
            if (!controller.HasQueuedThisPhase && Input.GetMouseButtonDown(0))
            {
                // ���� �޺� ��ȣ = ���� �޺� �ܰ�(ComboStep) + 1
                int nextComboIndex = controller.ComboStep + 1;

                // '��Ʈ�ѷ�.MaxUnlockedCombo' �Ӽ����� �ر� ���� �˻�
                if (nextComboIndex <= controller.MaxUnlockedCombo)
                {
                    controller.QueuedAttack = true;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] ���� {controller.currentLevel} �� �޺� {nextComboIndex} �Է� ���");
                }
                else
                {
                    // ��� �޺���� QueuedAttack�� ������ �ʰ� ����
                    controller.QueuedAttack = false;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] ���� {controller.currentLevel} �� �޺� {nextComboIndex} ���, �Է� ����");
                }
            }

            // �� Execute ���� �� �α�
            Debug.Log("[AttackState] Execute �� HasQueuedThisPhase=" + controller.HasQueuedThisPhase + ", QueuedAttack=" + controller.QueuedAttack);

            // �޺� �Է� ����
            if (Input.GetMouseButtonDown(0) && !controller.HasQueuedThisPhase)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                controller.QueuedAttack = true;
                controller.HasQueuedThisPhase = true;
                Debug.Log("[AttackState] �޺� �Է� ���� �� queuedAttack=true");
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log($"[AttackState] Exit �� ComboStep={controller.ComboStep}");

            controller.IsAttacking = false;
            controller.HasPlayedSfx = false;
            controller.Animator.SetBool("IsAttacking", false);
                        
        }

        // ��������������������������������������������������������������������������������������������������������������
        // Castle ����: ���� �ð� �� �̻����� �߻��ϰ� EndAttack() ȣ��
        // ��������������������������������������������������������������������������������������������������������������
        private IEnumerator DelayedShootAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);

            // �̻��� �߻� ����
            if (controller.MissileObject == null
                || controller.FirePos == null
                || controller.Data == null)
            {
                yield break;
            }

            // �̻��� ������Ʈ�� �߻� ��ġ�� �̵�
            controller.MissileObject.transform.position = controller.FirePos.position;

            // ���� ����
            Vector2 dir = (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            // Missile ������Ʈ �ʱ�ȭ (������ ��� ����)
            Missile missile = controller.MissileObject.GetComponent<Missile>();
            missile.Init(dir, GetMissileDamage());

            // �̻��� �׷��� ���� ����
            SpriteRenderer missileRend = controller.MissileObject.GetComponent<SpriteRenderer>();
            if (missileRend != null)
            {
                missileRend.flipX = controller.SpriteRenderer.flipX;
            }

            // �̻��� Ȱ��ȭ
            controller.MissileObject.SetActive(true);

            // �޺� �ܰ谡 4�̸��̸� �ٷ� EndAttack, �ƴϸ� �ణ�� �߰� ������ �� EndAttack
            if (controller.ComboStep < 4)
            {
                controller.EndAttack();
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                controller.EndAttack();
            }
        }

        // ��������������������������������������������������������������������������������������������������������������
        // �̻��� ������ ��� �Լ�
        // ��������������������������������������������������������������������������������������������������������������
        private int GetMissileDamage()
        {
            if (controller.Data == null)
                return 0;

            // Fury ������ ���� ���
            float gaugePercent = controller.CurrentFuryAmount / controller.Data.furyMax;

            if (gaugePercent >= 3f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 3f);
            if (gaugePercent >= 2f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 2f);

            return Mathf.FloorToInt(controller.Data.attackDamage);
        }
    }
}
