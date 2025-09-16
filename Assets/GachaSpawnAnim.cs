using UnityEngine;
using DG.Tweening;

public class GachaSpawnAnim : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private void OnEnable()
    {
        // Start from scale 0
        transform.localScale = Vector3.zero;

        // Animate to its initial scale
        transform.DOScale(Vector3.one, duration).SetEase(ease);
    }
}