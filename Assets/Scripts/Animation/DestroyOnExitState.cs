using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExitState : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    // 当动画状态退出时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 销毁挂载Animator组件的那个GameObject
        Destroy(animator.gameObject);
    }
}
