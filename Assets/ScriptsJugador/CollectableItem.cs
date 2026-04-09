using UnityEngine;
using System.Collections;

public class CollectibleItem : MonoBehaviour, IInteractable, ICorruptible
{
    [SerializeField] private string itemName;
    [SerializeField] private float detectionRadius = 10f; // Rango para avisar al Corruptor
    [SerializeField] private LayerMask enemyLayer;

    private bool isCorrupted = false;
    public bool IsCorrupted => isCorrupted;

    private void Start()
    {
        // En cuanto el objeto "nace" en la iglesia, avisa a los corruptores cercanos
        EmitPresence();
    }

    public void EmitPresence()
    {
        if (isCorrupted) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                receiver.OnStimulusReceived(transform.position, StimulusType.Corruptible);
            }
        }
    }

    public void Interact()
    {
        if (isCorrupted)
        {
            Debug.Log("El objeto está corrompido y no puedes tocarlo.");
            return;
        }

        // Si no está corrompido, se añade al inventario y oculta de la escena
        Object.FindFirstObjectByType<PlayerInventory>().AddItem(itemName);

        // Se anade el objeto al inventario del jugador y se desactiva de la escena
        if (Object.FindFirstObjectByType<Inventario>().addItem(gameObject.GetComponent<IInventoryItem>()))
        {
            gameObject.SetActive(false);
        }

        
    }

    public void Corrupt()
    {
        isCorrupted = true;
        Debug.Log($"<color=purple>{itemName} ha sido corrompido.</color>");
        StartCoroutine(RestoreTimer());
    }

    public void Restore()
    {
        isCorrupted = false;
    }

    private IEnumerator RestoreTimer()
    {
        yield return new WaitForSeconds(120f); // 2 minutos bloqueado
        Restore();
    }
}