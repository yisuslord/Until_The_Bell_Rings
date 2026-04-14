
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour, IMenuPanel
{
    [Header("UI References")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Button closeButton;

    public bool IsVisible => contentParent.activeSelf;

    private void Awake()
    {
        Hide(); // Empezar oculto
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show() => contentParent.SetActive(true);
    public void Hide() => contentParent.SetActive(false);
}