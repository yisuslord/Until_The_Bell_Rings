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
        // Factor de variación aleatoria (entre -7% y +7%)
        float randomFactor = Random.Range(-0.07f, 0.07f);

        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            // =============== LÓGICA DEL SENSIBLE ===============
            if (enemy is Sensible sensible)
            {
                if (currentLevel == 1) { agent.speed = 3.2f; }
                else if (currentLevel == 2) { agent.speed = 3.5f; }
                else // Nivel 3+
                {
                    agent.speed = 3.9f + (3.9f * randomFactor);
                }
                Debug.Log($"[LevelManager] Sensible configurado - Velocidad: {agent.speed}");
            }

            // =============== LÓGICA DEL MANIFESTADO ===============
            else if (enemy is Manifestado manifestado)
            {
                if (currentLevel == 1)
                {
                    agent.speed = 2.0f;
                    manifestado.darknessThreshold = 4.5f; // Tarda más en aparecer al inicio
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 2.3f;
                    manifestado.darknessThreshold = 3.0f; // Tiempo estándar
                }
                else // Nivel 3+
                {
                    agent.speed = 2.6f + (2.6f * randomFactor);
                    // Aparece rapidísimo en la oscuridad (entre 1.3s y 1.7s aprox)
                    manifestado.darknessThreshold = 1.5f + (1.5f * randomFactor);
                }
                Debug.Log($"[LevelManager] Manifestado - Velocidad: {agent.speed}, Tiempo en Oscuridad: {manifestado.darknessThreshold}s");
            }

            // =============== LÓGICA DEL CORRUPTOR ===============
            else if (enemy is CorruptorEnemy corruptor)
            {
                if (currentLevel == 1)
                {
                    agent.speed = 2.5f;
                    corruptor.waitBeforeAttempt = 4.0f;
                    corruptor.scanInterval = 2.0f;       // Escanea lento el mapa
                    corruptor.successChance = 40f;       // 40% probabilidad de corromper
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 3.4f;
                    corruptor.waitBeforeAttempt = 3.2f;
                    corruptor.scanInterval = 1.5f;       // Escaneo estándar
                    corruptor.successChance = 60f;       // 60% probabilidad de corromper
                }
                else // Nivel 3+
                {
                    agent.speed = 4.5f + (4.5f * randomFactor);
                    corruptor.waitBeforeAttempt = 2.0f;
                    corruptor.scanInterval = 0.8f + (1.0f * randomFactor); // Escanea el doble de rápido
                    corruptor.successChance = 85f + (85f * randomFactor);  // Casi un sabotaje garantizado (85% base)
                }
                Debug.Log($"[LevelManager] Corruptor - Velocidad: {agent.speed}, Scan: {corruptor.scanInterval}s, Éxito: {corruptor.successChance}%");
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
                if (stalkerAgent != null) stalkerAgent.speed = 3.6f + (3.6f * randomFactor);
                stalker.attemptInterval = 6f;
                stalker.attackChance = 60f + (60f * randomFactor);
            }
            Debug.Log($"[LevelManager] Acechador - Velocidad: {stalkerAgent.speed}, Intervalo Altar: {stalker.attemptInterval}s, Probabilidad: {stalker.attackChance}%");
        }
    }

    [Header("Referencias Extras Legacy")]
    public GameObject flashlight;
}