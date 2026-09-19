using UnityEngine;

public class MainScenarioStarter : MonoBehaviour
{
    [SerializeField] private GameObject mainScenario;
    private bool started;
    private ClickBoxObject clickBoxObject;

    private void Awake()
    {
        clickBoxObject = GetComponent<ClickBoxObject>();

        if (clickBoxObject != null)
        {
            clickBoxObject.onClick.AddListener(StartScenario);
        }
        else
        {
            Debug.LogWarning("同一个对象上没有 ClickBoxObject。", this);
        }
    }

    private void Start()
    {
        if (mainScenario != null)
        {
            mainScenario.SetActive(false);
        }
    }

    public void StartScenario()
    {
        if (started)
        {
            return;
        }

        started = true;

        if (mainScenario != null)
        {
            mainScenario.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MainScenario 没有设置。", this);
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (clickBoxObject != null)
        {
            clickBoxObject.onClick.RemoveListener(StartScenario);
        }
    }
}
