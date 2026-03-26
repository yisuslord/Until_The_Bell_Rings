using UnityEngine;


public class Candle : MonoBehaviour, IInteractable
{
    private bool isLit = false;
    [SerializeField] private float lightRadius = 8f;
    [SerializeField] private LayerMask enemyLayer; // Configura esto a la capa "Enemies"

    public void Interact()
    {
        if (!isLit)
        {
            LightCandle();
        }
    }

    private void LightCandle()
    {
        isLit = true;

        Debug.Log("Encendiendo vela");

        // Buscamos receptores de estímulos en el área
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, lightRadius, enemyLayer);

        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                receiver.OnStimulusReceived(transform.position, StimulusType.Light);
            }
        }

    }
}