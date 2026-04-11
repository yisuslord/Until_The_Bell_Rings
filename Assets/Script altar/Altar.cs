using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Altar : MonoBehaviour, IDamageable
{
    [Header("Altar Stats")]
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    [Header("Attack Settings")]
    [SerializeField] private float drainInterval = 5f; // Cada cuántos segundos pierde vida
    [SerializeField] private int damagePerUnlitCandle = 2; // Cuánto daño hace cada vela apagada

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
        StartCoroutine(DrainHealthRoutine());
        UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);
        UpdateCandlesUI();
    }

    private IEnumerator DrainHealthRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(drainInterval);

            int unlitCount = 0;
            foreach (Candle candle in allCandlesInScene)
            {
                if (!candle.IsLit || candle.IsCorrupted) unlitCount++;
            }

            if (unlitCount > 0)
            {
                int totalDamage = unlitCount * damagePerUnlitCandle;
                TakeDamage(totalDamage);
                Debug.Log($"<color=orange>El altar pierde {totalDamage} de vida por la oscuridad.</color>");
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
            if (c.IsLit && !c.IsCorrupted) lit++;
            else unlit++;
        }
        UIManager.Instance.UpdateCandleCount(lit, unlit);
    }

    // Aquí recibe el golpe físico del Asechador o el daño por oscuridad
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
    }
}