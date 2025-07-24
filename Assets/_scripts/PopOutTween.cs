using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PopOutRectTween : MonoBehaviour
{
    public static PopOutRectTween Instance { get; private set; }

    [Header("Toggle Size Settings")]
    public Vector2 expandedSize      = new Vector2(500f, 300f);
    public float   toggleDuration    = 0.4f;
    public Ease    toggleEase        = Ease.InOutQuad;

    [Header("Toggle Buffer (seconds)")]
    [Tooltip("Delay before enabling/disabling toggleObjects")]
    public float toggleBufferSeconds = 0.2f;

    [Header("Objects to Toggle")]
    [Tooltip("These will be disabled when the panel is expanded, re-enabled when collapsed.")]
    public GameObject[] toggleObjects;

    [Header("Script to Toggle")]
    [Tooltip("Assign the script you want frozen/unfrozen (e.g. FirstPersonController)")]
    public MonoBehaviour scriptToToggle;

    private RectTransform rectTransform;
    private Vector2        originalSize;
    private bool           isExpanded;
    private Coroutine      _bufferCoroutine;

    private void Awake()
    {
        // singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rectTransform = GetComponent<RectTransform>();
        originalSize  = rectTransform.sizeDelta;

        // start collapsed with sub-objects visible
        rectTransform.sizeDelta = originalSize;
        ToggleObjects(true);

        // initial freeze: disable your movement/look script
        if (scriptToToggle != null)
            scriptToToggle.enabled = false;

        // cursor visible & unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void ToggleOn()
    {
        if (isExpanded) return;

        // unfreeze: enable your script
        if (scriptToToggle != null)
            scriptToToggle.enabled = true;

        // lock & hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // animate expand
        KillBuffer();
        rectTransform.DOKill();
        rectTransform
            .DOSizeDelta(expandedSize, toggleDuration)
            .SetEase(toggleEase);

        // then hide sub-objects
        _bufferCoroutine = StartCoroutine(BufferToggle(false));

        isExpanded = true;
    }

    public void ToggleOff()
    {
        if (!isExpanded) return;

        // freeze: disable your script
        if (scriptToToggle != null)
            scriptToToggle.enabled = false;

        // unlock & show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // animate collapse
        KillBuffer();
        rectTransform.DOKill();
        rectTransform
            .DOSizeDelta(originalSize, toggleDuration)
            .SetEase(toggleEase);

        // then show sub-objects
        _bufferCoroutine = StartCoroutine(BufferToggle(true));

        isExpanded = false;
    }

    private IEnumerator BufferToggle(bool enable)
    {
        yield return new WaitForSeconds(toggleBufferSeconds);
        ToggleObjects(enable);
    }

    private void KillBuffer()
    {
        if (_bufferCoroutine != null)
        {
            StopCoroutine(_bufferCoroutine);
            _bufferCoroutine = null;
        }
    }

    private void ToggleObjects(bool enable)
    {
        if (toggleObjects == null) return;
        foreach (var obj in toggleObjects)
            if (obj != null)
                obj.SetActive(enable);
    }
}
