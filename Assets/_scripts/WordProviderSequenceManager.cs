using UnityEngine;

public class WordProviderSequenceManager : MonoBehaviour
{
    [Tooltip("Drag-and-drop all your WordPackageProvider objects here, in order.")]
    public WordPackageProvider[] providers;

    private int _current = 0;

    // ✅ Global flag accessible anywhere
    public static bool AllProvidersCompleted { get; private set; } = false;

    private void Awake()
    {
        // Reset at start
        AllProvidersCompleted = false;

        // Disable all providers initially
        for (int i = 0; i < providers.Length; i++)
        {
            providers[i].gameObject.SetActive(false);
        }

        // Enable the first provider if available
        if (providers.Length > 0)
        {
            ActivateProvider(0);
        }
    }

    private void ActivateProvider(int index)
    {
        var prov = providers[index];
        prov.gameObject.SetActive(true);

        // Subscribe to the event
        prov.OnTypewriterComplete += HandleProviderComplete;
    }

    private void HandleProviderComplete(WordPackageProvider finished)
    {
        // Unsubscribe so we don’t double-fire
        finished.OnTypewriterComplete -= HandleProviderComplete;

        // Move to next one
        _current++;

        if (_current < providers.Length)
        {
            ActivateProvider(_current);
        }
        else
        {
            // ✅ All providers are done
            AllProvidersCompleted = true;
            Debug.Log("✅ All WordPackageProviders have been completed!");
        }
    }
}