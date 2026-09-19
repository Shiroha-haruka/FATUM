using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance { get; private set; }

    [Header("カーソル画像")]
    public Texture2D cursorTexture;

    [Header("クリック位置")]
    public Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("カーソル画像が設定されていません");
            return;
        }

        Cursor.SetCursor(
            cursorTexture,
            hotspot,
            CursorMode.Auto
        );
    }
}