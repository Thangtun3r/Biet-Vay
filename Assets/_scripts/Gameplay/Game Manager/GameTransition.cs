using System;
using UnityEngine;
using DG.Tweening;

public class GameTransition : MonoBehaviour
{
    public static GameTransition Instance { get; private set; }

    public RectTransform targetRect;
    public Vector2 startSize = Vector2.zero;     // Collapsed size
    public Vector2 endSize = new Vector2(500, 300); // Full expanded size
    public float transitionDuration = 0.5f;
    
    public static event Action OnExpandStarted;
    public static event Action OnCollapseStarted;
    public static event Action OnCollapseCompleted;
    public static event Action OnExpandCompleted;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //targetRect.sizeDelta = startSize;
    }

    public void Expand()
    {
        targetRect.DOSizeDelta(endSize, transitionDuration)
            .SetEase(Ease.InOutSine).OnComplete(() => {
            OnExpandCompleted?.Invoke();
        });
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        OnExpandStarted?.Invoke();
    }

    public void Collapse()
    {
        OnCollapseStarted?.Invoke();
        targetRect.DOSizeDelta(startSize, transitionDuration).SetEase(Ease.InOutSine)
            .OnComplete(() => {
                OnCollapseCompleted?.Invoke();
            });
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
    }
}