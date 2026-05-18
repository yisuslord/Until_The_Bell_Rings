using UnityEngine;

public enum InputType { KeyboardMouse, Gamepad }

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }
    public InputType CurrentInput { get; private set; } = InputType.KeyboardMouse;

    // Evento opcional por si quieres que otros scripts se enteren del cambio al instante
    public System.Action<InputType> OnInputDeviceChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Detectar si se toca el Teclado o el Ratón
        if (Input.anyKeyDown || Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
        {
            CambiarDispositivo(InputType.KeyboardMouse);
        }

        // 2. Detectar si se mueven los Joysticks o botones del Mando de Xbox
        if (MandoEnUso())
        {
            CambiarDispositivo(InputType.Gamepad);
        }
    }

    private void CambiarDispositivo(InputType nuevoTipo)
    {
        if (CurrentInput != nuevoTipo)
        {
            CurrentInput = nuevoTipo;
            OnInputDeviceChanged?.Invoke(CurrentInput);
            Debug.Log($"<color=yellow>Mando cambiado a: {nuevoTipo}</color>");
        }
    }

    private bool MandoEnUso()
    {
        // Monitorea botones típicos de Xbox (A, B, X, Y, Start, etc.) del joystick 1 al 10 de Unity
        for (int i = (int)KeyCode.JoystickButton0; i <= (int)KeyCode.JoystickButton19; i++)
        {
            if (Input.GetKey((KeyCode)i)) return true;
        }

        // Monitorea los ejes de los sticks del control
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.2f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.2f)
        {
            return true;
        }

        return false;
    }
}
