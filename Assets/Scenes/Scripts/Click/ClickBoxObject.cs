using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ClickBoxObject : MonoBehaviour
{
    public Vector2 Size = new Vector2(1f, 1f);

    public TextMeshPro hoverNameText;

    public bool showClickMessage = true;

    [Header("Hover時に下のログ欄へ表示する文章")]
    [TextArea]
    public string hoverLogMessage;

    public UnityEvent onClick;


    void Start()
    {
        if (hoverNameText != null)
            hoverNameText.gameObject.SetActive(false);
    }


    public bool IsClickInside(Vector2 clickPosition)
    {
        Vector2 objPosition = transform.position;

        return
            clickPosition.x > objPosition.x - Size.x / 2f &&
            clickPosition.x < objPosition.x + Size.x / 2f &&
            clickPosition.y > objPosition.y - Size.y / 2f &&
            clickPosition.y < objPosition.y + Size.y / 2f;
    }


    public int GetSortingOrder()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return 0;

        return spriteRenderer.sortingOrder;
    }


    public void ShowName()
    {
        if (hoverNameText != null)
            hoverNameText.gameObject.SetActive(true);
    }


    public void HideName()
    {
        if (hoverNameText != null)
            hoverNameText.gameObject.SetActive(false);
    }


    public void Click()
    {
        Debug.Log(gameObject.name + " がクリックされた！");

        onClick?.Invoke();
    }
}