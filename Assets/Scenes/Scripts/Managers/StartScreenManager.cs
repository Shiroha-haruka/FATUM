using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField]
    private LanguageSelectionController languageSelectionController;

    public void StartGame()
    {
        
        //ゲーム開始時に言語選択画面を表示する
        if (languageSelectionController == null)
        {
            languageSelectionController = FindAnyObjectByType<LanguageSelectionController>();
        }

        if (languageSelectionController == null)
        {
            return;
        }

        languageSelectionController.ShowLanguageSelection();
    }
}
