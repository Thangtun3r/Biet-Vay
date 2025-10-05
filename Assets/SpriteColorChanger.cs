using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SpriteButton
{
    public string nameLabel;
    public int typeID;
    public Sprite sprite;
    public string hexColor = "#FFFFFF"; // optional color for wrapper (in hex)
}

public class SpriteColorChanger : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Sprite Mappings")]
    public List<SpriteButton> spriteMappings = new List<SpriteButton>();

    [Header("UI Targets")]
    public Image buttonVisual; // changes sprite
    public Image buttonWrapper; // changes color

    [Header("Defaults")]
    public Sprite defaultSprite;
    public Color defaultWrapperColor = Color.white;

    private Dictionary<int, SpriteButton> spriteDict = new Dictionary<int, SpriteButton>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Build dictionary for quick lookup
        foreach (var mapping in spriteMappings)
        {
            if (!spriteDict.ContainsKey(mapping.typeID))
                spriteDict.Add(mapping.typeID, mapping);
        }

        // Capture defaults if not manually set
        if (defaultSprite == null)
        {
            if (spriteRenderer != null)
                defaultSprite = spriteRenderer.sprite;
            else if (buttonVisual != null)
                defaultSprite = buttonVisual.sprite;
        }

        if (buttonWrapper != null)
            defaultWrapperColor = buttonWrapper.color;
    }

    /// <summary>
    /// Change sprite and color according to ID
    /// </summary>
    public void SetType(int id)
    {
        if (spriteDict.TryGetValue(id, out var mapping))
        {
            // --- Sprite change ---
            Sprite newSprite = mapping.sprite;
            if (spriteRenderer != null)
                spriteRenderer.sprite = newSprite;
            if (buttonVisual != null)
                buttonVisual.sprite = newSprite;

            // --- Wrapper color change ---
            if (buttonWrapper != null && !string.IsNullOrEmpty(mapping.hexColor))
            {
                if (ColorUtility.TryParseHtmlString(mapping.hexColor, out var parsedColor))
                    buttonWrapper.color = parsedColor;
                else
                    Debug.LogWarning($"Invalid hex color '{mapping.hexColor}' for ID {id}", this);
            }
        }
        else
        {
            Debug.LogWarning($"No sprite mapping found for type ID {id}", this);
        }
    }

    /// <summary>
    /// Reset visuals to default sprite and color
    /// </summary>
    public void ResetSprite()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;

        if (buttonVisual != null)
            buttonVisual.sprite = defaultSprite;

        if (buttonWrapper != null)
            buttonWrapper.color = defaultWrapperColor;
    }
}
