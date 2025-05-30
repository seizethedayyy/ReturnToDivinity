using UnityEngine;

public class HitLogger : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("[HitLogger] ▶ Hit 상태 진입");
    }
}