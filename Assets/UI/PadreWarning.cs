using UnityEngine;

public class PadreWarning : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject indicatorPrefab; 
    [SerializeField] private float margin = 50f; 

    private GameObject indicatorInstance;
    private RectTransform indicatorRect;
    private NightNPC padre;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;

    private bool initialized = false;

    private void Start()
    {
        padre = GetComponent<NightNPC>();
        mainCamera = Camera.main;

        GameObject canvas = GameObject.Find("Canvas"); 
        indicatorInstance = Instantiate(indicatorPrefab, canvas.transform);
        indicatorRect = indicatorInstance.GetComponent<RectTransform>();
        canvasGroup = indicatorInstance.GetComponent<CanvasGroup>();

        indicatorInstance.SetActive(false);
    }

    private void Update()
    {
        if (!initialized)
        {
            initialized = true;
            return;
        }

        if (!padre.canInteract)
        {
            if (indicatorInstance.activeSelf)
            {
                indicatorInstance.SetActive(false);
            }

            return;
        }

        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                           screenPos.y <= 0 || screenPos.y >= Screen.height;

        if (isOffScreen)
        {
            indicatorInstance.SetActive(true);


            float alpha = Mathf.PingPong(Time.time * 5f, 2f);
            canvasGroup.alpha = alpha;

            float edgeX = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
            float edgeY = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

            indicatorRect.position = new Vector2(edgeX, edgeY);
        }
        else
        {
            indicatorInstance.SetActive(false);

        }
    }

    private void OnDestroy()
    {
        if (indicatorInstance != null) Destroy(indicatorInstance);
    }
}