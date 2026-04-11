using UnityEngine;
using TMPro; // Asegúrate de usar TextMeshPro para que se vea bien

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI altarHealthText;
    [SerializeField] private TextMeshProUGUI candlesText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAltarHealth(int currentHealth, int maxHealth)
    {
        if (altarHealthText != null)
        {
            altarHealthText.text = $"Vida del Altar: {currentHealth} / {maxHealth}";
        }
    }

    public void UpdateCandleCount(int lit, int unlit)
    {
        if (candlesText != null)
        {
            candlesText.text = $"Velas Prendidas: {lit}\nVelas Apagadas: {unlit}";

            if (unlit > 0)
            {
                candlesText.color = Color.red; // Se pone rojo si hay apagadas
            }
            else
            {
                candlesText.color = Color.white;
            }
        }
    }
}