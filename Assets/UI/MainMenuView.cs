using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button creditsButton; // 🔥 Ya lo tienes aquí declarado

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanelObject;

    // Eventos para que el controlador los escuche
    public UnityEvent OnStartPressed = new UnityEvent();
    public UnityEvent OnQuitPressed = new UnityEvent();
    public UnityEvent OnCreditsPressed = new UnityEvent(); // 🔥 NUEVO: Evento de aviso

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

        // 🔥 NUEVO: Le ordenamos al botón de créditos que dispare el aviso al controlador
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(() => OnCreditsPressed?.Invoke());
        }
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

        // 🔥 NUEVO: Limpieza preventiva de listeners
        if (creditsButton != null) creditsButton.onClick.RemoveAllListeners();
    }
}