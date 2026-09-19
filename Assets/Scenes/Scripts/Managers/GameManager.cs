using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム全体")]
    public bool isGameStarted = false;

    [Header("チュートリアル")]
    public bool tutorialChoiceMade = false;
    public bool tutorialHelped = false;

    // 分岐ルートを動かすために使う
    public bool tutorialCompleted = false;

    // 結果アニメーションまで全部終了したか
    public bool tutorialFinished = false;

    [Header("メインシナリオ")]
    public bool mainScenarioStarted = false;


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


    public void StartGame()
    {
        isGameStarted = true;

        Debug.Log("ゲーム開始！");
    }


    public void TutorialSelectYes()
    {
        tutorialChoiceMade = true;
        tutorialHelped = true;
        tutorialCompleted = true;

        Debug.Log("チュートリアル：助けるを選択");
    }


    public void TutorialSelectNo()
    {
        tutorialChoiceMade = true;
        tutorialHelped = false;
        tutorialCompleted = true;

        Debug.Log("チュートリアル：助けないを選択");
    }


    public void FinishTutorial()
    {
        tutorialFinished = true;

        Debug.Log("チュートリアル完全終了！");
    }


    public void StartMainScenario()
    {
        if (mainScenarioStarted)
        {
            return;
        }

        mainScenarioStarted = true;

        Debug.Log("メインシナリオ開始！");
    }


    // =========================================
    // ゲームを最初の状態に戻す
    // =========================================
    public void ResetGame()
    {
        isGameStarted = false;

        tutorialChoiceMade = false;
        tutorialHelped = false;
        tutorialCompleted = false;
        tutorialFinished = false;

        mainScenarioStarted = false;

        // 念のため時間停止も解除
        Time.timeScale = 1f;

        Debug.Log("ゲーム進行をリセットしました！");
    }
}