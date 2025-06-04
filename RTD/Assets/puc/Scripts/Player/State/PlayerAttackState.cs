using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Animations; // AnimatorControllerParameterType »ç¿ëÀ» À§ÇØ ÇÊ¿ä

namespace Player.States
{
    public class PlayerAttackState : PlayerBaseState
    {
        public override string StateName => "Attack";

        // ·ÎÄÃ º¯¼ö·Î SFX Áßº¹ Àç»ý ¹æÁö¸¦ À§ÇÑ ÇÃ·¡±×
        private bool hasPlayedSfxLocal;

        public PlayerAttackState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();

            controller.HasQueuedThisPhase = false;
            controller.QueuedAttack = false;
            controller.IsAttacking = true;
            hasPlayedSfxLocal = false;

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨ç ÄÞº¸ ´Ü°è °è»ê (°¡Àå ¸ÕÀú ¼öÇà)
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            if (controller.ComboStep == 0)
            {
                controller.ComboStep = 1; // ÄÞº¸ ½ÃÀÛÀº ¹«Á¶°Ç 1´Ü°è
            }
            else
            {
                // ÀÌÈÄ ´Ü°è´Â ComboTimer ±âÁØÀ¸·Î Áõ°¡
                controller.ComboStep = controller.ComboStep switch
                {
                    1 when controller.ComboTimer <= controller.ComboDelay => 2,
                    2 when controller.ComboTimer <= controller.ComboDelay => 3,
                    3 when controller.ComboTimer <= controller.ComboDelay => 4,
                    _ => 1
                };
            }

            Debug.Log($"[PlayerAttackState] ÄÞº¸ ´Ü°è °è»ê ÈÄ ComboStep={controller.ComboStep}");

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨è ÀÎ°ÔÀÓ UIÀÇ ÄÞº¸ ½½·Ô ¾÷µ¥ÀÌÆ®
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨é Animator ÆÄ¶ó¹ÌÅÍ ¼³Á¤
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            controller.Animator.SetBool("IsAttacking", true);
            controller.Animator.SetTrigger("AttackTrigger");
            Debug.Log("[PlayerAttackState] Animator.SetBool(\"IsAttacking\", true) ¹× SetTrigger(\"AttackTrigger\") ½ÇÇà");

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨ê ¾Ö´Ï¸ÞÀÌ¼Ç Á÷Á¢ Àç»ý (Animator Trigger º´Çà °¡´É)
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            string animName = (controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
                ? "Shoot"
                : $"Attack{controller.ComboStep}";

            if (controller.Animator.HasState(0, Animator.StringToHash(animName)))
            {
                controller.Animator.Play(animName, 0);
                Debug.Log($"[PlayerAttackState] Animator.Play(\"{animName}\") ½ÇÇà");
            }
            else
            {
                Debug.LogWarning($"[PlayerAttackState] Enter(): Animator¿¡ »óÅÂ '{animName}'ÀÌ(°¡) ¾ø½À´Ï´Ù.");
            }

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨ë SFX Àç»ý (1È¸¸¸)
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            if (!hasPlayedSfxLocal)
            {
                AudioManager.Instance?.PlaySfx("attack_sfx");
                hasPlayedSfxLocal = true;
                Debug.Log("[PlayerAttackState] SFX 'attack_sfx' Àç»ý");
            }

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¨ì °ø°Ý µô·¹ÀÌ¿¡ µû¶ó EndAttack È£Ãâ ¿¹¾à
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            if (controller.Data != null && controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
            {
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                controller.StartCoroutine(DelayedShootAndEnd(delay));
                Debug.Log($"[PlayerAttackState] Castle ¡æ DelayedShootAndEnd({delay:f2}) ÄÚ·çÆ¾ ½ÃÀÛ");
            }
            else if (controller.Data != null)
            {
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                controller.Invoke(nameof(controller.EndAttack), delay);
                Debug.Log($"[PlayerAttackState] Knight ¡æ Invoke EndAttack() after {delay:f2}ÃÊ");
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
                    Debug.Log($"[AttackState] ·¹º§ {controller.currentLevel} ¡æ ÄÞº¸ {nextComboIndex} ÀÔ·Â Çã¿ë");
                }
                else
                {
                    controller.QueuedAttack = false;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] ·¹º§ {controller.currentLevel} ¡æ ÄÞº¸ {nextComboIndex} Àá±è, ÀÔ·Â ¹«½Ã");
                }
            }

            Debug.Log("[AttackState] Execute ¡æ HasQueuedThisPhase=" + controller.HasQueuedThisPhase + ", QueuedAttack=" + controller.QueuedAttack);

            if (Input.GetMouseButtonDown(0) && !controller.HasQueuedThisPhase)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                controller.QueuedAttack = true;
                controller.HasQueuedThisPhase = true;
                Debug.Log("[AttackState] ÄÞº¸ ÀÔ·Â °¨Áö ¡æ queuedAttack=true");
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log($"[AttackState] Exit ¡æ ComboStep={controller.ComboStep}");

            controller.IsAttacking = false;
            controller.HasPlayedSfx = false;

            // AnimatorÀÇ IsAttackingÀ» false·Î ÇØÁ¦ÇÏ¿© Idle ÀüÀÌ °¡´ÉÇÏ°Ô ÇÔ
            controller.Animator.SetBool("IsAttacking", false);
        }

        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        // Castle Àü¿ë: ÀÏÁ¤ ½Ã°£ ÈÄ ¹Ì»çÀÏÀ» ¹ß»çÇÏ°í EndAttack() È£Ãâ
        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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
                Debug.Log("[PlayerAttackState] ComboStep < 4 ¡æ Áï½Ã EndAttack() È£Ãâ");
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                controller.EndAttack();
                Debug.Log("[PlayerAttackState] ComboStep >= 4 ¡æ 0.2ÃÊ ´ë±â ÈÄ EndAttack() È£Ãâ");
            }
        }

        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        // ¹Ì»çÀÏ µ¥¹ÌÁö °è»ê ÇÔ¼ö
        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

