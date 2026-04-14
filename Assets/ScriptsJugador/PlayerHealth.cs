using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Audio")]
    //[SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioClip clipDano;
    //[SerializeField] private AudioClip clipCaminar;
    [SerializeField] private AudioClip clipMorir;
    [SerializeField] private AudioClip clipCurar;

    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    public static event Action OnPlayerDeath; 

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        // Cambiamos InParent por GetComponent normal, ya que el AudioSource vive en el mismo Player
        AudioSource pSource = GetComponent<AudioSource>();
        pSource.PlayOneShot(clipDano);

        currentHealth -= amount;
        UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
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
        AudioSource pSource = GetComponent<AudioSource>();

        // 1. Sumamos la vida primero
        currentHealth += amount;

        // 2. IMPORTANTE: Validamos el máximo ANTES de avisar a la UI
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // 3. Ahora sí, actualizamos la barra con el valor final correcto
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }

        // 4. Sonido
        if (pSource != null && clipCurar != null)
        {
            pSource.PlayOneShot(clipCurar);
            Debug.Log("Sonido de curación ejecutado.");
        }

        Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
    }
}