using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Altar : MonoBehaviour, IDamageable
{
    [Header("Altar Stats")]
    // 🔥 CAMBIO: Ahora son float para permitir un drenaje totalmente fluido y suave
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;

    [Header("Continuous Drain Settings")]
    [Tooltip("Cuánto daño por segundo hace CADA vela apagada/corrupta")]
    [SerializeField] private float baseDamagePerSecond = 0.5f;

    private List<Candle> allCandlesInScene = new List<Candle>();
    private bool isGameOver = false;

    private void Awake()
    {
        Candle[] foundCandles = Object.FindObjectsByType<Candle>(FindObjectsSortMode.None);
        allCandlesInScene.AddRange(foundCandles);
        currentHealth = maxHealth;
    }

    private void Start()
    {
        UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);
        UpdateCandlesUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        // 1. Contar velas apagadas o corruptas en tiempo real
        int unlitCount = 0;
        foreach (Candle candle in allCandlesInScene)
        {
            if (candle != null && (!candle.IsLit || candle.IsCorrupted))
                unlitCount++;
        }

        // 2. Si hay velas apagadas, aplicar daño continuo por segundo
        if (unlitCount > 0)
        {
            // La velocidad de bajada escala dinámicamente con la cantidad de velas
            float totalDamageThisFrame = unlitCount * baseDamagePerSecond * Time.deltaTime;

            // En lugar de llamar a TakeDamage (que es para golpes secos), reducimos directo de forma suave
            currentHealth -= totalDamageThisFrame;

            if (currentHealth < 0) currentHealth = 0;

            // Actualizar la interfaz constantemente
            UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                TriggerGameOver();
            }
        }
    }

    // El Altar avisa a la UI cuando una vela cambia de estado
    public void NotifyCandleChanged()
    {
        UpdateCandlesUI();
    }

    private void UpdateCandlesUI()
    {
        int lit = 0;
        int unlit = 0;
        foreach (Candle c in allCandlesInScene)
        {
            if (c == null) continue;
            if (c.IsLit && !c.IsCorrupted) lit++;
            else unlit++;
        }
        UIManager.Instance.UpdateCandleCount(lit, unlit);
    }

    // 🔥 Mantenemos este método intacto para los golpes secos e instantáneos del Acechador
    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("<color=red>EL ALTAR SE HA EXTINGUIDO. LA OSCURIDAD REINA.</color>");
        Time.timeScale = 0f;
        SceneManager.LoadScene(2);
    }
}