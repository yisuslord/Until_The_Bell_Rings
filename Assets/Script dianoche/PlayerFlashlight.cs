using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [SerializeField] private float noiseInterval = 1f;
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private float timer;
    private FlashlightController controller; // Referencia al padre

    void Awake()
    {
        // Buscamos el controlador en el objeto padre
        controller = GetComponentInParent<FlashlightController>();
    }

    void Update()
    {
        // 🔥 EL CANDADO: Si el controlador dice que está apagada, no hacemos nada
        if (controller == null || !controller.IsOn) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EmitLightNoise();
            timer = noiseInterval;
        }
    }

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