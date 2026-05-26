using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
        // 🔥 LLAMADA AL AUDIO MANAGER ANTES DE DESTRUIR EL OBJETO
        if (AudioManager.Instance != null && clipDano != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipDano, .5f);
        }

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
        // 🔥 LLAMADA AL AUDIO MANAGER ANTES DE DESTRUIR EL OBJETO
        if (AudioManager.Instance != null && clipMorir != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipMorir, 1f);
        }
        Debug.Log("El jugador ha muerto");

        // 1. Ejecutamos el evento por si otros sistemas necesitan saberlo
        OnPlayerDeath?.Invoke();

        // 3. Liberamos el cursor (IMPORTANTE para poder clicar los botones del menú de muerte)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 4. Mandamos a la escena 2 (DeathScene)
        // Asegúrate de que en Build Settings la escena de muerte tenga el índice 2
        SceneManager.LoadScene(2);
    }

    public void Heal(int amount)
    {
        

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

        // 🔥 LLAMADA AL AUDIO MANAGER ANTES DE DESTRUIR EL OBJETO
        if (AudioManager.Instance != null && clipCurar != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipCurar, 1f);
        }

        Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
    }
}