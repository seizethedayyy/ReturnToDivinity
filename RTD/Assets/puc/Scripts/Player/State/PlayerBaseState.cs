using UnityEngine;

namespace Player.States
{
    public abstract class PlayerBaseState
    {
        // 상태가 속할 PlayerController 레퍼런스
        protected PlayerController controller;
        // 상태 이름(디버그용)
        public abstract string StateName { get; }

        // 생성자: 각 상태는 PlayerController를 전달받아야 함
        protected PlayerBaseState(PlayerController controller)
        {
            this.controller = controller;
        }

        // 상태 진입 시(한 번만 호출)
        public virtual void Enter()
        {
            Debug.Log($"[PlayerState] Enter {StateName}");
        }

        // 매 프레임 호출
        public abstract void Execute();

        // 상태 종료 직전에 호출
        public virtual void Exit()
        {
            Debug.Log($"[PlayerState] Exit {StateName}");
        }
    }
}
