using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Audio")]
    [SerializeField] private AudioClip clipDano;
    [SerializeField] private AudioClip clipMorir;
    [SerializeField] private AudioClip clipCurar;

    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    // Evento para avisar a otros scripts (como logros o enemigos) que el jugador murio
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        // El jugador inicia la partida con la vida al maximo
        currentHealth = maxHealth;
    }

    // Se ejecuta cuando el jugador recibe un golpe
    public void TakeDamage(int amount)
    {
        // Reproducimos el sonido de daño en 2D a traves del AudioManager
        if (AudioManager.Instance != null && clipDano != null)
        {
            AudioManager.Instance.PlaySFX2D(clipDano, .5f);
        }

        // Restamos la vida y actualizamos los corazones o la barra en la interfaz
        currentHealth -= amount;
        UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        Debug.Log($"Jugador dañado. Vida restante: {currentHealth}");

        // Si la vida llega a cero o menos, se activa la logica de muerte
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Controla todo lo que pasa cuando el jugador se queda sin vida
    private void Die()
    {
        // Reproducimos el sonido de muerte
        if (AudioManager.Instance != null && clipMorir != null)
        {
            AudioManager.Instance.PlaySFX2D(clipMorir, 1f);
        }
        Debug.Log("El jugador ha muerto");

        // Disparamos el evento de muerte para avisar al resto del juego
        OnPlayerDeath?.Invoke();

        // Liberamos y mostramos el mouse para que el jugador pueda usar los botones del menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cargamos la pantalla de muerte (debe ser el indice 2 en los Build Settings)
        SceneManager.LoadScene(2);
    }

    // Se ejecuta al usar un objeto de curacion como una pocion
    public void Heal(int amount)
    {
        // Sumamos la salud recibida
        currentHealth += amount;

        // Nos aseguramos de que la vida no pase del limite maximo permitido
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Actualizamos los cambios en la interfaz de usuario
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }

        // Reproducimos el efecto de sonido de curacion
        if (AudioManager.Instance != null && clipCurar != null)
        {
            AudioManager.Instance.PlaySFX2D(clipCurar, 1f);
        }

        Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
    }
}