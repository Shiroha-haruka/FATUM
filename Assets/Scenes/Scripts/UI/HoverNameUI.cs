using TMPro;
using UnityEngine;

public class HoverNameUI : MonoBehaviour
{
    public static HoverNameUI Instance;

    public TextMeshProUGUI hoverText;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    Camera cam;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;
        hoverText.gameObject.SetActive(false);
    }

    public void Show(string text, Vector3 worldPos)
    {
        hoverText.text = text;
        hoverText.gameObject.SetActive(true);

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos + offset);
        hoverText.transform.position = screenPos;
    }

    public void Hide()
    {
        hoverText.gameObject.SetActive(false);
    }
}
