using UnityEngine;
using DG.Tweening;

public class RandomLocalStrafe2D : MonoBehaviour
{
    [Header("Local X Drift (visual-only)")]
    [Tooltip("Max small step to the LEFT (negative local X).")]
    public float maxLeftStep = 0.25f;

    [Tooltip("Max small drift to the RIGHT (positive local X).")]
    public float maxRightDrift = 0.6f;

    [Tooltip("Chance that a cycle starts with a little left nudge before drifting right.")]
    [Range(0f, 1f)] public float leftNudgeChance = 0.35f;

    [Header("Timing")]
    public Vector2 moveDurRange = new Vector2(0.12f, 0.35f);   // single leg duration
    public Vector2 holdRange    = new Vector2(0.00f, 0.25f);   // pause between legs

    [Header("Ease")]
    public Ease ease = Ease.InOutSine;

    [Header("Advanced")]
    [Tooltip("Clamp final local X offset around base (avoid drifting away forever).")]
    public float clampAbsX = 1.0f;

    private Vector3 _baseLocal;
    private Sequence _seq;

    void Awake()
    {
        _baseLocal = transform.localPosition;
    }

    void OnEnable()
    {
        _baseLocal = transform.localPosition; // rebase if you enable while moving
        StartCycle();
    }

    void OnDisable()
    {
        if (_seq != null) _seq.Kill();
        DOTween.Kill(this);
    }

    void OnDestroy()
    {
        if (_seq != null) _seq.Kill();
        DOTween.Kill(this);
    }

    void LateUpdate()
    {
        // keep the wiggle bounded around base
        var lp = transform.localPosition;
        float dx = Mathf.Clamp(lp.x - _baseLocal.x, -clampAbsX, clampAbsX);
        transform.localPosition = new Vector3(_baseLocal.x + dx, lp.y, lp.z);
    }

    void StartCycle()
    {
        if (_seq != null) _seq.Kill();
        _seq = DOTween.Sequence().SetTarget(this);

        // Randomly decide if we do a small LEFT nudge first
        bool doLeft = Random.value < leftNudgeChance;

        if (doLeft && maxLeftStep > 0f)
        {
            float leftAmt = -Random.Range(maxLeftStep * 0.2f, maxLeftStep);     // small left step
            float leftDur = Random.Range(moveDurRange.x, moveDurRange.y);
            float leftHold = Random.Range(holdRange.x, holdRange.y);

            _seq.Append(transform.DOLocalMoveX(_baseLocal.x + leftAmt, leftDur).SetEase(ease));
            _seq.AppendInterval(leftHold);
        }

        // Then drift right a little (or sometimes just tiny)
        float rightAmt = Random.Range(maxRightDrift * 0.15f, maxRightDrift);
        float rightDur = Random.Range(moveDurRange.x, moveDurRange.y);
        float rightHold = Random.Range(holdRange.x, holdRange.y);

        _seq.Append(transform.DOLocalMoveX(_baseLocal.x + rightAmt, rightDur).SetEase(ease));
        _seq.AppendInterval(rightHold);

        // Ease back near base (not always perfectly to 0 so it feels alive)
        float settleAmt = Random.Range(-maxLeftStep * 0.1f, maxRightDrift * 0.2f);
        float settleDur = Random.Range(moveDurRange.x, moveDurRange.y);

        _seq.Append(transform.DOLocalMoveX(_baseLocal.x + settleAmt, settleDur).SetEase(ease));

        // When done, start another randomized cycle
        _seq.OnComplete(StartCycle);
    }
}
