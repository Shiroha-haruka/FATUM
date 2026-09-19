using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("タイトルへ戻る確認画面")]
    public GameObject pausePanel;

    private bool isPauseMenuOpen = false;


    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }


    private void Update()
    {
        // ESCキー
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseMenuOpen)
            {
                OpenPauseMenu();
            }
            else
            {
                ClosePauseMenu();
            }
        }
    }


    // ============================
    // ESC確認画面を開く
    // ============================

    public void OpenPauseMenu()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning(
                "PausePanelが設定されていません！"
            );

            return;
        }


        isPauseMenuOpen = true;

        pausePanel.SetActive(true);


        // ゲームを停止
        Time.timeScale = 0f;

        Debug.Log(
            "タイトルへ戻る確認画面を表示"
        );
    }


    // ============================
    // 「いいえ」
    // ============================

    public void SelectNo()
    {
        ClosePauseMenu();
    }


    private void ClosePauseMenu()
    {
        isPauseMenuOpen = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        // ゲーム再開
        Time.timeScale = 1f;

        Debug.Log(
            "ゲームを再開しました"
        );
    }


    // ============================
    // 「はい」
    // ============================

    public void SelectYes()
    {
        // 念のため時間停止解除
        Time.timeScale = 1f;


        // ゲーム進行を初期状態へ
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }


        // タイトルへ
        SceneManager.LoadScene(
            "TitleScene"
        );
    }


    private void OnDestroy()
    {
        // Scene切り替え時に
        // timeScale = 0 が残らないようにする
        Time.timeScale = 1f;
    }
}