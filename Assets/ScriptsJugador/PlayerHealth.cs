using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public static event Action OnPlayerDeath; 

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Jugador dañado. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("El jugador ha muerto");
        OnPlayerDeath?.Invoke();
        
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        // Si la curación supera el máximo, la igualamos al máximo
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
    }
}