using UnityEngine;
using System.Collections.Generic;

public class WordPackageLoader : MonoBehaviour
{
    [System.Serializable]
    public class PackageEntry
    {
        public string packageID;         // ID used to request it
        public WordsPackage package;     // The actual ScriptableObject
    }

    [Header("Package Mappings")]
    public List<PackageEntry> packageMappings = new List<PackageEntry>();

    [Header("References")]
    public WordPoolManager wordPoolManager;     // Manager that spawns words
    public WordsPooling wordPooling;            // Logic pool
    public WordsPooling visualPooling;          // Visual pool (optional)

    private Dictionary<string, WordsPackage> packageLookup = new Dictionary<string, WordsPackage>();

    private void Awake()
    {
        BuildLookup();
    }

    private void Start()
    {
        // ✅ Auto-load the FIRST package if available
        if (packageMappings.Count > 0 && packageMappings[0].package != null)
        {
            Debug.Log($"✅ Auto-loading first package: {packageMappings[0].packageID}");
            ShowPackageByID(packageMappings[0].packageID);
        }

        // ✅ Subscribe to global event to clear pools
        WordPoolEvents.OnClearPools += ClearPools;
    }

    private void OnDestroy()
    {
        // ✅ Unsubscribe to avoid memory leaks
        WordPoolEvents.OnClearPools -= ClearPools;
    }

    private void BuildLookup()
    {
        packageLookup.Clear();

        foreach (var entry in packageMappings)
        {
            if (entry.package == null || string.IsNullOrEmpty(entry.packageID))
            {
                Debug.LogWarning("⚠ Found an empty PackageEntry! Please check your mappings.");
                continue;
            }

            if (!packageLookup.ContainsKey(entry.packageID))
                packageLookup.Add(entry.packageID, entry.package);
            else
                Debug.LogWarning($"⚠ Duplicate package ID detected: {entry.packageID}");
        }
    }

    /// ✅ Public method to load a package by ID
    public void ShowPackageByID(string packageID)
    {
        if (!packageLookup.TryGetValue(packageID, out WordsPackage pkg))
        {
            Debug.LogWarning($"❌ WordsPackage with ID '{packageID}' not found!");
            return;
        }

        // ✅ Clear existing before loading new
        ClearPools();

        // ✅ Create words from the package
        wordPoolManager.CreateSentence(pkg);
    }

    /// ✅ Clear logic + visual pools
    private void ClearPools()
    {
        Debug.Log("🧹 Clearing all word pools...");
        if (wordPooling != null) wordPooling.clearPool();
        if (visualPooling != null) visualPooling.clearPool();
    }
}
