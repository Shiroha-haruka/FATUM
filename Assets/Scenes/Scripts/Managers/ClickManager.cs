using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance { get; private set; }

    private ClickBoxObject[] clickBoxObjects;

    public TextMeshProUGUI hoverText;
    public TextMeshProUGUI clickText;

    public int maxMessageCount = 3;

    private List<string> messageLog = new List<string>();

    private ClickBoxObject currentHoverObject;

    private Camera mainCamera;

    // DangerMarkなどのHover説明
    private string hoverEventMessage = "";

    // 結果などの固定メッセージ
    private string eventMessage = "";

    private Coroutine eventMessageCoroutine;


    private void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        mainCamera = Camera.main;

        // 非表示中(GameRootなど)も取得
        clickBoxObjects = FindObjectsByType<ClickBoxObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
    }


    void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;

        Vector3 target = mainCamera.ScreenToWorldPoint(mousePosition);

        CheckHover(target);

        if (Input.GetMouseButtonDown(0))
        {
            CheckClick(target);
        }
    }


    //-------------------------------------------------
    // カーソル
    //-------------------------------------------------

    void CheckHover(Vector3 target)
    {
        ClickBoxObject hoverObject = GetTopObject(target);

        if (currentHoverObject != hoverObject)
        {
            if (currentHoverObject != null)
            {
                currentHoverObject.HideName();
            }

            currentHoverObject = hoverObject;

            if (currentHoverObject != null)
            {
                currentHoverObject.ShowName();
            }
        }


        // 上側などに使っている従来のHoverText
        if (hoverText != null)
        {
            if (hoverObject != null)
            {
                hoverText.text =
                    hoverObject.gameObject.name +
                    " にカーソルが合っています";
            }
            else
            {
                hoverText.text = "";
            }
        }


        //-------------------------------------------------
        // 下のログ欄にHover説明を表示
        //-------------------------------------------------

        hoverEventMessage = "";

        if (hoverObject != null)
        {
            if (!string.IsNullOrEmpty(hoverObject.hoverLogMessage))
            {
                hoverEventMessage = hoverObject.hoverLogMessage;
            }
        }

        UpdateClickText();
    }


    //-------------------------------------------------
    // クリック
    //-------------------------------------------------

    void CheckClick(Vector3 target)
    {
        ClickBoxObject clickedObject = GetTopObject(target);

        if (clickedObject == null)
            return;

        clickedObject.Click();

        if (clickText != null && clickedObject.showClickMessage)
        {
            string message =
                clickedObject.gameObject.name +
                " をクリックしました！";

            StartCoroutine(ShowClickText(message));
        }
    }


    //-------------------------------------------------
    // 通常のクリック履歴
    //-------------------------------------------------

    IEnumerator ShowClickText(string message)
    {
        messageLog.Add(message);

        if (messageLog.Count > maxMessageCount)
        {
            messageLog.RemoveAt(0);
        }

        UpdateClickText();

        yield return new WaitForSeconds(3f);

        messageLog.Remove(message);

        UpdateClickText();
    }


    //-------------------------------------------------
    // イベント結果文章
    //-------------------------------------------------

    public void ShowEventMessage(string message, float duration = 5f)
    {
        if (eventMessageCoroutine != null)
        {
            StopCoroutine(eventMessageCoroutine);
        }

        eventMessageCoroutine =
            StartCoroutine(ShowEventMessageCoroutine(message, duration));
    }


    IEnumerator ShowEventMessageCoroutine(string message, float duration)
    {
        eventMessage = message;

        UpdateClickText();

        // Time.timeScale = 0でも消えるようにする
        yield return new WaitForSecondsRealtime(duration);

        eventMessage = "";

        UpdateClickText();

        eventMessageCoroutine = null;
    }


    //-------------------------------------------------
    // 下のログ欄更新
    //-------------------------------------------------

    void UpdateClickText()
    {
        if (clickText == null)
            return;

        clickText.text = "";


        // 結果文章を最優先
        if (!string.IsNullOrEmpty(eventMessage))
        {
            clickText.text = eventMessage;
            return;
        }


        // Hover説明
        if (!string.IsNullOrEmpty(hoverEventMessage))
        {
            clickText.text = hoverEventMessage;
            return;
        }


        // 通常のクリック履歴
        foreach (string message in messageLog)
        {
            clickText.text += message + "\n";
        }
    }


    //-------------------------------------------------
    // 一番前のオブジェクト取得
    //-------------------------------------------------

    ClickBoxObject GetTopObject(Vector3 target)
    {
        ClickBoxObject topObject = null;
        int highestOrder = int.MinValue;

        foreach (ClickBoxObject obj in clickBoxObjects)
        {
            if (obj == null)
                continue;

            // 非表示オブジェクトは無視
            if (!obj.gameObject.activeInHierarchy)
                continue;

            if (!obj.IsClickInside(target))
                continue;

            int order = obj.GetSortingOrder();

            if (order > highestOrder)
            {
                highestOrder = order;
                topObject = obj;
            }
        }

        return topObject;
    }
}