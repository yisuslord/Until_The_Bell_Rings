using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [SerializeField] private float noiseInterval = 1f;
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private float timer;

    void Update()
    {
        // Solo funciona si el GameObject está encendido en el Inspector
        if (gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EmitLightNoise();
                timer = noiseInterval;
            }
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