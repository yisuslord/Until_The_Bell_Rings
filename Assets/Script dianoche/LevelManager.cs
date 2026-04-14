using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal; // Necesario para controlar luces 2D

public enum GameState { Day, Night }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración de Nivel")]
    public int currentLevel = 1;
    public GameState currentState = GameState.Day;
    public float nightDuration = 60f;
    private float timer;

    [Header("Iluminación")]
    public Light2D globalLight;
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.15f;
    public float transitionSpeed = 2f;

    [Header("Referencias")]
    public List<EnemyBase> allEnemies;
    public Asechador stalker;

    private void Awake() { Instance = this; }

    private void Start()
    {
        SetDay();
    }

    private void Update()
    {
        // Transición suave de la luz (Día/Noche)
        float targetIntensity = (currentState == GameState.Day) ? dayIntensity : nightIntensity;
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);

        // Temporizador de la noche
        if (currentState == GameState.Night)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndNight();
            }
        }
    }

    public void StartNight()
    {
        currentState = GameState.Night;
        timer = nightDuration;

        // Configuración de enemigos por nivel
        foreach (var enemy in allEnemies)
        {
            enemy.gameObject.SetActive(true);
        }

        // El Acechador solo aparece del nivel 2 en adelante
        if (currentLevel < 2 && stalker != null)
        {
            stalker.gameObject.SetActive(false);
        }

        Debug.Log("Iniciando Noche del Nivel " + currentLevel);
    }

    public void EndNight()
    {
        currentLevel++;
        SetDay();
        Debug.Log("Sobreviviste. Ahora es el día del Nivel " + currentLevel);
    }

    private void SetDay()
    {
        currentState = GameState.Day;

        // Desactivar a todos los enemigos
        foreach (var enemy in allEnemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}