using System.Collections;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    public static CameraZoomController Instance { get; private set; }

    [Header("ズーム先")]
    public Transform yesZoomPoint;
    public Transform noZoomPoint;

    [Header("ズーム設定")]
    public float zoomSize = 2f;
    public float zoomDuration = 1f;

    private Camera mainCamera;

    private Vector3 defaultPosition;
    private float defaultSize;

    private Coroutine zoomCoroutine;


    private void Awake()
    {
        Instance = this;

        mainCamera = GetComponent<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError(
                "CameraZoomControllerはMain Cameraに付けてください！"
            );

            return;
        }

        defaultPosition = transform.position;
        defaultSize = mainCamera.orthographicSize;

        Debug.Log("CameraZoomController 起動");
    }


    public void ZoomToYesEvent()
    {
        Debug.Log("はい用カメラズーム開始");

        StartZoom(yesZoomPoint);
    }


    public void ZoomToNoEvent()
    {
        Debug.Log("いいえ用カメラズーム開始");

        StartZoom(noZoomPoint);
    }


    private void StartZoom(Transform target)
    {
        if (mainCamera == null)
        {
            Debug.LogError("カメラが取得できていません");
            return;
        }

        if (target == null)
        {
            Debug.LogError("ズーム先が設定されていません！");
            return;
        }

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }

        zoomCoroutine = StartCoroutine(
            ZoomRoutine(target)
        );
    }


    private IEnumerator ZoomRoutine(Transform target)
    {
        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            startPosition.z
        );

        float startSize =
            mainCamera.orthographicSize;

        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float t =
                elapsedTime / zoomDuration;

            t = Mathf.Clamp01(t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            mainCamera.orthographicSize =
                Mathf.Lerp(
                    startSize,
                    zoomSize,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;

        mainCamera.orthographicSize =
            zoomSize;

        Debug.Log(
            "カメラズーム完了：" +
            target.name
        );
    }


    public void ReturnToDefault()
    {
        if (mainCamera == null)
        {
            return;
        }

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }

        zoomCoroutine =
            StartCoroutine(ReturnRoutine());
    }


    private IEnumerator ReturnRoutine()
    {
        Vector3 startPosition =
            transform.position;

        float startSize =
            mainCamera.orthographicSize;

        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime +=
                Time.unscaledDeltaTime;

            float t =
                elapsedTime / zoomDuration;

            t = Mathf.Clamp01(t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    defaultPosition,
                    t
                );

            mainCamera.orthographicSize =
                Mathf.Lerp(
                    startSize,
                    defaultSize,
                    t
                );

            yield return null;
        }

        transform.position =
            defaultPosition;

        mainCamera.orthographicSize =
            defaultSize;
    }
}