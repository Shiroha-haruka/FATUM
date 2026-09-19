using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("質問画面")]
    public GameObject questionPanel;

    [Header("チュートリアルのデンジャーマーク")]
    public GameObject dangerMark;

    [Header("カウントダウン表示")]
    public TextMeshProUGUI countdownText;

    [Header("選択できる時間")]
    public float choiceTime = 10f;

    [Header("カウントを表示し始める残り時間")]
    public float countdownShowTime = 5f;


    // =========================================
    // チュートリアル終了演出
    // =========================================

    [Header("結果終了後、カメラを戻すまでの待ち時間")]
    public float returnCameraDelay = 1f;

    [Header("カメラが戻ってからフェード開始までの待ち時間")]
    public float fadeStartDelay = 2f;

    [Header("フェードにかける時間")]
    public float fadeDuration = 1.5f;

    [Header("フェードアウトさせるSprite")]
    public SpriteRenderer[] fadeTargets;

    [Header("次のイベントのデンジャーマーク")]
    public GameObject nextDangerMark;


    private Coroutine countdownCoroutine;

    private bool choiceFinished = false;

    private bool human2Finished = false;
    private bool carFinished = false;
    private bool resultFinished = false;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // 次のDangerMarkは最初は隠す
        if (nextDangerMark != null)
        {
            nextDangerMark.SetActive(false);
        }

        Time.timeScale = 1f;
    }


    // =========================================
    // 質問表示
    // =========================================

    public void ShowQuestion()
    {
        if (questionPanel != null &&
            questionPanel.activeSelf)
        {
            return;
        }

        Debug.Log("チュートリアル質問を表示");

        choiceFinished = false;

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        Time.timeScale = 0f;

        countdownCoroutine =
            StartCoroutine(ChoiceCountdown());
    }


    // =========================================
    // カウントダウン
    // =========================================

    private IEnumerator ChoiceCountdown()
    {
        float remainingTime = choiceTime;

        while (remainingTime > 0f)
        {
            if (remainingTime <= countdownShowTime)
            {
                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);

                    countdownText.text =
                        Mathf.CeilToInt(
                            remainingTime
                        ).ToString();
                }
            }

            remainingTime -=
                Time.unscaledDeltaTime;

            yield return null;
        }


        if (choiceFinished)
        {
            yield break;
        }


        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "0";
        }


        yield return new WaitForSecondsRealtime(0.5f);


        TimeoutIgnore();
    }


    // =========================================
    // はい
    // =========================================

    public void SelectYes()
    {
        if (choiceFinished)
        {
            return;
        }

        choiceFinished = true;

        StopCountdown();

        Debug.Log("はい を選択しました");


        PrepareResult();

        HideDangerMark();


        Time.timeScale = 1f;


        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }


        if (CameraZoomController.Instance != null)
        {
            CameraZoomController.Instance
                .ZoomToYesEvent();
        }


        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .TutorialSelectYes();
        }
    }


    // =========================================
    // いいえ
    // =========================================

    public void SelectNo()
    {
        if (choiceFinished)
        {
            return;
        }

        choiceFinished = true;

        StopCountdown();

        Debug.Log("いいえ を選択しました");


        PrepareResult();

        HideDangerMark();


        Time.timeScale = 1f;


        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }


        if (CameraZoomController.Instance != null)
        {
            CameraZoomController.Instance
                .ZoomToNoEvent();
        }


        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .TutorialSelectNo();
        }
    }


    // =========================================
    // 時間切れ
    // =========================================

    private void TimeoutIgnore()
    {
        if (choiceFinished)
        {
            return;
        }

        choiceFinished = true;

        Debug.Log("時間切れ：無視");


        PrepareResult();

        HideDangerMark();


        Time.timeScale = 1f;


        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }


        if (CameraZoomController.Instance != null)
        {
            CameraZoomController.Instance
                .ZoomToNoEvent();
        }


        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .TutorialSelectNo();
        }
    }


    // =========================================
    // 結果準備
    // =========================================

    private void PrepareResult()
    {
        human2Finished = false;
        carFinished = false;
        resultFinished = false;
    }


    // =========================================
    // Human2終了通知
    // =========================================

    public void Human2RouteFinished()
    {
        human2Finished = true;

        Debug.Log(
            "Human2の結果ルート終了"
        );

        CheckResultFinished();
    }


    // =========================================
    // Car終了通知
    // =========================================

    public void CarRouteFinished()
    {
        carFinished = true;

        Debug.Log(
            "Carの結果ルート終了"
        );

        CheckResultFinished();
    }


    // =========================================
    // 両方終了したか確認
    // =========================================

    private void CheckResultFinished()
    {
        if (resultFinished)
        {
            return;
        }


        if (human2Finished &&
            carFinished)
        {
            resultFinished = true;

            Debug.Log(
                "Human2とCarの両方が終了！"
            );

            StartCoroutine(
                FinishResultSequence()
            );
        }
    }


    // =========================================
    // チュートリアル終了演出
    // =========================================

    private IEnumerator FinishResultSequence()
    {
        // =====================================
        // 結果メッセージを下のログ欄に表示
        // =====================================

        if (ClickManager.Instance != null &&
            GameManager.Instance != null)
        {
            if (GameManager.Instance.tutorialHelped)
            {
                ClickManager.Instance.ShowEventMessage(
                    LocalizationManager.Instance.Get("tutorial.result.helped"),
                    5f
                );
            }
            else
            {
                ClickManager.Instance.ShowEventMessage(
                    LocalizationManager.Instance.Get("tutorial.result.not_helped"),
                    5f
                );
            }
        }


        // 少し結果を見せる
        if (returnCameraDelay > 0f)
        {
            yield return new WaitForSeconds(
                returnCameraDelay
            );
        }


        // カメラを元に戻す
        Debug.Log(
            "カメラを元に戻します"
        );


        if (CameraZoomController.Instance != null)
        {
            CameraZoomController.Instance
                .ReturnToDefault();


            // カメラが戻り終わるまで待つ
            yield return new WaitForSeconds(
                CameraZoomController.Instance
                    .zoomDuration
            );
        }


        // =====================================
        // カメラが戻ってから数秒待つ
        // =====================================

        if (fadeStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                fadeStartDelay
            );
        }


        // =====================================
        // Human1 / Human2 / Carをフェードアウト
        // =====================================

        Debug.Log(
            "チュートリアルキャラをフェードアウト"
        );


        yield return StartCoroutine(
            FadeOutTutorialObjects()
        );


        // =====================================
        // チュートリアル完全終了
        // =====================================

        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .FinishTutorial();
        }


        // =====================================
        // 次のDangerMarkを表示
        // =====================================

        if (nextDangerMark != null)
        {
            nextDangerMark.SetActive(true);

            Debug.Log(
                "次のDangerMarkを表示！"
            );
        }


    }

    public void BeginSecondTutorial()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.secondTutorialStarted)
        {
            return;
        }

        if (nextDangerMark != null)
        {
            nextDangerMark.SetActive(false);
        }

        GameManager.Instance.StartSecondTutorial();
    }


    // =========================================
    // フェードアウト
    // =========================================

    private IEnumerator FadeOutTutorialObjects()
    {
        float elapsedTime = 0f;


        // 元の色を保存
        Color[] startColors =
            new Color[fadeTargets.Length];


        for (int i = 0;
             i < fadeTargets.Length;
             i++)
        {
            if (fadeTargets[i] != null)
            {
                startColors[i] =
                    fadeTargets[i].color;
            }
        }


        // 徐々に透明にする
        while (elapsedTime < fadeDuration)
        {
            elapsedTime +=
                Time.deltaTime;


            float t =
                elapsedTime /
                fadeDuration;


            t = Mathf.Clamp01(t);


            for (int i = 0;
                 i < fadeTargets.Length;
                 i++)
            {
                if (fadeTargets[i] == null)
                {
                    continue;
                }


                Color color =
                    startColors[i];


                color.a =
                    Mathf.Lerp(
                        startColors[i].a,
                        0f,
                        t
                    );


                fadeTargets[i].color =
                    color;
            }


            yield return null;
        }


        // 完全に透明にする
        for (int i = 0;
             i < fadeTargets.Length;
             i++)
        {
            if (fadeTargets[i] == null)
            {
                continue;
            }


            Color color =
                fadeTargets[i].color;

            color.a = 0f;

            fadeTargets[i].color =
                color;


            // 完全に消した後は無効化
            fadeTargets[i]
                .gameObject
                .SetActive(false);
        }
    }


    // =========================================
    // DangerMarkを隠す
    // =========================================

    private void HideDangerMark()
    {
        if (dangerMark != null)
        {
            dangerMark.SetActive(false);
        }
    }


    // =========================================
    // カウント停止
    // =========================================

    private void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(
                countdownCoroutine
            );

            countdownCoroutine = null;
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1f;
    }
}
