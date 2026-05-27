using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [SerializeField] private float noiseInterval = 1f;
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private float timer;
    private FlashlightController controller;

    // Obtiene la referencia del controlador de la linterna situado en el objeto padre al inicializar el script.
    // Es necesario para poder consultar si la linterna se encuentra encendida o apagada antes de emitir ruido.
    void Awake()
    {
        controller = GetComponentInParent<FlashlightController>();
    }

    // Ejecuta un temporizador continuo en cada frame mientras la linterna este encendida.
    // Se conecta con el estado booleano del FlashlightController.
    // La razon de esto es obligar a la linterna a emitir pulsos de "ruido de luz" a intervalos regulares, actuando como un faro que atrae a los enemigos cercanos si el jugador la mantiene activa.
    void Update()
    {
        if (controller == null || !controller.IsOn) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EmitLightNoise();
            timer = noiseInterval;
        }
    }

    // Detecta de forma radial a los enemigos que se encuentren dentro del rango de la linterna.
    // Se conecta con la interfaz IStimulusReceiver de los enemigos detectados en la capa especifica.
    // Envia un estimulo de tipo ruido a los receptores para alertar a las inteligencias artificiales de la presencia del jugador a traves de la luz de la linterna.
    void EmitLightNoise()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, noiseRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                receiver.OnStimulusReceived(transform.position, StimulusType.Noise);
            }
        }
    }
}