using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal; // ?? NUEVO: Necesario para controlar luces 2D
using TMPro;
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
    public GameObject uiRelojContenedor; // El objeto que contiene el icono y el texto
    public TextMeshProUGUI textoTiempo;  // El componente de texto real

    [Header("Configuración de Luz")]
    public Light2D lightGlobal;
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.15f;
    public float lightTransitionSpeed = 1.5f;

    [Header("Referencias")]
    public GameObject flashlight;
    public List<EnemyBase> allEnemies;
    public Asechador stalker;

    private void Awake() { Instance = this; }

    private void Start()
    {
        SetDay();
    }

    private void Update()
    {
        // Control de luz global
        if (lightGlobal != null)
        {
            float targetIntensity = (currentState == GameState.Day) ? dayIntensity : nightIntensity;
            lightGlobal.intensity = Mathf.Lerp(lightGlobal.intensity, targetIntensity, Time.deltaTime * lightTransitionSpeed);
        }

        // Temporizador de la noche
        if (currentState == GameState.Night)
        {
            timer -= Time.deltaTime;

            // 🔥 Actualizar el texto del reloj
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
            // Evitamos que el tiempo baje de 0 para el texto
            float tiempoMostrar = Mathf.Max(0, timer);

            // Formatear segundos a minutos:segundos (Ej: 01:25)
            int minutos = Mathf.FloorToInt(tiempoMostrar / 60);
            int segundos = Mathf.FloorToInt(tiempoMostrar % 60);

            textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void StartNight()
    {
        currentState = GameState.Night;
        timer = nightDuration;

        // 🔥 MOSTRAR el reloj al empezar la noche
        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(true);

        if (flashlight != null) flashlight.SetActive(true);

        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(true);
        }

        if (currentLevel < 2 && stalker != null)
        {
            stalker.gameObject.SetActive(false);
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

        // 🔥 OCULTAR el reloj al hacerse de día
        if (uiRelojContenedor != null) uiRelojContenedor.SetActive(false);

        if (flashlight != null) flashlight.SetActive(false);

        foreach (var enemy in allEnemies)
        {
            if (enemy != null) enemy.gameObject.SetActive(false);
        }
    }
}