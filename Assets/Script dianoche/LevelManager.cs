using UnityEngine;
using System.Collections.Generic;

public enum GameState { Day, Night }

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Configuración de Nivel")]
    public int currentLevel = 1;
    public GameState currentState = GameState.Day;
    public float nightDuration = 60f;
    private float timer;

    [Header("Referencias")]
    public GameObject lightGlobal; // Tu Light2D Global
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

        // Aquí cambiarías la luz (ver script de abajo)
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
        flashlight.SetActive(false);

        // Desactivar a todos los enemigos
        foreach (var enemy in allEnemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}