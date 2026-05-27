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
    [SerializeField] private AudioClip musicaDia;
    [SerializeField] private AudioClip musicaNoche;
    [SerializeField] private float duracionFadeOut = 1.5f;

    [Header("Referencias Extras Legacy")]
    public GameObject flashlight;
    public GameObject padreGameObject;

    // Inicializa el patron Singleton para permitir que cualquier script del juego acceda de forma directa al LevelManager.
    private void Awake() { Instance = this; }

    // Configura el inicio del juego estableciendo de forma predeterminada la fase de dia.
    private void Start()
    {
        SetDay();
    }

    // Gestiona la transicion de la intensidad de la luz global y el reloj de la noche frame a frame.
    // Se conecta con el componente Light2D y el flujo de finalizacion de la fase nocturna.
    // Modifica de forma suave la iluminacion de la escena segun el estado actual y descuenta el tiempo de juego cuando es de noche para terminar el ciclo al llegar a cero.
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

    // Formatea el tiempo restante de la noche en minutos y segundos para mostrarlo en pantalla.
    // Se conecta directamente con el componente de texto de TextMeshPro de la interfaz.
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

    // Activa la transicion hacia el estado nocturno del nivel.
    // Se conecta con la UI del reloj, el objeto de la linterna, las corrutinas de audio y la configuracion de estadisticas de los enemigos.
    // Oculta los elementos diurnos del mapa, inicializa el temporizador de supervivencia, cambia la musica ambiental a la de tension e incrementa los atributos de velocidad y comportamiento de las amenazas segun la dificultad del nivel.
    public void StartNight()
    {
        currentState = GameState.Night;
        timer = nightDuration;

        padreGameObject.SetActive(false);

        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(true);
        if (flashlight != null) flashlight.SetActive(true);

        StartCoroutine(TransicionMusicaFase(musicaNoche));

        ConfigurarEstadisticasPorNivel();

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

    // Determina el flujo del juego al terminar con exito el tiempo de la noche.
    // Se conecta con los objetos heredados de la escena y el cargador de niveles de Unity.
    // Si el jugador supera la noche del nivel 3 activa la secuencia de victoria; en caso contrario, incrementa el nivel actual y restablece el ciclo diurno para prepararse para la siguiente ronda.
    public void EndNight()
    {
        padreGameObject.SetActive(true);
        if (currentLevel >= 3)
        {
            StartCoroutine(SecuenciaVictoriaRoutine());
        }
        else
        {
            currentLevel++;
            SetDay();
        }
    }

    // Corrutina encargada de procesar el estado de victoria del juego.
    // Se conecta con el sistema de escenas de Unity mediante SceneManager.
    // Devuelve la iluminacion de dia al entorno para dar retroalimentacion visual, detiene la ejecucion unos segundos para permitir que el jugador asimile el exito y carga la escena final del juego.
    private System.Collections.IEnumerator SecuenciaVictoriaRoutine()
    {
        SetDay();

        Debug.Log("<color=green>[LevelManager] ¡Nivel 3 completado! Sobreviviste. Iniciando transición al WinState...</color>");

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Winstate");
    }

    // Establece los parametros correspondientes a la fase segura o de dia.
    // Se conecta con los elementos de la interfaz de usuario, la linterna del jugador y los objetos de los enemigos de la escena.
    // Desactiva visualmente el reloj, apaga la linterna, limpia el mapa ocultando a todos los enemigos en ejecucion y cambia la musica ambiental por una pista relajante.
    private void SetDay()
    {
        currentState = GameState.Day;
        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(false);
        if (flashlight != null) flashlight.SetActive(false);

        StartCoroutine(TransicionMusicaFase(musicaDia));

        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(false);
        }
        if (stalker != null) stalker.gameObject.SetActive(false);
    }

    // Corrutina de desvanecimiento e intercambio de las pistas de musica de fondo.
    // Se conecta de forma directa con los metodos globales del AudioManager.
    // Realiza un fade-out suave de la musica actual, aguarda a que el volumen se reduzca por completo y reproduce la pista asignada al nuevo estado para evitar transiciones de sonido abruptas o molestas para el jugador.
    private System.Collections.IEnumerator TransicionMusicaFase(AudioClip nuevaPista)
    {
        if (AudioManager.Instance == null || nuevaPista == null) yield break;

        AudioManager.Instance.FadeOutMusic(duracionFadeOut);

        yield return new WaitForSeconds(duracionFadeOut);

        AudioManager.Instance.PlayMusic(nuevaPista, .5f);
    }

    // Modifica los componentes NavMeshAgent y los parametros especificos de cada tipo de enemigo de la escena.
    // Se conecta directamente con las instancias de los scripts de los enemigos (Sensible, Manifestado, CorruptorEnemy y Asechador).
    // Su objetivo es escalar la dificultad del juego de manera progresiva a traves de los niveles (1, 2 y 3), incrementando velocidades de movimiento y reduciendo tiempos de reaccion de forma matematica, agregando ademas una variacion aleatoria en el nivel 3 para romper la predictibilidad de las rutas de los enemigos.
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
                    agent.speed = 5f;
                    corruptor.waitBeforeAttempt = 4.0f;
                    corruptor.scanInterval = 1.5f;
                    corruptor.successChance = 80f;
                }
                else if (currentLevel == 2)
                {
                    agent.speed = 6f;
                    corruptor.waitBeforeAttempt = 3.2f;
                    corruptor.scanInterval = 1f;
                    corruptor.successChance = 90f;
                }
                else
                {
                    agent.speed = 7f + (4.5f * randomFactor);
                    corruptor.waitBeforeAttempt = 2.0f;
                    corruptor.scanInterval = 0.5f + (1.0f * randomFactor);
                    corruptor.successChance = 100f + (5f * randomFactor);
                }
            }
        }

        if (stalker != null && currentLevel >= 2)
        {
            NavMeshAgent stalkerAgent = stalker.GetComponent<NavMeshAgent>();

            if (currentLevel == 2)
            {
                if (stalkerAgent != null) stalkerAgent.speed = 2f;
                stalker.attemptInterval = 15f;
                stalker.attackChance = 40f;
            }
            else
            {
                if (stalkerAgent != null) stalkerAgent.speed = 3f + (3.6f * randomFactor);
                stalker.attemptInterval = 10f;
                stalker.attackChance = 60f + (60f * randomFactor);
            }
        }
    }
}