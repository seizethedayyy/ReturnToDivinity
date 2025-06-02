using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.States
{
    public class PlayerAttackState : PlayerBaseState
    {
        public override string StateName => "Attack";

        // 로컬 변수로 SFX 중복 재생 방지를 위한 플래그
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
            Debug.Log($"[AttackState] Enter → ComboStep={controller.ComboStep}");

            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            hasPlayedSfxLocal = false;
            controller.IsAttacking = true;

            controller.Animator.SetBool("IsAttacking", true);
            controller.Animator.SetInteger("ComboStep", controller.ComboStep);

            // 콤보 단계 계산 (이전 상태의 ComboTimer, ComboStep 참고)
            controller.ComboStep = controller.ComboStep switch
            {
                0 => 1,
                1 when controller.ComboTimer <= controller.ComboDelay => 2,
                2 when controller.ComboTimer <= controller.ComboDelay => 3,
                3 when controller.ComboTimer <= controller.ComboDelay => 4,
                _ => 1
            };
                          
            
            // 인게임 UI의 콤보 슬롯 업데이트
            // 기존: controller.InGameUIManager?.UpdateComboSlot(controller.ComboStep);
            // 변경: 싱글턴을 통해 직접 호출
            InGameUIManager.Instance?.UpdateComboSlot(controller.ComboStep);

            Debug.Log($"[AttackState] 콤보 단계 계산 후 ComboStep={controller.ComboStep}");

            // ───────────────────────────────────────────────────────
            // 2) 애니메이션 Play 및 SFX 재생
            // ───────────────────────────────────────────────────────
            // 캐릭터 타입에 따라 재생할 애니메이션 이름 결정
            string animName = (controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
                ? "Shoot"
                : $"Attack{controller.ComboStep}";

            if (controller.Animator.HasState(0, Animator.StringToHash(animName)))
            {
                controller.Animator.Play(animName, 0);
            }

            // SFX는 한 번만 재생
            if (!hasPlayedSfxLocal)
            {
                AudioManager.Instance?.PlaySfx("attack_sfx");
                hasPlayedSfxLocal = true;
            }

            // ───────────────────────────────────────────────────────
            // 3) 공격 속도(attackSpeed)에 따라 EndAttack 호출 시점 결정
            // ───────────────────────────────────────────────────────
            if (controller.Data != null && controller.CurrentCharacterType == PlayerController.CharacterType.Castle)
            {
                // Castle 타입(미사일 발사)
                controller.Animator.SetBool("IsAttacking", true);

                // attackSpeed에 따라 딜레이 계산 (0.6초 기본)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // 지연 후 미사일 발사 및 EndAttack 실행
                controller.StartCoroutine(DelayedShootAndEnd(delay));
            }
            else if (controller.Data != null)
            {
                // Knight 타입(근접 공격)
                float delay = controller.Data.attackSpeed > 0
                    ? 1f / controller.Data.attackSpeed
                    : 0.6f;

                // Invoke로 EndAttack 호출
                controller.Invoke(nameof(controller.EndAttack), delay);
            }

            // ───────────────────────────────────────────────────────
            // ★ 디버그 로그 추가 예시:
            //   공격 애니메이션이 재생될 때마다 찍히도록 하려면 이곳에 Debug.Log를 추가할 수 있습니다.
            // Debug.Log($"[PlayerAttackState] Enter → ComboStep={controller.ComboStep}, Anim={animName}");
            // ───────────────────────────────────────────────────────
        }


        public override void Execute()
        {
            
            if (!controller.HasQueuedThisPhase && Input.GetMouseButtonDown(0))
            {
                // 다음 콤보 번호 = 현재 콤보 단계(ComboStep) + 1
                int nextComboIndex = controller.ComboStep + 1;

                // '컨트롤러.MaxUnlockedCombo' 속성으로 해금 여부 검사
                if (nextComboIndex <= controller.MaxUnlockedCombo)
                {
                    controller.QueuedAttack = true;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] 레벨 {controller.currentLevel} → 콤보 {nextComboIndex} 입력 허용");
                }
                else
                {
                    // 잠긴 콤보라면 QueuedAttack을 붙이지 않고 무시
                    controller.QueuedAttack = false;
                    controller.HasQueuedThisPhase = true;
                    Debug.Log($"[AttackState] 레벨 {controller.currentLevel} → 콤보 {nextComboIndex} 잠김, 입력 무시");
                }
            }

            // ② Execute 진입 시 로그
            Debug.Log("[AttackState] Execute → HasQueuedThisPhase=" + controller.HasQueuedThisPhase + ", QueuedAttack=" + controller.QueuedAttack);

            // 콤보 입력 감지
            if (Input.GetMouseButtonDown(0) && !controller.HasQueuedThisPhase)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                controller.QueuedAttack = true;
                controller.HasQueuedThisPhase = true;
                Debug.Log("[AttackState] 콤보 입력 감지 → queuedAttack=true");
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log($"[AttackState] Exit → ComboStep={controller.ComboStep}");

            controller.IsAttacking = false;
            controller.HasPlayedSfx = false;
            controller.Animator.SetBool("IsAttacking", false);
                        
        }

        // ───────────────────────────────────────────────────────
        // Castle 전용: 일정 시간 후 미사일을 발사하고 EndAttack() 호출
        // ───────────────────────────────────────────────────────
        private IEnumerator DelayedShootAndEnd(float delay)
        {
            yield return new WaitForSeconds(delay);

            // 미사일 발사 로직
            if (controller.MissileObject == null
                || controller.FirePos == null
                || controller.Data == null)
            {
                yield break;
            }

            // 미사일 오브젝트를 발사 위치로 이동
            controller.MissileObject.transform.position = controller.FirePos.position;

            // 방향 설정
            Vector2 dir = (controller.SpriteRenderer.flipX ? Vector2.left : Vector2.right);

            // Missile 컴포넌트 초기화 (데미지 계산 포함)
            Missile missile = controller.MissileObject.GetComponent<Missile>();
            missile.Init(dir, GetMissileDamage());

            // 미사일 그래픽 방향 맞춤
            SpriteRenderer missileRend = controller.MissileObject.GetComponent<SpriteRenderer>();
            if (missileRend != null)
            {
                missileRend.flipX = controller.SpriteRenderer.flipX;
            }

            // 미사일 활성화
            controller.MissileObject.SetActive(true);

            // 콤보 단계가 4미만이면 바로 EndAttack, 아니면 약간의 추가 딜레이 후 EndAttack
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

        // ───────────────────────────────────────────────────────
        // 미사일 데미지 계산 함수
        // ───────────────────────────────────────────────────────
        private int GetMissileDamage()
        {
            if (controller.Data == null)
                return 0;

            // Fury 게이지 비율 계산
            float gaugePercent = controller.CurrentFuryAmount / controller.Data.furyMax;

            if (gaugePercent >= 3f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 3f);
            if (gaugePercent >= 2f)
                return Mathf.FloorToInt(controller.Data.attackDamage * 2f);

            return Mathf.FloorToInt(controller.Data.attackDamage);
        }
    }
}
