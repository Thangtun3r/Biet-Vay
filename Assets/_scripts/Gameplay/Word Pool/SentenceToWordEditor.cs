using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SentenceToWordEditor : EditorWindow
{
    private string inputText = "";
    private string savePath = "Assets/WordTemplates";
    private bool autoReset = true;

    private List<string> editableBlocks = new List<string>(); // Editable blocks
    private WordPoolManager wordPoolManager;

    [MenuItem("Tools/Word Pool Editor")]
    public static void ShowWindow()
    {
        GetWindow<SentenceToWordEditor>("Word Pool Manager");
    }

    void OnGUI()
    {
        GUILayout.Label("Sentence Input", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Press ENTER for new block or use ';' to separate inline.\nYou can edit blocks below in real-time.", MessageType.Info);

        // Text area for raw input
        EditorGUI.BeginChangeCheck();
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(80));
        if (EditorGUI.EndChangeCheck())
        {
            AutoParseToBlocks();
        }

        GUILayout.Space(10);

        // ======= Editable Array-like UI =======
        GUILayout.Label($"🟦 Preview & Edit Blocks ({editableBlocks.Count})", EditorStyles.boldLabel);

        if (editableBlocks.Count > 0)
        {
            for (int i = 0; i < editableBlocks.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Editable field for this block
                editableBlocks[i] = EditorGUILayout.TextField($"[{i + 1}]", editableBlocks[i]);

                // Remove button
                if (GUILayout.Button("❌", GUILayout.Width(30)))
                {
                    editableBlocks.RemoveAt(i);
                    GUI.FocusControl(null); // Avoid Unity focus bug
                    break; // Avoid layout errors after removal
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("No blocks yet. Type something above.", EditorStyles.miniLabel);
        }

        // Add new block manually
        if (GUILayout.Button("➕ Add New Block"))
        {
            editableBlocks.Add("New Block");
        }

        GUILayout.Space(10);

        // ======= Save Location UI =======
        GUILayout.Label("Save Location", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Current Path: " + savePath, EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Change Folder", GUILayout.Width(120)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("Please select a folder inside your project's Assets folder!");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        autoReset = EditorGUILayout.Toggle("Auto Reset Before New", autoReset);

        // ======= Generate Button =======
        GUI.enabled = editableBlocks.Count > 0;
        if (GUILayout.Button("✅ Generate Words"))
        {
            if (autoReset) ResetGeneratedWords();
            GenerateWordsFromBlocks();
        }
        GUI.enabled = true;

        GUILayout.Space(5);

        // Reset blocks
        if (GUILayout.Button("🔄 Reset Preview Blocks"))
        {
            editableBlocks.Clear();
        }

        GUILayout.Space(10);

        GUILayout.Label("Generated Words: (After Creation)", EditorStyles.boldLabel);

        if (GUILayout.Button("🎲 Randomize & Assign To WordPoolManager"))
        {
            RandomizeAndAssign();
        }

        GUILayout.Space(10);
        wordPoolManager = (WordPoolManager)EditorGUILayout.ObjectField("Word Pool Manager", wordPoolManager, typeof(WordPoolManager), true);
    }

    // 🔹 Auto-parsing when typing new text
    void AutoParseToBlocks()
    {
        editableBlocks.Clear();

        if (string.IsNullOrWhiteSpace(inputText)) return;

        // Split by lines
        var lines = inputText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Split by semicolon inside each line
            var parts = line.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                string clean = p.Trim();
                if (!string.IsNullOrEmpty(clean))
                    editableBlocks.Add(clean);
            }
        }
    }

    void GenerateWordsFromBlocks()
    {
        if (editableBlocks.Count == 0)
        {
            Debug.LogWarning("⚠️ No blocks to generate!");
            return;
        }

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string parent = Path.GetDirectoryName(savePath);
            string newFolder = Path.GetFileName(savePath);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                AssetDatabase.CreateFolder("Assets", "WordTemplates");
                parent = "Assets/WordTemplates";
            }
            AssetDatabase.CreateFolder(parent, newFolder);
        }

        for (int i = 0; i < editableBlocks.Count; i++)
        {
            WordsTemplate wordAsset = ScriptableObject.CreateInstance<WordsTemplate>();
            wordAsset.word = editableBlocks[i];
            wordAsset.id = i + 1;

            // Safe filename
            string safeWordName = new string(editableBlocks[i].Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrEmpty(safeWordName)) safeWordName = "Word";

            string assetName = $"Word_{i + 1}_{safeWordName}.asset";
            string fullPath = Path.Combine(savePath, assetName);
            AssetDatabase.CreateAsset(wordAsset, fullPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Generated {editableBlocks.Count} ScriptableObjects in: {savePath}");
    }

    void ResetGeneratedWords()
    {
        Debug.Log("🔄 Resetting generated word list (Preview stays intact).");
    }

    void RandomizeAndAssign()
    {
        if (wordPoolManager == null)
        {
            Debug.LogError("❌ Assign a WordPoolManager before randomizing!");
            return;
        }

        if (editableBlocks.Count == 0)
        {
            Debug.LogWarning("⚠️ No blocks available to assign!");
            return;
        }

        // Randomize order
        var randomized = editableBlocks.OrderBy(x => Random.value).ToList();

        // Create temporary WordsTemplates in memory (not saved as assets)
        List<WordsTemplate> tempList = new List<WordsTemplate>();
        for (int i = 0; i < randomized.Count; i++)
        {
            WordsTemplate temp = ScriptableObject.CreateInstance<WordsTemplate>();
            temp.word = randomized[i];
            temp.id = i + 1;
            tempList.Add(temp);
        }

        wordPoolManager.wordsTemplate = tempList.ToArray();
        EditorUtility.SetDirty(wordPoolManager);

        Debug.Log("🎲 Randomized words assigned to WordPoolManager!");
    }
}
