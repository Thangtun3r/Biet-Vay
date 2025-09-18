using UnityEngine;
using DG.Tweening;
using Yarn.Unity; // <-- add this for DOTween

public class GachaObject : MonoBehaviour
{
    
    public DialogueRunner dialogueRunner;
    
    [Header("Where to spawn the selected prefab")]
    [SerializeField] private Transform spawnRoot;  // leave null = use this.transform

    [Header("Debug / Info")]
    [SerializeField] private string currentName;

    private GameObject currentInstance;

    private void OnEnable()  => GachaMachine.OnGachaRolled += Apply;
    private void OnDisable() => GachaMachine.OnGachaRolled -= Apply;

    public void Apply(GachaObjectSO data)
    {
        if (data == null || data.prefab == null) return;

        // Destroy old
        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        // Spawn new as child
        var parent = spawnRoot ? spawnRoot : transform;
        currentInstance = Instantiate(data.prefab, parent);

        // Reset transform under parent
        currentInstance.transform.localPosition = Vector3.zero;
        currentInstance.transform.localRotation = Quaternion.identity;

        // Start at scale 0
        currentInstance.transform.localScale = Vector3.zero;

        // Animate to full size (Vector3.one = initial scale)
        currentInstance.transform
            .DOScale(Vector3.one, 0.5f) // duration 0.5s
            .SetEase(Ease.OutBack);     // nice "pop" effect

        // Set the current name for reference
        currentName = data.displayName;

        // Check if the displayed gacha item is the favorite one
        CheckForFavoriteGashapon(data);
    }
    
    private void CheckForFavoriteGashapon(GachaObjectSO data)
    {
        if (data.isFavorite)
        {
            dialogueRunner.VariableStorage.SetValue("$favoriteGashapon", true);
        }
        else
        {
            dialogueRunner.VariableStorage.SetValue("$favoriteGashapon", false);
        }
    }
}