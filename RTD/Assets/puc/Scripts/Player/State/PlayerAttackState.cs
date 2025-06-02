using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

            Debug.Log("[AttackState] Enter ¡æ ComboStep=" + controller.ComboStep);

            controller.HasQueuedThisPhase = false;
            controller.QueuedAttack = false;

            controller.IsAttacking = true;
            hasPlayedSfxLocal = false;

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // 1) °ø°Ý »óÅÂ ÁøÀÔ ½Ã ÃÊ±âÈ­
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            hasPlayedSfxLocal = false;
            controller.IsAttacking = true;

            // ÄÞº¸ ´Ü°è °è»ê (ÀÌÀü »óÅÂÀÇ ComboTimer, ComboStep Âü°í)
            controller.ComboStep = controller.ComboStep switch
            {
                0 => 1,
                1 when controller.ComboTimer <= controller.ComboDelay => 2,
                2 when controller.ComboTimer <= controller.ComboDelay => 3,
                3 when controller.ComboTimer <= controller.ComboDelay => 4,
                _ => 1
            };

            // ÀÎ°ÔÀÓ UIÀÇ ÄÞº¸ ½½·Ô ¾÷µ¥ÀÌÆ®
            // ±âÁ¸: controller.InGameUIManager?.UpdateComboSlot(controller.ComboStep);
            // º¯°æ: ½Ì±ÛÅÏÀ» ÅëÇØ Á÷Á¢ È£Ãâ
            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            Debug.Log($"[AttackState] ÄÞº¸ ´Ü°è °è»ê ÈÄ ComboStep={controller.ComboStep}");

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // 2) ¾Ö´Ï¸ÞÀÌ¼Ç Play ¹× SFX Àç»ý
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // Ä³¸¯ÅÍ Å¸ÀÔ¿¡ µû¶ó Àç»ýÇÒ ¾Ö´Ï¸ÞÀÌ¼Ç ÀÌ¸§ °áÁ¤
            string animName = (controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
                ? "Shoot"
                : $"Attack{controller.ComboStep}";

            if (controller.Animator.HasState(0, Animator.StringToHash(animName)))
            {
                controller.Animator.Play(animName, 0);
            }

            // SFX´Â ÇÑ ¹ø¸¸ Àç»ý
            if (!hasPlayedSfxLocal)
            {
                AudioManager.Instance?.PlaySfx("attack_sfx");
                hasPlayedSfxLocal = true;
            }

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // 3) °ø°Ý ¼Óµµ(attackSpeed)¿¡ µû¶ó EndAttack È£Ãâ ½ÃÁ¡ °áÁ¤
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            if (controller.Data != null && controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
            {
                // Castle Å¸ÀÔ(¹Ì»çÀÏ ¹ß»ç)
                controller.Animator.SetBool("IsAttacking", true);

                // attackSpeed¿¡ µû¶ó µô·¹ÀÌ °è»ê (0.6ÃÊ ±âº»)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // Áö¿¬ ÈÄ ¹Ì»çÀÏ ¹ß»ç ¹× EndAttack ½ÇÇà
                controller.StartCoroutine(DelayedShootAndEnd(delay));
            }
            else if (controller.Data != null)
            {
                // Knight Å¸ÀÔ(±ÙÁ¢ °ø°Ý)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // Invoke·Î EndAttack È£Ãâ
                controller.Invoke(nameof(controller.EndAttack), delay);
            }

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¡Ú µð¹ö±× ·Î±× Ãß°¡ ¿¹½Ã:
            //   °ø°Ý ¾Ö´Ï¸ÞÀÌ¼ÇÀÌ Àç»ýµÉ ¶§¸¶´Ù ÂïÈ÷µµ·Ï ÇÏ·Á¸é ÀÌ°÷¿¡ Debug.Log¸¦ Ãß°¡ÇÒ ¼ö ÀÖ½À´Ï´Ù.
            // Debug.Log($"[PlayerAttackState] Enter ¡æ ComboStep={controller.ComboStep}, Anim={animName}");
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        }


        public override void Execute()
        {
            // ¨è Execute ÁøÀÔ ½Ã ·Î±×
            Debug.Log("[AttackState] Execute ¡æ HasQueuedThisPhase=" + controller.HasQueuedThisPhase + ", QueuedAttack=" + controller.QueuedAttack);

            // ÄÞº¸ ÀÔ·Â °¨Áö
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

            // °ø°Ý »óÅÂ¿¡¼­ ³ª¿Ã ¶§ ¾Ö´Ï¸ÞÀÌÅÍ ÆÄ¶ó¹ÌÅÍ ÇØÁ¦
            Debug.Log("[AttackState] Exit ¡æ ÀÌµ¿ »óÅÂ ÀüÀÌ Á÷Àü");
            controller.IsAttacking = false;
            controller.HasPlayedSfx = false;
            controller.Animator.SetBool("IsAttacking", false);

            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
            // ¡Ú µð¹ö±× ·Î±× Ãß°¡ ¿¹½Ã:
            //   °ø°Ý »óÅÂ Exit ½ÃÁ¡¿¡ Debug.Log¸¦ ³²±â°í ½Í´Ù¸é ¾Æ·¡ ÁÙ ÁÖ¼® ÇØÁ¦
            // Debug.Log("[PlayerAttackState] Exit ¡æ Attack state Á¾·á, »óÅÂ ÀüÀÌ ÁØºñµÊ");
            // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        }

        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        // Castle Àü¿ë: ÀÏÁ¤ ½Ã°£ ÈÄ ¹Ì»çÀÏÀ» ¹ß»çÇÏ°í EndAttack() È£Ãâ
        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        private IEnumerator DelayedShootAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);

            // ¹Ì»çÀÏ ¹ß»ç ·ÎÁ÷
            if (controller.MissileObject == null
                || controller.FirePos == null
                || controller.Data == null)
            {
                yield break;
            }

            // ¹Ì»çÀÏ ¿ÀºêÁ§Æ®¸¦ ¹ß»ç À§Ä¡·Î ÀÌµ¿
            controller.MissileObject.transform.position = controller.FirePos.position;

            // ¹æÇâ ¼³Á¤
            Vector2 dir = (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            // Missile ÄÄÆ÷³ÍÆ® ÃÊ±âÈ­ (µ¥¹ÌÁö °è»ê Æ÷ÇÔ)
            Missile missile = controller.MissileObject.GetComponent<Missile>();
            missile.Init(dir, GetMissileDamage());

            // ¹Ì»çÀÏ ±×·¡ÇÈ ¹æÇâ ¸ÂÃã
            SpriteRenderer missileRend = controller.MissileObject.GetComponent<SpriteRenderer>();
            if (missileRend != null)
            {
                missileRend.flipX = controller.SpriteRenderer.flipX;
            }

            // ¹Ì»çÀÏ È°¼ºÈ­
            controller.MissileObject.SetActive(true);

            // ÄÞº¸ ´Ü°è°¡ 4¹Ì¸¸ÀÌ¸é ¹Ù·Î EndAttack, ¾Æ´Ï¸é ¾à°£ÀÇ Ãß°¡ µô·¹ÀÌ ÈÄ EndAttack
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

        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        // ¹Ì»çÀÏ µ¥¹ÌÁö °è»ê ÇÔ¼ö
        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        private int GetMissileDamage()
        {
            if (controller.Data == null)
                return 0;

            // Fury °ÔÀÌÁö ºñÀ² °è»ê
            float gaugePercent = controller.CurrentFuryAmount / controller.Data.furyMax;

            if (gaugePercent >= 3f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 3f);
            if (gaugePercent >= 2f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 2f);

            return Mathf.FloorToInt(controller.Data.attackDamage);
        }
    }
}
