using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("Cursor Textures")]
    public Texture2D normalCursor;       
    public Texture2D interactableCursor; 
    public Texture2D clickCursor;        

    [Header("Settings")]
    [Range(0.1f, 5f)] public float cursorScale = 1f; // Size multiplier
    public Vector2 hotspot = Vector2.zero; 
    public CursorMode cursorMode = CursorMode.Auto;

    private enum CursorState { Normal, Interactable, Click }
    private CursorState currentState;

    private void Start()
    {
        SetCursorState(CursorState.Normal);
    }

    private void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked || !Cursor.visible)
            return; // Skip when locked/invisible

        if (Input.GetMouseButtonDown(0))
        {
            SetCursorState(CursorState.Click);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SetCursorState(currentState == CursorState.Interactable ? CursorState.Interactable : CursorState.Normal);
        }
    }
    
    public void OnHoverInteractable(bool isHovering)
    {
        if (Cursor.lockState == CursorLockMode.Locked || !Cursor.visible)
            return;

        if (isHovering)
            SetCursorState(CursorState.Interactable);
        else
            SetCursorState(CursorState.Normal);
    }

    private void SetCursorState(CursorState state)
    {
        currentState = state;
        Texture2D tex = null;

        switch (state)
        {
            case CursorState.Normal: tex = normalCursor; break;
            case CursorState.Interactable: tex = interactableCursor; break;
            case CursorState.Click: tex = clickCursor; break;
        }

        if (tex != null)
            Cursor.SetCursor(ResizeTexture(tex, cursorScale), hotspot, cursorMode);
    }

    private Texture2D ResizeTexture(Texture2D source, float scale)
    {
        int newWidth = Mathf.RoundToInt(source.width * scale);
        int newHeight = Mathf.RoundToInt(source.height * scale);

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(newWidth, newHeight, source.format, false);
        newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTex;
    }
}
