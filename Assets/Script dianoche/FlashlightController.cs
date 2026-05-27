using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject lightVisuals;

    [Header("Ajustes de Batería")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float drainRate = 5f;
    [SerializeField] private Image batteryBarImage;

    private float currentEnergy;
    public bool IsOn { get; private set; }

    // Establece el valor inicial de la energia de la linterna a su capacidad maxima y sincroniza la UI del HUD al iniciar el juego.
    void Start()
    {
        currentEnergy = maxEnergy;
        ActualizarUI();
    }

    // Escucha las pulsaciones de teclado del jugador y procesa el desgaste continuo de la bateria.
    // Se conecta con la variable global de estado del LevelManager para restringir su encendido.
    // Evita que el jugador pueda encender la linterna durante el dia, controla la activacion de las luces visuales y consume la energia frame a frame basandose en el tiempo transcurrido, forzando el apagado si la bateria llega a cero.
    void Update()
    {
        if (LevelManager.Instance.currentState == GameState.Night)
        {
            if ((Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Light")) && currentEnergy > 0)
            {
                IsOn = !IsOn;
                lightVisuals.SetActive(IsOn);
            }
        }
        else if (IsOn)
        {
            ApagarLinterna();
        }

        if (IsOn)
        {
            currentEnergy -= drainRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            ActualizarUI();

            if (currentEnergy <= 0f)
            {
                ApagarLinterna();
            }
        }
    }

    // Forzar el apagado logico y visual de la linterna en el juego.
    // Desactiva el booleano de control de estado y oculta el objeto del renderizador de luz adjunto.
    private void ApagarLinterna()
    {
        IsOn = false;
        lightVisuals.SetActive(false);
    }

    // Incrementa el nivel de la energia de la linterna segun un valor especifico recibido.
    // Pensado para conectarse externamente con el metodo de uso de objetos consumibles del inventario (como ItemBattery.cs al presionar la tecla Q).
    // Restringe el valor de la energia mediante un limite maximo para evitar desbordamientos de la variable y actualiza los graficos del HUD.
    public void RechargeBattery(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        ActualizarUI();
        Debug.Log($"Linterna recargada. Energía actual: {currentEnergy}");
    }

    // Modifica de manera directa la barra visual de la interfaz del jugador.
    // Se conecta con la propiedad fillAmount del componente de tipo Image de Unity UI.
    // Transforma el valor de la energia a un rango flotante normalizado entre 0.0 y 1.0 exigido por el componente grafico para representar de forma fiel el remanente de bateria en pantalla.
    private void ActualizarUI()
    {
        if (batteryBarImage != null)
        {
            batteryBarImage.fillAmount = currentEnergy / maxEnergy;
        }
    }
}