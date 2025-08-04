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

    [MenuItem("Tools/Word Package Editor")]
    public static void ShowWindow()
    {
        GetWindow<SentenceToWordEditor>("Word Package Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Sentence Input", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Press ENTER for new block or use ';' to separate inline.\nYou can edit blocks below in real-time.", MessageType.Info);

        // Raw input text
        EditorGUI.BeginChangeCheck();
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(80));
        if (EditorGUI.EndChangeCheck())
        {
            AutoParseToBlocks();
        }

        GUILayout.Space(10);

        // ======= Preview & Editable Blocks =======
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
                    break; // avoid layout errors
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("No blocks yet. Type something above.", EditorStyles.miniLabel);
        }

        if (GUILayout.Button("➕ Add New Block"))
        {
            editableBlocks.Add("New Block");
        }

        GUILayout.Space(10);

        // ======= Save Location =======
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
        if (GUILayout.Button("✅ Generate Words Package"))
        {
            if (autoReset) ResetGeneratedWords();
            GenerateWordsPackage();
        }
        GUI.enabled = true;

        GUILayout.Space(5);

        // Reset preview
        if (GUILayout.Button("🔄 Reset Preview Blocks"))
        {
            editableBlocks.Clear();
        }
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

    // ✅ Generate ONE WordsPackage.asset
    void GenerateWordsPackage()
    {
        if (editableBlocks.Count == 0)
        {
            Debug.LogWarning("⚠️ No blocks to generate!");
            return;
        }

        // Create the package ScriptableObject
        WordsPackage package = ScriptableObject.CreateInstance<WordsPackage>();

        for (int i = 0; i < editableBlocks.Count; i++)
        {
            WordsTemplate temp = new WordsTemplate();
            temp.word = editableBlocks[i];
            temp.id = i + 1;
            package.words.Add(temp);
        }

        // ✅ Store total word count
        package.wordsCount = editableBlocks.Count;

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] split = savePath.Split('/');
            string parent = "Assets";
            for (int i = 1; i < split.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(parent + "/" + split[i]))
                    AssetDatabase.CreateFolder(parent, split[i]);
                parent += "/" + split[i];
            }
        }

        // ✅ Generate a unique ID suffix
        string uniqueID = System.DateTime.Now.ToString("yyyyMMdd_HHmmss"); 
        string assetName = $"WordsPackage_{uniqueID}.asset";

        string assetPath = Path.Combine(savePath, assetName);

        AssetDatabase.CreateAsset(package, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Created WordsPackage ({editableBlocks.Count} words) → {assetPath}");
    }



    void ResetGeneratedWords()
    {
        Debug.Log("🔄 Resetting generated word list (Preview stays intact).");
    }
}
