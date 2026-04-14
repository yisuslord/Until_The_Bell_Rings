using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal; // ?? NUEVO: Necesario para controlar luces 2D

public enum GameState { Day, Night }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración de Nivel")]
    public int currentLevel = 1;
    public GameState currentState = GameState.Day;
    public float nightDuration = 60f;
    private float timer;

    [Header("Configuración de Luz")]
    // ?? CAMBIO: En vez de GameObject, ahora es de tipo Light2D
    public Light2D lightGlobal;
    public float dayIntensity = 1.0f;     // Intensidad de día
    public float nightIntensity = 0.15f;  // Intensidad de noche
    public float lightTransitionSpeed = 1.5f; // Qué tan rápido se oscurece

    [Header("Referencias")]
    public GameObject flashlight;    // La linterna del jugador
    public List<EnemyBase> allEnemies; // Arrastra aquí a todos tus enemigos
    public Asechador stalker;        // Referencia específica al Acechador

    private void Awake() { Instance = this; }

    private void Start()
    {
        SetDay();
    }

    private void Update()
    {
        // ?? NUEVO: Esto hace que la luz baje o suba suavemente cada frame
        if (lightGlobal != null)
        {
            float targetIntensity = (currentState == GameState.Day) ? dayIntensity : nightIntensity;
            lightGlobal.intensity = Mathf.Lerp(lightGlobal.intensity, targetIntensity, Time.deltaTime * lightTransitionSpeed);
        }

        // Temporizador original de la noche
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

        if (flashlight != null) flashlight.SetActive(true);

        // Configuración de enemigos por nivel
        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(true); // Agregué "if != null" por si acaso borras a alguno
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

        if (flashlight != null) flashlight.SetActive(false);

        // Desactivar a todos los enemigos
        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(false);
        }
    }
}