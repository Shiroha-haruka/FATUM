using System.Collections;
using UnityEngine;

public class SimpleRouteCompletionGroup : MonoBehaviour
{
    [Tooltip("组内所有路线完成后开启的对象，例如三台车的父对象")]
    [SerializeField] private GameObject[] activateWhenAllCompleted;

    [Header("全部到达后的会话文本")]
    [SerializeField] private string messageKey = "story.gang_cars_appear";

    [Min(0f)]
    [SerializeField] private float messageDuration = 7f;

    private SimpleRouteMover[] movers;
    private bool groupCompleted;
    private Coroutine waitCoroutine;

    private void OnEnable()
    {
        groupCompleted = false;
        movers = GetComponentsInChildren<SimpleRouteMover>(true);

        if (movers.Length == 0)
        {
            Debug.LogWarning($"{name}: 路人组中没有 SimpleRouteMover。", this);
            return;
        }

        waitCoroutine = StartCoroutine(WaitForAllRoutes());
    }

    private void OnDisable()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
    }

    private IEnumerator WaitForAllRoutes()
    {
        yield return null;

        while (!AreAllRoutesCompleted())
        {
            yield return null;
        }

        CompleteGroup();
        waitCoroutine = null;
    }

    private bool AreAllRoutesCompleted()
    {
        foreach (SimpleRouteMover mover in movers)
        {
            if (mover != null && mover.gameObject.activeSelf && !mover.IsRouteCompleted)
            {
                return false;
            }
        }

        return true;
    }

    private void CompleteGroup()
    {
        if (groupCompleted)
        {
            return;
        }

        groupCompleted = true;

        if (!string.IsNullOrWhiteSpace(messageKey) &&
            ClickManager.Instance != null &&
            LocalizationManager.Instance != null)
        {
            string message = LocalizationManager.Instance.Get(messageKey);
            ClickManager.Instance.ShowEventMessage(message, messageDuration);
            StartCoroutine(ShowFollowupMessage());
        }

        foreach (GameObject target in activateWhenAllCompleted)
        {
            if (target != null)
            {
                target.SetActive(true);
            }
        }
    }

    private IEnumerator ShowFollowupMessage()
    {
        yield return new WaitForSecondsRealtime(messageDuration);

        if (ClickManager.Instance == null)
        {
            yield break;
        }

        ClickManager.Instance.ShowPagedEventMessageKeys(
            "story.final_choice_1",
            "story.final_choice_2",
            "story.final_choice_3"
        );
    }
}
