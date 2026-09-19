using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public void StartGame()
    {
        // ゲーム開始フラグをON
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        // GameSceneへ移動
        SceneManager.LoadScene("GameScene");
    }
}