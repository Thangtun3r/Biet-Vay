using UnityEngine;
using DG.Tweening;

public class GachaSpawnAnim : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] float spawnDuration = 0.4f;
    [SerializeField] Ease spawnEase = Ease.OutBack;
    private float startTilt = 87.6f;   // random degrees for initial tilt

    [Header("Idle Wobble")]
    private float wobbleAmount = 13.2f;   // degrees
    private float wobbleDuration = 3.5f; // seconds

    Sequence seq;
    Vector3 originalLocalEuler;
    Vector3 originalLocalScale;

    void OnEnable()
    {
        transform.DOKill();
        seq?.Kill();

        // Cache original LOCAL transform (relative to parent)
        originalLocalEuler = transform.localEulerAngles;
        originalLocalScale = transform.localScale;

        // Start small + with a small random LOCAL tilt
        Vector3 rand = new Vector3(
            Random.Range(-startTilt, startTilt),
            0f,                                   // keep Y fixed if you only want left/right & up/down
            Random.Range(-startTilt, startTilt)
        );
        transform.localEulerAngles = originalLocalEuler + rand;
        transform.localScale = Vector3.zero;

        // Tween back to the ORIGINAL LOCAL rotation & scale
        seq = DOTween.Sequence();
        seq.Join(transform.DOScale(originalLocalScale, spawnDuration).SetEase(spawnEase));
        seq.Join(transform.DOLocalRotate(originalLocalEuler, spawnDuration).SetEase(Ease.OutQuad));

        // Idle wobble around the ORIGINAL LOCAL rotation (no drift)
        seq.AppendCallback(() =>
        {
            Vector3 target = originalLocalEuler + new Vector3(wobbleAmount, 0f, -wobbleAmount);
            transform.DOLocalRotate(target, wobbleDuration)
                     .SetEase(Ease.InOutSine)
                     .SetLoops(-1, LoopType.Yoyo);
        });
    }

    void OnDisable()
    {
        transform.DOKill();
        seq?.Kill();
        // snap back (optional)
        transform.localEulerAngles = originalLocalEuler;
        transform.localScale = originalLocalScale;
    }
}
