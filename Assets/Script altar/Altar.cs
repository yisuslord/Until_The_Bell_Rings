using UnityEngine;
using System.Collections.Generic;

public class Altar : MonoBehaviour
{
    [Header("Altar Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;

    [Header("Detection Settings")]
    private List<Candle> allCandlesInScene = new List<Candle>();

    private void Awake()
    {
        Candle[] foundCandles = Object.FindObjectsByType<Candle>(FindObjectsSortMode.None);
        allCandlesInScene.AddRange(foundCandles);
        maxHealth = allCandlesInScene.Count;

        // Quitamos el UpdateAltarHealth de aquí para evitar el check instantáneo
    }

    private void Start()
    {
        // Ejecutamos el primer chequeo un frame después o al final del Start
        UpdateAltarHealth();
    }

    public void UpdateAltarHealth()
    {
        int activeLights = 0;
        foreach (Candle candle in allCandlesInScene)
        {
            if (candle.IsLit && !candle.IsCorrupted) activeLights++;
        }

        currentHealth = activeLights;
        Debug.Log($"<color=cyan>Altar Salud: {currentHealth}/{maxHealth}</color>");

        // Solo activamos el Game Over si ya pasó el arranque inicial
        // Y verificamos que realmente no haya luces
        if (currentHealth <= 0 && maxHealth > 0 && Time.timeSinceLevelLoad > 0.1f)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("<color=red>EL ALTAR SE HA EXTINGUIDO. LA OSCURIDAD REINA.</color>");

        // Detenemos el tiempo (puedes cambiar esto por tu lógica de UI de derrota)
        Time.timeScale = 0f;

        // Nota: Aquí podrías disparar un evento para que el jugador deje de moverse
    }
}