using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveAnimManager : MonoBehaviour
{
    [SerializeField] private Animator resolveAnimator;
    public static bool isResolved = false;
    
    private void OnEnable()
    {
        GameManager.OnResolveAnim += PlayResolveAnimation;
    }
    private void OnDisable()
    {
        GameManager.OnResolveAnim -= PlayResolveAnimation;
    }
    private void PlayResolveAnimation()
    {
        if (resolveAnimator != null)
        {
            resolveAnimator.enabled = true;
            isResolved = true;
            resolveAnimator.SetBool("isResolved", true);
        }
    }
}

