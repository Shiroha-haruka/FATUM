using System.Collections;
using System;
using UnityEngine;

[System.Serializable]
public class SimpleRoutePoint
{
    public Transform point;

    [Tooltip("到达这个点时切换的图片；留空则不切换")]
    public Sprite spriteOnArrival;

    [Min(0f)]
    public float waitAfterArrival;
}

public class SimpleRouteMover : MonoBehaviour
{
    public event Action<SimpleRouteMover> RouteCompleted;
    public bool IsRouteCompleted { get; private set; }

    [Header("移动路线（第一个点是出生位置）")]
    [SerializeField] private SimpleRoutePoint[] routePoints;

    [Header("移动速度")]
    [Min(0.01f)]
    [SerializeField] private float movementSpeed = 1f;

    [Header("路线结束")]
    [SerializeField] private bool hideAtEnd;

    [Tooltip("到达最后一个点后开启的对象，例如其他人物的父对象")]
    [SerializeField] private GameObject[] activateAtEnd;

    [Header("到达后的会话文本")]
    [Tooltip("Localization CSV 的文本 Key；留空则不显示")]
    [SerializeField] private string messageKeyAtEnd;

    [Min(0f)]
    [SerializeField] private float messageDuration = 6f;

    private SpriteRenderer spriteRenderer;
    private Coroutine moveCoroutine;
    private Vector3[] routePositions;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        IsRouteCompleted = false;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        if (!TryPlaceAtSpawnPoint())
        {
            return;
        }

        moveCoroutine = StartCoroutine(MoveRoute());
    }

    private bool TryPlaceAtSpawnPoint()
    {
        if (routePoints == null || routePoints.Length == 0 || routePoints[0].point == null)
        {
            Debug.LogWarning($"{name}: 没有设置移动点1。", this);
            return false;
        }

        routePositions = new Vector3[routePoints.Length];
        for (int i = 0; i < routePoints.Length; i++)
        {
            routePositions[i] = routePoints[i].point != null
                ? routePoints[i].point.position
                : transform.position;
        }

        ApplyPointImmediately(routePoints[0], routePositions[0]);
        return true;
    }

    private IEnumerator MoveRoute()
    {

        if (routePoints[0].waitAfterArrival > 0f)
        {
            yield return new WaitForSeconds(routePoints[0].waitAfterArrival);
        }

        for (int i = 1; i < routePoints.Length; i++)
        {
            SimpleRoutePoint routePoint = routePoints[i];
            if (routePoint.point == null)
            {
                continue;
            }

            yield return MoveTo(routePoint, routePositions[i]);

            if (routePoint.waitAfterArrival > 0f)
            {
                yield return new WaitForSeconds(routePoint.waitAfterArrival);
            }
        }

        moveCoroutine = null;
        IsRouteCompleted = true;
        ShowCompletionMessage();
        RouteCompleted?.Invoke(this);

        if (activateAtEnd != null)
        {
            foreach (GameObject target in activateAtEnd)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }

        if (hideAtEnd)
        {
            gameObject.SetActive(false);
        }
    }

    private void ShowCompletionMessage()
    {
        if (string.IsNullOrWhiteSpace(messageKeyAtEnd) ||
            ClickManager.Instance == null ||
            LocalizationManager.Instance == null)
        {
            return;
        }

        string message = LocalizationManager.Instance.Get(messageKeyAtEnd);
        ClickManager.Instance.ShowEventMessage(message, messageDuration);
    }

    private void ApplyPointImmediately(SimpleRoutePoint routePoint, Vector3 position)
    {
        transform.position = position;
        ChangeSprite(routePoint.spriteOnArrival);
    }

    private IEnumerator MoveTo(SimpleRoutePoint routePoint, Vector3 targetPosition)
    {
        while ((transform.position - targetPosition).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        ChangeSprite(routePoint.spriteOnArrival);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        if (newSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}
