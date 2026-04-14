// Guardar como: PauseMenuView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PauseMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    public UnityEvent OnExitToMenu = new UnityEvent();

    private IMenuPanel _settingsLogic;
    private IMenuPanel _controlsLogic;

    private void Awake()
    {
        _settingsLogic = settingsPanel?.GetComponent<IMenuPanel>();
        _controlsLogic = controlsPanel?.GetComponent<IMenuPanel>();
    }

    private void OnEnable()
    {
        // El botón Resume llama directamente al controlador (lo asignamos en el inspector)
        controlsButton.onClick.AddListener(() => _controlsLogic?.Show());
        settingsButton.onClick.AddListener(() => _settingsLogic?.Show());
        exitButton.onClick.AddListener(() => OnExitToMenu?.Invoke());
    }

    private void OnDisable() => RemoveListeners();

    private void RemoveListeners()
    {
        resumeButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }
}