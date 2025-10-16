// NPCChatting.cs
// Attach this to any NPC with a SpriteRenderer.
// NPC will "chat" (animate sprites), then take breaks (hide), and always face the camera.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCChatting : MonoBehaviour
{
    [Header("Talking Sprites")]
    [Tooltip("Mouth / talking frames for animation.")]
    [SerializeField] private Sprite[] talkingSprites;

    [Header("Timing Settings")]
    [Tooltip("Time between sprite changes while talking (min, max).")]
    [SerializeField] private Vector2 talkFrameInterval = new Vector2(0.05f, 0.15f);

    [Tooltip("How long NPC keeps talking before taking a break (min, max).")]
    [SerializeField] private Vector2 talkBurstDuration = new Vector2(1.5f, 3.0f);

    [Tooltip("How long NPC stays silent (sprite disabled) between talking bursts (min, max).")]
    [SerializeField] private Vector2 breakDuration = new Vector2(1.0f, 4.0f);

    [Header("Billboard Settings")]
    [Tooltip("If true, this NPC will always face the main camera.")]
    [SerializeField] private bool enableBillboard = true;

    [Tooltip("If true, only rotates on Y-axis (good for top-down / 3D).")]
    [SerializeField] private bool lockVerticalRotation = true;

    private SpriteRenderer spriteRenderer;
    private Coroutine chatRoutine;
    private bool isActive = true;
    private Transform cam;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void OnEnable()
    {
        StartChatting();
    }

    void OnDisable()
    {
        StopChatting();
    }

    void LateUpdate()
    {
        if (enableBillboard && cam != null)
        {
            Vector3 lookDir = cam.position - transform.position;

            if (lockVerticalRotation)
                lookDir.y = 0f; // ignore tilt

            if (lookDir.sqrMagnitude > 0.001f)
                transform.forward = -lookDir.normalized; // face the camera
        }
    }

    /// <summary>Start the random talk/break loop.</summary>
    public void StartChatting()
    {
        if (chatRoutine != null) return;
        isActive = true;
        chatRoutine = StartCoroutine(ChatLoop());
    }

    /// <summary>Stop all talking and hide the sprite.</summary>
    public void StopChatting()
    {
        isActive = false;
        if (chatRoutine != null)
        {
            StopCoroutine(chatRoutine);
            chatRoutine = null;
        }
        spriteRenderer.enabled = false;
    }

    private IEnumerator ChatLoop()
    {
        while (isActive)
        {
            // === TALKING ===
            spriteRenderer.enabled = true;
            float talkTime = Random.Range(talkBurstDuration.x, talkBurstDuration.y);
            float timer = 0f;

            while (timer < talkTime)
            {
                if (talkingSprites.Length > 0)
                {
                    spriteRenderer.sprite = talkingSprites[Random.Range(0, talkingSprites.Length)];
                }

                float frameDelay = Random.Range(talkFrameInterval.x, talkFrameInterval.y);
                timer += frameDelay;
                yield return new WaitForSeconds(frameDelay);
            }

            // === BREAK ===
            spriteRenderer.enabled = false;
            float breakTime = Random.Range(breakDuration.x, breakDuration.y);
            yield return new WaitForSeconds(breakTime);
        }
    }
}
