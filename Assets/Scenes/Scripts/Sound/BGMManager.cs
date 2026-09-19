using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("タイトルBGM")]
    public AudioClip titleBGM;

    private void Start()
    {
        PlayTitleBGM();
    }

    public void PlayTitleBGM()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSourceが設定されていません！");
            return;
        }

        if (titleBGM == null)
        {
            Debug.LogWarning("タイトルBGMが設定されていません！");
            return;
        }

        audioSource.clip = titleBGM;
        audioSource.loop = true;
        audioSource.Play();

        Debug.Log("タイトルBGM再生開始！");
    }

    public void StopBGM()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
    }
}