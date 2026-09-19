using System.Collections;
using UnityEngine;

[System.Serializable]
public class RoutePoint
{
    [Header("移動先")]
    public Transform point;

    [Header("この地点までの移動時間")]
    public float moveDuration = 1f;

    [Header("この地点に着いた時のZ回転角度")]
    public float targetRotationZ = 0f;


    [Header("この地点へ向かう時の見た目")]
    [Tooltip("設定した場合、この地点へ向かい始めた瞬間にSpriteを変更します")]
    public Sprite changeSprite;


    [Header("この地点へ向かい始めた瞬間の効果音")]
    public AudioClip moveStartSound;

    [Header("この地点に着く前に鳴らす効果音")]
    public AudioClip arrivalSound;

    [Header("到着の何秒前に鳴らすか")]
    public float soundLeadTime = 0f;
}


public class PictogramMover : MonoBehaviour
{
    public enum TutorialResultObject
    {
        None,
        Human2,
        Car
    }


    [Header("ゲーム開始後の通常ルート")]
    public RoutePoint[] firstRoute;

    [Header("通常ルート開始までの待ち時間")]
    public float firstRouteDelay = 0f;

    [Header("通常ルート開始時の効果音")]
    public AudioClip firstRouteStartSound;


    [Header("「はい」を選んだ時の安全ルート")]
    public RoutePoint[] safeRoute;

    [Header("安全ルート開始までの待ち時間")]
    public float safeRouteDelay = 0f;

    [Header("安全ルート開始時の効果音")]
    public AudioClip safeRouteStartSound;


    [Header("「いいえ」を選んだ時の危険ルート")]
    public RoutePoint[] dangerRoute;

    [Header("危険ルート開始までの待ち時間")]
    public float dangerRouteDelay = 0f;

    [Header("危険ルート開始時の効果音")]
    public AudioClip dangerRouteStartSound;


    [Header("見た目")]
    public SpriteRenderer spriteRenderer;


    [Header("効果音")]
    public AudioSource audioSource;


    [Header("チュートリアル結果終了通知")]
    public TutorialResultObject tutorialResultObject =
        TutorialResultObject.None;


    private bool firstRouteStarted = false;
    private bool branchRouteStarted = false;
    private bool isMoving = false;


    private void Awake()
    {
        // Inspectorで設定していなくても
        // 同じGameObjectから自動取得する
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }
    }


    void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }


        // ==========================
        // ゲーム開始後の通常ルート
        // ==========================

        if (GameManager.Instance.isGameStarted &&
            !firstRouteStarted)
        {
            firstRouteStarted = true;

            StartCoroutine(
                StartRouteWithDelay(
                    firstRoute,
                    firstRouteDelay,
                    firstRouteStartSound,
                    false
                )
            );
        }


        // ==========================
        // チュートリアル分岐後
        // ==========================

        if (GameManager.Instance.tutorialCompleted &&
            !branchRouteStarted &&
            !isMoving)
        {
            branchRouteStarted = true;


            // 「はい」
            if (GameManager.Instance.tutorialHelped)
            {
                Debug.Log(
                    gameObject.name +
                    "：安全ルートへ移動"
                );

                StartCoroutine(
                    StartRouteWithDelay(
                        safeRoute,
                        safeRouteDelay,
                        safeRouteStartSound,
                        true
                    )
                );
            }

            // 「いいえ」または時間切れ
            else
            {
                Debug.Log(
                    gameObject.name +
                    "：危険ルートへ移動"
                );

                StartCoroutine(
                    StartRouteWithDelay(
                        dangerRoute,
                        dangerRouteDelay,
                        dangerRouteStartSound,
                        true
                    )
                );
            }
        }
    }


    IEnumerator StartRouteWithDelay(
        RoutePoint[] route,
        float delay,
        AudioClip startSound,
        bool isResultRoute
    )
    {
        isMoving = true;


        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }


        // ルート開始SE
        PlaySound(startSound);


        yield return StartCoroutine(
            MoveRoute(route)
        );


        isMoving = false;


        if (isResultRoute)
        {
            NotifyTutorialResultFinished();
        }
    }


    IEnumerator MoveRoute(RoutePoint[] route)
    {
        if (route == null ||
            route.Length == 0)
        {
            yield break;
        }


        for (int i = 0; i < route.Length; i++)
        {
            if (route[i].point == null)
            {
                continue;
            }


            // ==============================
            // このPointへ向かうSpriteに変更
            // ==============================

            if (route[i].changeSprite != null)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite =
                        route[i].changeSprite;
                }
                else
                {
                    Debug.LogWarning(
                        gameObject.name +
                        " にSpriteRendererがありません！"
                    );
                }
            }


            // ==============================
            // 移動開始SE
            // ==============================

            PlaySound(
                route[i].moveStartSound
            );


            Vector3 startPosition =
                transform.position;

            Vector3 endPosition =
                route[i].point.position;

            float moveDuration =
                route[i].moveDuration;


            Quaternion startRotation =
                transform.rotation;

            Quaternion endRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    route[i].targetRotationZ
                );


            bool arrivalSoundPlayed = false;


            // ==============================
            // 移動時間が0の場合
            // ==============================

            if (moveDuration <= 0f)
            {
                transform.position =
                    endPosition;

                transform.rotation =
                    endRotation;


                PlaySound(
                    route[i].arrivalSound
                );

                continue;
            }


            float elapsedTime = 0f;


            // ==============================
            // 移動
            // ==============================

            while (elapsedTime < moveDuration)
            {
                elapsedTime +=
                    Time.deltaTime;


                float progress =
                    elapsedTime /
                    moveDuration;


                progress =
                    Mathf.Clamp01(progress);


                transform.position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        progress
                    );


                transform.rotation =
                    Quaternion.Lerp(
                        startRotation,
                        endRotation,
                        progress
                    );


                // 到着までの残り時間
                float remainingTime =
                    moveDuration -
                    elapsedTime;


                // 到着より少し前にSE
                if (!arrivalSoundPlayed &&
                    route[i].arrivalSound != null &&
                    remainingTime <=
                    route[i].soundLeadTime)
                {
                    PlaySound(
                        route[i].arrivalSound
                    );

                    arrivalSoundPlayed = true;
                }


                yield return null;
            }


            // ==============================
            // 最終位置
            // ==============================

            transform.position =
                endPosition;

            transform.rotation =
                endRotation;


            // まだ到着SEが鳴っていなければ鳴らす
            if (!arrivalSoundPlayed &&
                route[i].arrivalSound != null)
            {
                PlaySound(
                    route[i].arrivalSound
                );
            }
        }


        Debug.Log(
            gameObject.name +
            " のルート移動完了！"
        );
    }


    private void PlaySound(
        AudioClip clip
    )
    {
        if (clip == null)
        {
            return;
        }


        if (audioSource == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " にAudioSourceが設定されていません"
            );

            return;
        }


        audioSource.PlayOneShot(
            clip
        );
    }


    private void NotifyTutorialResultFinished()
    {
        if (TutorialManager.Instance == null)
        {
            return;
        }


        switch (tutorialResultObject)
        {
            case TutorialResultObject.Human2:

                TutorialManager.Instance
                    .Human2RouteFinished();

                break;


            case TutorialResultObject.Car:

                TutorialManager.Instance
                    .CarRouteFinished();

                break;
        }
    }
}