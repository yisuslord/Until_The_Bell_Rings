using UnityEngine;
using UnityEngine.UI; // Obligatorio para detectar el componente Image

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject lightVisuals;

    [Header("Ajustes de Batería")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float drainRate = 5f; // Cuánta energía gasta por segundo
    [SerializeField] private Image batteryBarImage; // 🔥 Cambiado de Slider a Image

    private float currentEnergy;
    public bool IsOn { get; private set; }

    void Start()
    {
        currentEnergy = maxEnergy;
        ActualizarUI();
    }

    void Update()
    {
        // Control de encendido/apagado por el jugador
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

        // Gasto de batería mientras está encendida
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

    private void ApagarLinterna()
    {
        IsOn = false;
        lightVisuals.SetActive(false);
    }

    // Método público que llamará la batería (ItemBattery.cs) al usarse con Q
    public void RechargeBattery(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        ActualizarUI();
        Debug.Log($"Linterna recargada. Energía actual: {currentEnergy}");
    }

    private void ActualizarUI()
    {
        if (batteryBarImage != null)
        {
            // 🔥 Modificamos el fillAmount (acepta valores de 0.0f a 1.0f)
            batteryBarImage.fillAmount = currentEnergy / maxEnergy;
        }
    }
}