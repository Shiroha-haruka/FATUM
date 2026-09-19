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

    private GameObject dialogueBox;
    private bool dialogueSuppressed;

    public int maxMessageCount = 3;

    private List<string> messageLog = new List<string>();

    private ClickBoxObject currentHoverObject;

    private Camera mainCamera;

    // DangerMarkなどのHover説明
    private string hoverEventMessage = "";

    // 結果などの固定メッセージ
    private string eventMessage = "";

    // ゲーム開始時の操作案内
    private string guideMessage = "";

    private Coroutine eventMessageCoroutine;
    private string[] pagedEventMessages;
    private int pagedEventMessageIndex;


    private void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        mainCamera = Camera.main;

        if (clickText != null && clickText.transform.parent != null)
        {
            dialogueBox = clickText.transform.parent.gameObject;
        }

        if (LocalizationManager.Instance != null)
        {
            guideMessage = LocalizationManager.Instance.Get("game.guide.click_danger");
        }

        // 非表示中(GameRootなど)も取得
        clickBoxObjects = FindObjectsByType<ClickBoxObject>(FindObjectsInactive.Include);

        UpdateClickText();
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
            if (pagedEventMessages != null)
            {
                ShowNextEventMessagePage();
                return;
            }

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
                hoverText.text = LocalizationManager.Instance.GetFormat(
                    "click.hover_format",
                    hoverObject.gameObject.name
                );
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
                hoverEventMessage = LocalizationManager.Instance.LocalizeSource(
                    hoverObject.hoverLogMessage
                );
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

        guideMessage = "";
        clickedObject.Click();

        if (clickText != null && clickedObject.showClickMessage)
        {
            string message = LocalizationManager.Instance.GetFormat(
                "click.clicked_format",
                clickedObject.gameObject.name
            );

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
        dialogueSuppressed = false;

        if (eventMessageCoroutine != null)
        {
            StopCoroutine(eventMessageCoroutine);
        }

        eventMessageCoroutine =
            StartCoroutine(ShowEventMessageCoroutine(message, duration));
    }

    public void HideDialogueBox()
    {
        dialogueSuppressed = true;
        UpdateClickText();
    }

    public void ShowPagedEventMessageKeys(params string[] messageKeys)
    {
        if (messageKeys == null || messageKeys.Length == 0 ||
            LocalizationManager.Instance == null)
        {
            return;
        }

        if (eventMessageCoroutine != null)
        {
            StopCoroutine(eventMessageCoroutine);
            eventMessageCoroutine = null;
        }

        pagedEventMessages = new string[messageKeys.Length];
        for (int i = 0; i < messageKeys.Length; i++)
        {
            pagedEventMessages[i] = LocalizationManager.Instance.Get(messageKeys[i]);
        }

        pagedEventMessageIndex = 0;
        dialogueSuppressed = false;
        eventMessage = pagedEventMessages[0];
        UpdateClickText();
    }

    private void ShowNextEventMessagePage()
    {
        pagedEventMessageIndex++;

        if (pagedEventMessageIndex >= pagedEventMessages.Length)
        {
            pagedEventMessages = null;
            eventMessage = "";
        }
        else
        {
            eventMessage = pagedEventMessages[pagedEventMessageIndex];
        }

        UpdateClickText();
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

        string displayText = "";

        if (!string.IsNullOrEmpty(eventMessage))
        {
            displayText = eventMessage;
        }
        else if (!string.IsNullOrEmpty(hoverEventMessage))
        {
            displayText = hoverEventMessage;
        }
        else if (messageLog.Count > 0)
        {
            displayText = string.Join("\n", messageLog);
        }
        else
        {
            displayText = guideMessage;
        }

        clickText.text = displayText;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(
                !dialogueSuppressed &&
                !string.IsNullOrWhiteSpace(displayText)
            );
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
