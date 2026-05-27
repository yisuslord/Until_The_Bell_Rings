using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Altar : MonoBehaviour, IDamageable
{
    [Header("Altar Stats")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;

    [Header("Continuous Drain Settings")]
    [Tooltip("Cuánto daño por segundo hace CADA vela apagada/corrupta")]
    [SerializeField] private float baseDamagePerSecond = 0.5f;

    private List<Candle> allCandlesInScene = new List<Candle>();
    private bool isGameOver = false;

    // Localiza de forma automatica e incondicional todas las instancias del script Candle distribuidas en la escena al iniciar.
    // Carga las referencias de las velas en la coleccion interna del Altar para poder monitorear sus estados de salud globales e inicializa la barra de vida al maximo.
    private void Awake()
    {
        Candle[] foundCandles = Object.FindObjectsByType<Candle>(FindObjectsSortMode.None);
        allCandlesInScene.AddRange(foundCandles);
        currentHealth = maxHealth;
    }

    // Sincroniza la representacion grafica inicial de los componentes del HUD del juego.
    // Se conecta directamente con el UIManager del sistema para refrescar la salud del Altar y el totalizador del marcador de las velas de la escena.
    private void Start()
    {
        UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);
        UpdateCandlesUI();
    }

    // Monitorea frame a frame el estado fisico de las velas para aplicar un daño constante sobre la vida del Altar.
    // Se conecta con los estados individuales de las velas (IsLit / IsCorrupted) y con los metodos de actualizacion visual de la UI.
    // Su proposito de diseño es forzar un drenaje continuo, dinamico y suave en la salud del Altar que escala de acuerdo al numero total de velas desatendidas por el jugador, provocando el fin de la partida si el indicador llega a cero.
    private void Update()
    {
        if (isGameOver) return;

        int unlitCount = 0;
        foreach (Candle candle in allCandlesInScene)
        {
            if (candle != null && (!candle.IsLit || candle.IsCorrupted))
                unlitCount++;
        }

        if (unlitCount > 0)
        {
            float totalDamageThisFrame = unlitCount * baseDamagePerSecond * Time.deltaTime;

            currentHealth -= totalDamageThisFrame;

            if (currentHealth < 0) currentHealth = 0;

            UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                TriggerGameOver();
            }
        }
    }

    // Permite que las velas del escenario informen al Altar cuando sufren una alteracion en sus estados individuales.
    // Se conecta con las llamadas delegadas de las velas individuales para refrescar los marcadores de la interfaz publica de inmediato.
    public void NotifyCandleChanged()
    {
        UpdateCandlesUI();
    }

    // Contabiliza el numero exacto de velas encendidas puras contra las velas apagadas o infectadas presentes.
    // Se conecta con el metodo UpdateCandleCount expuesto por el UIManager.
    // Sirve para actualizar de manera constante el panel numerico del HUD del jugador para que conozca el estado del mapa.
    private void UpdateCandlesUI()
    {
        int lit = 0;
        int unlit = 0;
        foreach (Candle c in allCandlesInScene)
        {
            if (c == null) continue;
            if (c.IsLit && !c.IsCorrupted) lit++;
            else unlit++;
        }
        UIManager.Instance.UpdateCandleCount(lit, unlit);
    }

    // Aplica una deduccion de vida directa y de impacto instantaneo sobre el Altar.
    // Heredado de la interfaz IDamageable, se conecta con los metodos de ataque directo de enemigos de choque repentino (como las arremetidas del Acechador).
    // Permite restar bloques de vida fijos y discretos a la salud, enviando el resultado a la UI y validando el fin del juego si el daño destruye el nucleo.
    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UIManager.Instance.UpdateAltarHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    // Ejecuta de forma irreversible la secuencia de derrota del juego al destruirse el altar.
    // Se conecta con el administrador de escenas (SceneManager) y con la escala de tiempo global de Unity.
    // Pausa por completo todo el movimiento de los sistemas fisicos del juego congelando el motor y carga de inmediato la pantalla o escena asignada al Game Over.
    private void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("<color=red>EL ALTAR SE HA EXTINGUIDO. LA OSCURIDAD REINA.</color>");
        Time.timeScale = 0f;
        SceneManager.LoadScene(2);
    }
}