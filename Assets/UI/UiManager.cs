using UnityEngine;
using UnityEngine.UI; // Importante para usar el componente Image
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Altar UI")]
    [SerializeField] private Image altarBarFill; // Arrastra aquí la imagen "RellenoVerde"
    [SerializeField] private TextMeshProUGUI altarPercentText; // Texto opcional para el %

    [Header("Velas UI")]
    [SerializeField] private TextMeshProUGUI candlesCounterText; // El texto al lado del icono

    /*[Header("Batería de linterna")]
    [SerializeField] private Image batterylight;*/

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Actualiza la barra verde
    public void UpdateAltarHealth(float currentHealth, float maxHealth)
    {
        if (altarBarFill != null)
        {
            // Al ser floats, la división es directa y súper precisa
            float fillValue = currentHealth / maxHealth;
            altarBarFill.fillAmount = fillValue;

            // Cambiar color a rojo si queda poca vida (menor al 30%)
            altarBarFill.color = (fillValue < 0.3f) ? Color.red : Color.white;
        }

        if (altarPercentText != null)
        {
            // Muestra el porcentaje sin decimales molestos usando :0
            altarPercentText.text = $"{(currentHealth / maxHealth * 100f):0}%";
        }
    }

    // Actualiza el contador "Encendidas / Totales"
    public void UpdateCandleCount(int lit, int unlit)
    {
        if (candlesCounterText != null)
        {
            int total = lit + unlit;
            candlesCounterText.text = $"{lit} / {total}";

            // Si hay velas apagadas, ponemos el texto naranja como advertencia
            candlesCounterText.color = (unlit > 0) ? new Color(1f, 0.5f, 0f) : Color.white;
        }
    }

    [Header("Player UI")]
    [SerializeField] private Image playerHealthBarFill; // Arrastra aquí el "RellenoRojo"
    [SerializeField] private Image playerStaminaBarFill;

    // ... (dentro de la clase UIManager)

    public void UpdatePlayerHealth(int currentHealth, int maxHealth)
    {
        if (playerHealthBarFill != null)
        {
            float fillValue = (float)currentHealth / maxHealth;
            playerHealthBarFill.fillAmount = fillValue;

            // Si la vida es mayor al 34% (más de 1 corazón si tienes 3), vuelve a ser roja.
            // Si es 1 corazón o menos, se pone magenta (o el color de alerta que elijas)
            playerHealthBarFill.color = (fillValue <= 0.34f) ? Color.magenta : Color.red;
        }
    }

    public void UpdatePlayerStamina(float currentStamina, float maxStamina, bool isExhausted)
    {
        if (playerStaminaBarFill != null)
        {
            // Calculamos el porcentaje (0.0 a 1.0)
            float fillValue = currentStamina / maxStamina;
            playerStaminaBarFill.fillAmount = fillValue;

            // FEEDBACK VISUAL:
            // Si está agotado (isExhausted), la ponemos gris o roja para avisar que no puede correr.
            // Si no, color cian o verde.
            if (isExhausted)
            {
                playerStaminaBarFill.color = Color.gray; // O un rojo tenue
            }
            else
            {
                playerStaminaBarFill.color = Color.cyan;
            }
        }
    }
}