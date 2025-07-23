using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class WordPackageRandomizer : MonoBehaviour
{
    [Header("References")]
    public WordsPackage[] availablePackages; // All your test packages
    public WordPoolManager wordPoolManager;
    public WordsPooling wordPooling;

    [Header("Settings")]
    public float showTime = 4f;        // how long a package stays visible
    public float delayWhenCleared = 1f; // how long the pool stays empty before next

    private Coroutine randomizeRoutine;

    private void Start()
    {
        randomizeRoutine = StartCoroutine(RandomizePackagesRoutine());
    }

    private IEnumerator RandomizePackagesRoutine()
    {
        while (true)
        {
            // Pick a random package & show it
            WordsPackage randomPackage = availablePackages[Random.Range(0, availablePackages.Length)];
            wordPoolManager.CreateSentence(randomPackage);

            // Show it for X seconds
            yield return new WaitForSeconds(showTime);

            // Clear the pool (you can plug in your custom clear function here)
            ClearPoolWithDelay();

            // Wait some time while it's empty
            yield return new WaitForSeconds(delayWhenCleared);
        }
    }

    private void ClearPoolWithDelay()
    {
        wordPooling.clearPool();
        
    }
    
}