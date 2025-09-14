// CharacterColorDatabase.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character Color Database", fileName = "CharacterColorDB")]
public class CharacterColorDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string characterName;
        [Tooltip("Use #RRGGBB or #AARRGGBB")]
        public string hexColor = "#FFFFFFFF";
    }

    public List<Entry> entries = new();

    // Runtime cache (case-insensitive keys)
    private Dictionary<string, Color> _map;

    void OnValidate() { _map = null; } // rebuild when edited in inspector

    public void Rebuild()
    {
        _map = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (string.IsNullOrWhiteSpace(e.characterName)) continue;
            var key = e.characterName.Trim();
            if (ColorUtility.TryParseHtmlString(e.hexColor, out var c))
                _map[key] = c;
            else
                Debug.LogWarning($"[CharacterColorDatabase] Invalid hex for '{key}': {e.hexColor}");
        }
    }

    public bool TryGet(string name, out Color color)
    {
        if (_map == null) Rebuild();
        if (string.IsNullOrWhiteSpace(name)) { color = default; return false; }
        return _map.TryGetValue(name.Trim(), out color);
    }
}