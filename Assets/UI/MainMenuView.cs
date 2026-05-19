// Guardar como: MainMenuView.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public class MainMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanelObject;

    // Eventos para que el controlador los escuche
    public UnityEvent OnStartPressed = new UnityEvent();
    public UnityEvent OnQuitPressed = new UnityEvent();

    private IMenuPanel _settingsPanel;

    private void Awake()
    {
        _settingsPanel = settingsPanelObject.GetComponent<IMenuPanel>();
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(() => OnStartPressed?.Invoke());
        settingsButton.onClick.AddListener(() => {
            _settingsPanel?.Show();
        });
        quitButton.onClick.AddListener(() => OnQuitPressed?.Invoke());
        backButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(settingsButton.gameObject));
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }

}