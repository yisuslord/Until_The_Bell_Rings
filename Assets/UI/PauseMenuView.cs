// Guardar como: PauseMenuView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PauseMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject controlsPanel;

    public UnityEvent OnExitToMenu = new UnityEvent();

    private IMenuPanel _settingsLogic;
    private IMenuPanel _controlsLogic;

    private void Awake()
    {
        _controlsLogic = controlsPanel?.GetComponent<IMenuPanel>();
    }

    private void OnEnable()
    {
        // El botón Resume llama directamente al controlador (lo asignamos en el inspector)
        controlsButton.onClick.AddListener(() => _controlsLogic?.Show());
        exitButton.onClick.AddListener(() => OnExitToMenu?.Invoke());
    }

    private void OnDisable() => RemoveListeners();

    private void RemoveListeners()
    {
        resumeButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }
}