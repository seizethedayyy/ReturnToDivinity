using UnityEngine;

namespace Player.States
{
    public abstract class PlayerBaseState
    {
        // ���°� ���� PlayerController ���۷���
        protected PlayerController controller;
        // ���� �̸�(����׿�)
        public abstract string StateName { get; }

        // ������: �� ���´� PlayerController�� ���޹޾ƾ� ��
        protected PlayerBaseState(PlayerController controller)
        {
            this.controller = controller;
        }

        // ���� ���� ��(�� ���� ȣ��)
        public virtual void Enter()
        {
            Debug.Log($"[PlayerState] Enter {StateName}");
        }

        // �� ������ ȣ��
        public abstract void Execute();

        // ���� ���� ������ ȣ��
        public virtual void Exit()
        {
            Debug.Log($"[PlayerState] Exit {StateName}");
        }
    }
}
