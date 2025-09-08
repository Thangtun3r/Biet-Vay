using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndFade : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("AnimID", 0);
    }
}
