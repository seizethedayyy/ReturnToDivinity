using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Animations; // AnimatorControllerParameterType ����� ���� �ʿ�

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

            // ��������������������������������������������������������������������������������������������������������������
            // �� �޺� �ܰ� ��� (���� ���� ����)
            // ��������������������������������������������������������������������������������������������������������������
            if (controller.ComboStep == 0)
            {
                controller.ComboStep = 1; // �޺� ������ ������ 1�ܰ�
            }
            else
            {
                // ���� �ܰ�� ComboTimer �������� ����
                controller.ComboStep = controller.ComboStep switch
                {
                    1 when controller.ComboTimer <= controller.ComboDelay => 2,
                    2 when controller.ComboTimer <= controller.ComboDelay => 3,
                    3 when controller.ComboTimer <= controller.ComboDelay => 4,
                    _ => 1
                };
            }

            Debug.Log($"[PlayerAttackState] �޺� �ܰ� ��� �� ComboStep={controller.ComboStep}");

            // ��������������������������������������������������������������������������������������������������������������
            // �� �ΰ��� UI�� �޺� ���� ������Ʈ
            // ��������������������������������������������������������������������������������������������������������������
            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            // ��������������������������������������������������������������������������������������������������������������
            // �� Animator �Ķ���� ����
            // ��������������������������������������������������������������������������������������������������������������
            controller.Animator.SetBool("IsAttacking", true);
            controller.Animator.SetTrigger("AttackTrigger");
            Debug.Log("[PlayerAttackState] Animator.SetBool(\"IsAttacking\", true) �� SetTrigger(\"AttackTrigger\") ����");

            // ��������������������������������������������������������������������������������������������������������������
            // �� �ִϸ��̼� ���� ��� (Animator Trigger ���� ����)
            // ��������������������������������������������������������������������������������������������������������������
            string animName = (controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
                ? "Shoot"
                : $"Attack{controller.ComboStep}";

            if (controller.Animator.HasState(0, Animator.StringToHash(animName)))
            {
                controller.Animator.Play(animName, 0);
                Debug.Log($"[PlayerAttackState] Animator.Play(\"{animName}\") ����");
            }
            else
            {
                Debug.LogWarning($"[PlayerAttackState] Enter(): Animator�� ���� '{animName}'��(��) �����ϴ�.");
            }

            // ��������������������������������������������������������������������������������������������������������������
            // �� SFX ��� (1ȸ��)
            // ��������������������������������������������������������������������������������������������������������������
            if (!hasPlayedSfxLocal)
            {
                AudioManager.Instance?.PlaySfx("attack_sfx");
                hasPlayedSfxLocal = true;
                Debug.Log("[PlayerAttackState] SFX 'attack_sfx' ���");
            }

            // ��������������������������������������������������������������������������������������������������������������
            // �� ���� �����̿� ���� EndAttack ȣ�� ����
            // ��������������������������������������������������������������������������������������������������������������
            if (controller.Data != null && controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
            {
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                controller.StartCoroutine(DelayedShootAndEnd(delay));
                Debug.Log($"[PlayerAttackState] Castle �� DelayedShootAndEnd({delay:f2}) �ڷ�ƾ ����");
            }
            else if (controller.Data != null)
            {
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                controller.Invoke(nameof(controller.EndAttack), delay);
                Debug.Log($"[PlayerAttackState] Knight �� Invoke EndAttack() after {delay:f2}��");
            }
        }


        public override void Execute()
        {
            if (!controller.HasQueuedThisPhase && Input.GetMouseButtonDown(0))
            {
                int nextComboIndex = controller.ComboStep + 1;
                if (nextComboIndex <= controller.MaxUnlockedCombo)
                {
                    controller.QueuedAttack = true;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] ���� {controller.currentLevel} �� �޺� {nextComboIndex} �Է� ���");
                }
                else
                {
                    controller.QueuedAttack = false;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] ���� {controller.currentLevel} �� �޺� {nextComboIndex} ���, �Է� ����");
                }
            }

            Debug.Log("[AttackState] Execute �� HasQueuedThisPhase=" + controller.HasQueuedThisPhase + ", QueuedAttack=" + controller.QueuedAttack);

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

            // Animator�� IsAttacking�� false�� �����Ͽ� Idle ���� �����ϰ� ��
            controller.Animator.SetBool("IsAttacking", false);
        }

        // ��������������������������������������������������������������������������������������������������������������
        // Castle ����: ���� �ð� �� �̻����� �߻��ϰ� EndAttack() ȣ��
        // ��������������������������������������������������������������������������������������������������������������
        private IEnumerator DelayedShootAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (controller.MissileObject == null
                || controller.FirePos == null
                || controller.Data == null)
            {
                yield break;
            }

            controller.MissileObject.transform.position = controller.FirePos.position;
            Vector2 dir = (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            Missile missile = controller.MissileObject.GetComponent<Missile>();
            missile.Init(dir, GetMissileDamage(), controller.transform);

            SpriteRenderer missileRend = controller.MissileObject.GetComponent<SpriteRenderer>();
            if (missileRend != null)
                missileRend.flipX = controller.SpriteRenderer.flipX;

            controller.MissileObject.SetActive(true);

            if (controller.ComboStep < 4)
            {
                controller.EndAttack();
                Debug.Log("[PlayerAttackState] ComboStep < 4 �� ��� EndAttack() ȣ��");
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                controller.EndAttack();
                Debug.Log("[PlayerAttackState] ComboStep >= 4 �� 0.2�� ��� �� EndAttack() ȣ��");
            }
        }

        // ��������������������������������������������������������������������������������������������������������������
        // �̻��� ������ ��� �Լ�
        // ��������������������������������������������������������������������������������������������������������������
        private int GetMissileDamage()
        {
            if (controller.Data == null)
                return 0;

            float gaugePercent = controller.CurrentFuryAmount / controller.Data.furyMax;

            if (gaugePercent >= 3f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 3f);
            if (gaugePercent >= 2f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 2f);

            return Mathf.FloorToInt(controller.Data.attackDamage);
        }
    }
}

