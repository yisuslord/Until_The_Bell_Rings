using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.AI; // 🔥 Necesario para modificar las velocidades del NavMeshAgent

public enum GameState { Day, Night }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración de Nivel")]
    public int currentLevel = 1;
    public GameState currentState = GameState.Day;
    public float nightDuration = 60f;
    private float timer;

    [Header("Configuración de UI")]
    public GameObject uiRelojContenedor;
    public TextMeshProUGUI textoTiempo;

    [Header("Configuración de Luz")]
    public Light2D lightGlobal;
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.15f;
    public float lightTransitionSpeed = 1.5f;

    [Header("Referencias de Enemigos")]
    public List<EnemyBase> allEnemies; // Asegúrate de arrastrar aquí a tus Sensibles, Manifestados y Corruptores de la escena
    public Asechador stalker;

    private void Awake() { Instance = this; }

    private void Start()
    {
        SetDay();
    }

    private void Update()
    {
        if (lightGlobal != null)
        {
            float targetIntensity = (currentState == GameState.Day) ? dayIntensity : nightIntensity;
            lightGlobal.intensity = Mathf.Lerp(lightGlobal.intensity, targetIntensity, Time.deltaTime * lightTransitionSpeed);
        }

        if (currentState == GameState.Night)
        {
            timer -= Time.deltaTime;
            ActualizarInterfazReloj();

            if (timer <= 0)
            {
                EndNight();
            }
        }
    }

    private void ActualizarInterfazReloj()
    {
        if (textoTiempo != null)
        {
            float tiempoMostrar = Mathf.Max(0, timer);
            int minutos = Mathf.FloorToInt(tiempoMostrar / 60);
            int segundos = Mathf.FloorToInt(tiempoMostrar % 60);
            textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void StartNight()
    {
        currentState = GameState.Night;
        timer = nightDuration;

        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(true);
        if (flashlight != null) flashlight.SetActive(true);

        // 🔥 1. Ajustar las estadísticas de los enemigos según el nivel actual
        ConfigurarEstadisticasPorNivel();

        // 2. Activar enemigos correspondientes
        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(true);
        }

        // Control del Acechador (Solo entra a partir del Nivel 2)
        if (stalker != null)
        {
            if (currentLevel >= 2)
            {
                stalker.gameObject.SetActive(true);
            }
            else
            {
                stalker.gameObject.SetActive(false);
            }
        }
    }

    public void EndNight()
    {
        currentLevel++;
        SetDay();
    }

    private void SetDay()
    {
        currentState = GameState.Day;
        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(false);
        if (flashlight != null) flashlight.SetActive(false);

        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(false);
        }
        if (stalker != null) stalker.gameObject.SetActive(false);
    }

    /// <summary>
    /// Modifica los parámetros de la IA dinámicamente según el nivel actual + un factor aleatorio.
    /// </summary>
    private void ConfigurarEstadisticasPorNivel()
    {
        // Factor de variación aleatoria (entre -7% y +7% de rendimiento en las estadísticas base del nivel)
        float randomFactor = Random.Range(-0.07f, 0.07f);

        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            // 🔍 DETECTAR QUÉ TIPO DE ENEMIGO ES:

            // =============== LÓGICA DEL SENSIBLE ===============
            if (enemy is Sensible sensible)
            {
                if (currentLevel == 1) { agent.speed = 3.2f; }
                else if (currentLevel == 2) { agent.speed = 3.5f; }
                else // Nivel 3+
                {
                    agent.speed = 3.9f + (3.9f * randomFactor); // Más veloz en nivel 3
                }
                Debug.Log($"[LevelManager] Sensible configurado - Velocidad: {agent.speed}");
            }

            // =============== LÓGICA DEL MANIFESTADO ===============
            else if (enemy is Manifestado manifestado)
            {
                // El manifestado es espectral, lo hacemos notablemente más lento que el sensible
                if (currentLevel == 1) { agent.speed = 2.0f; }
                else if (currentLevel == 2) { agent.speed = 2.3f; }
                else // Nivel 3+
                {
                    agent.speed = 2.6f + (2.6f * randomFactor);
                }
                Debug.Log($"[LevelManager] Manifestado configurado - Velocidad: {agent.speed}");
            }

            // =============== LÓGICA DEL CORRUPTOR ===============
            else if (enemy is CorruptorEnemy corruptor)
            {
                // El corruptor se vuelve drásticamente más veloz en cada nivel para alcanzar sus objetivos
                if (currentLevel == 1)
                {
                    agent.speed = 2.5f;
                    corruptor.waitBeforeAttempt = 4.0f;
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 3.4f;
                    corruptor.waitBeforeAttempt = 3.2f;
                }
                else // Nivel 3+
                {
                    agent.speed = 4.5f + (4.5f * randomFactor); // Muy rápido en nivel 3
                    corruptor.waitBeforeAttempt = 2.0f;        // Corrompe el doble de rápido
                }
                Debug.Log($"[LevelManager] Corruptor configurado - Velocidad: {agent.speed}, Tiempo Sabotaje: {corruptor.waitBeforeAttempt}");
            }
        }

        // =============== LÓGICA DEL ACECHADOR (JEFE) ===============
        if (stalker != null && currentLevel >= 2)
        {
            NavMeshAgent stalkerAgent = stalker.GetComponent<NavMeshAgent>();

            if (currentLevel == 2)
            {
                if (stalkerAgent != null) stalkerAgent.speed = 3.0f;
                stalker.attemptInterval = 10f;
                stalker.attackChance = 40f;
            }
            else // Nivel 3+
            {
                // En el nivel 3 el Acechador es una pesadilla para el Altar
                if (stalkerAgent != null) stalkerAgent.speed = 3.6f + (3.6f * randomFactor);
                stalker.attemptInterval = 6f;  // Piensa si atacar el altar mucho más seguido (cada 6s en vez de 10s)
                stalker.attackChance = 60f + (60f * randomFactor); // La probabilidad base sube al 60%
            }
            Debug.Log($"[LevelManager] Acechador configurado - Velocidad: {stalkerAgent.speed}, Intervalo Altar: {stalker.attemptInterval}s, Probabilidad: {stalker.attackChance}%");
        }
    }

    [Header("Referencias Extras Legacy")]
    public GameObject flashlight;
}