using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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
    public List<EnemyBase> allEnemies;
    public Asechador stalker;

    [Header("Música Dinámica (Día / Noche)")]
    [SerializeField] private AudioClip musicaDia;     // 🔥 Arrastra el audio relajante de día
    [SerializeField] private AudioClip musicaNoche;   // 🔥 Arrastra el audio tenso de noche
    [SerializeField] private float duracionFadeOut = 1.5f; // Segundos que tardará en apagarse la pista anterior

    [Header("Referencias Extras Legacy")]
    public GameObject flashlight;
    public GameObject padreGameObject;

    private void Awake() { Instance = this; }

    private void Start()
    {
        // Al iniciar la escena, forzamos de inmediato el estado del día original
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

        padreGameObject.SetActive(false);

        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(true);
        if (flashlight != null) flashlight.SetActive(true);

        // 🔥 Transición de Audio: Cambiamos a la música de noche de forma segura
        StartCoroutine(TransicionMusicaFase(musicaNoche));

        // Ajustar las estadísticas de los enemigos según el nivel actual
        ConfigurarEstadisticasPorNivel();

        // Activar enemigos correspondientes
        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(true);
        }

        if (stalker != null)
        {
            if (currentLevel >= 2) stalker.gameObject.SetActive(true);
            else stalker.gameObject.SetActive(false);
        }
    }

    public void EndNight()
    {
        padreGameObject.SetActive(true);
        // 🔥 Si se acaba la noche del nivel 3 (o superior), el jugador gana
        if (currentLevel >= 3)
        {
            StartCoroutine(SecuenciaVictoriaRoutine());
        }
        else
        {
            // Si va en nivel 1 o 2, avanza al siguiente día con normalidad
            currentLevel++;
            SetDay();
        }
    }

    // 🔥 CORRUTINA DE VICTORIA: Vuelve el día, congela la acción y cambia de escena
    private System.Collections.IEnumerator SecuenciaVictoriaRoutine()
    {
        // 1. Apagamos la lógica de la noche y restauramos el día (vuelve la luz global poco a poco)
        SetDay();

        Debug.Log("<color=green>[LevelManager] ¡Nivel 3 completado! Sobreviviste. Iniciando transición al WinState...</color>");

        // 2. Esperamos unos segundos para que el jugador asimile la victoria mientras sale el sol
        yield return new WaitForSeconds(2f);

        // 3. Cargamos la escena de ganar
        SceneManager.LoadScene("Winstate");
    }

    private void SetDay()
    {
        currentState = GameState.Day;
        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(false);
        if (flashlight != null) flashlight.SetActive(false);

        // 🔥 Transición de Audio: Regresamos a la música de día de forma segura
        StartCoroutine(TransicionMusicaFase(musicaDia));

        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(false);
        }
        if (stalker != null) stalker.gameObject.SetActive(false);
    }

    // 🔥 LA CORRUTINA DE TRANSICIÓN: Evita cortes bruscos en las pistas de fondo
    private System.Collections.IEnumerator TransicionMusicaFase(AudioClip nuevaPista)
    {
        if (AudioManager.Instance == null || nuevaPista == null) yield break;

        // 1. Iniciamos el desvanecimiento de la pista que esté sonando actualmente en el canal de música
        AudioManager.Instance.FadeOutMusic(duracionFadeOut);

        // 2. Esperamos en segundo plano a que el volumen llegue totalmente a cero
        yield return new WaitForSeconds(duracionFadeOut);

        // 3. Encendemos la nueva pista correspondiente a la fase
        AudioManager.Instance.PlayMusic(nuevaPista, .5f);
    }

    private void ConfigurarEstadisticasPorNivel()
    {
        float randomFactor = Random.Range(-0.07f, 0.07f);

        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            if (enemy is Sensible sensible)
            {
                if (currentLevel == 1) { agent.speed = 3.2f; }
                else if (currentLevel == 2) { agent.speed = 3.5f; }
                else { agent.speed = 3.9f + (3.9f * randomFactor); }
            }
            else if (enemy is Manifestado manifestado)
            {
                if (currentLevel == 1)
                {
                    agent.speed = 2.0f;
                    manifestado.darknessThreshold = 4.5f;
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 2.3f;
                    manifestado.darknessThreshold = 3.0f;
                }
                else
                {
                    agent.speed = 2.6f + (2.6f * randomFactor);
                    manifestado.darknessThreshold = 1.5f + (1.5f * randomFactor);
                }
            }
            else if (enemy is CorruptorEnemy corruptor)
            {
                if (currentLevel == 1)
                {
                    agent.speed = 3f;
                    corruptor.waitBeforeAttempt = 4.0f;
                    corruptor.scanInterval = 1.5f;
                    corruptor.successChance = 50f;
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 4f;
                    corruptor.waitBeforeAttempt = 3.2f;
                    corruptor.scanInterval = 1f;
                    corruptor.successChance = 70f;
                }
                else
                {
                    agent.speed = 6f + (4.5f * randomFactor);
                    corruptor.waitBeforeAttempt = 2.0f;
                    corruptor.scanInterval = 0.5f + (1.0f * randomFactor);
                    corruptor.successChance = 90f + (85f * randomFactor);
                }
            }
        }

        if (stalker != null && currentLevel >= 2)
        {
            NavMeshAgent stalkerAgent = stalker.GetComponent<NavMeshAgent>();

            if (currentLevel == 2)
            {
                if (stalkerAgent != null) stalkerAgent.speed = 3.0f;
                stalker.attemptInterval = 10f;
                stalker.attackChance = 40f;
            }
            else
            {
                if (stalkerAgent != null) stalkerAgent.speed = 3.6f + (3.6f * randomFactor);
                stalker.attemptInterval = 6f;
                stalker.attackChance = 60f + (60f * randomFactor);
            }
        }
    }
}