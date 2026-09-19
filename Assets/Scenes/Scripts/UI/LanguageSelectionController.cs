using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class LanguageSelectionController : MonoBehaviour
{
    [Header("Language Selection")]
    [SerializeField] private GameObject languageSelection;
    [SerializeField] private CanvasGroup selectionGroup;

    [Header("Japanese Button")]
    [SerializeField] private Button japaneseButton;
    [SerializeField] private TMP_Text japaneseText;

    [Header("English Button")]
    [SerializeField] private Button englishButton;
    [SerializeField] private TMP_Text englishText;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.55f;

    private bool isTransitioning;

    private void Start()
    {
        if (languageSelection == null)
        {
            Debug.LogError("LanguageSelectionが設定されていません。");
            return;
        }

        if (selectionGroup == null)
        {
            selectionGroup = languageSelection.GetComponent<CanvasGroup>();
        }

        if (selectionGroup == null)
        {
            Debug.LogError("LanguageSelectionにCanvasGroupがありません。");
            return;
        }

        selectionGroup.alpha = 0f;
        selectionGroup.interactable = false;
        selectionGroup.blocksRaycasts = false;

        languageSelection.SetActive(false);

       
        if (japaneseButton != null && japaneseText != null)
        {
            japaneseButton.onClick.AddListener(SelectJapanese);
            SetupHover(japaneseButton, japaneseText);
        }

        if (englishButton != null && englishText != null)
        {
            englishButton.onClick.AddListener(SelectEnglish);
            SetupHover(englishButton, englishText);
        }
    }

    public void ShowLanguageSelection()
    {
        Debug.Log("ShowLanguageSelection called.");

        if (languageSelection == null || selectionGroup == null)
        {
            Debug.LogError("言語選択UIの参照が不足しています。");
            return;
        }

        if (isTransitioning)
            return;

        Transform parent = languageSelection.transform.parent;

        if (parent != null && !parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
        }

        languageSelection.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        isTransitioning = true;

        selectionGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            selectionGroup.alpha =
                Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            yield return null;
        }

        selectionGroup.alpha = 1f;
        selectionGroup.interactable = true;
        selectionGroup.blocksRaycasts = true;

        isTransitioning = false;
    }

    
    public void SelectJapanese()
    {
        SelectLanguage(GameLanguage.Japanese);
    }

    public void SelectEnglish()
    {
        SelectLanguage(GameLanguage.English);
    }

    private void SelectLanguage(GameLanguage language)
    {
        if (isTransitioning)
            return;

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(language);
        }

        Debug.Log($"Language selected: {language}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        SceneManager.LoadScene("GameScene");
    }

    
    private void SetupHover(Button button, TMP_Text text)
    {
        
        button.image.color = Color.black;
        text.color = Color.white;

        EventTrigger trigger =
            button.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger =
                button.gameObject.AddComponent<EventTrigger>();
        }

        
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;

        enter.callback.AddListener((data) =>
        {
            button.image.color = Color.white;
            text.color = Color.black;
        });

        trigger.triggers.Add(enter);

        
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;

        exit.callback.AddListener((data) =>
        {
            button.image.color = Color.black;
            text.color = Color.white;
        });

        trigger.triggers.Add(exit);
    }
}
