using UnityEngine;
using System.Collections;

public abstract class BaseItem : MonoBehaviour, IInteractable, ICorruptible, IInventoryItem
{
    [Header("Item Info")]
    [SerializeField] protected string itemName;
    [SerializeField] protected float corruptionDuration = 120f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] public LayerMask enemyLayer;

    protected bool isCorrupted = false;
    public bool IsCorrupted => isCorrupted;
    public string ItemName => itemName;

    public Sprite InventoryIcon;

    // Al nacer en la escena, el objeto "emite su presencia"
    protected virtual void Start()
    {
        EmitPresence();
    }

    public void EmitPresence()
    {
        // Si el objeto ya fue corrompido, deja de llamar la atención
        if (isCorrupted) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                // Avisamos al Corruptor que aquí hay algo que puede romper
                receiver.OnStimulusReceived(transform.position, StimulusType.Corruptible);
            }
        }
    }

    public virtual void Interact()
    {
        if (isCorrupted)
        {
            Debug.Log($"{itemName} está corrompido.");
            return;
        }

        // Referencia al inventario (asegúrate de que el script se llame 'Inventario')
        var inv = Object.FindFirstObjectByType<Inventario>();
        if (inv != null && inv.addItem(this))
        {
            OnCollected();
        }
    }

    protected virtual void OnCollected()
    {
        gameObject.SetActive(false);
    }

    public abstract void Use();

    // --- Lógica de Corrupción ---
    public void Corrupt()
    {
        if (isCorrupted) return;
        isCorrupted = true;
        Debug.Log($"<color=purple>{itemName} ha sido saboteado.</color>");
        StartCoroutine(RestoreTimer());
    }

    public void Restore()
    {
        isCorrupted = false;
        EmitPresence(); // Al restaurarse, vuelve a ser un objetivo
    }

    private IEnumerator RestoreTimer()
    {
        yield return new WaitForSeconds(corruptionDuration);
        Restore();
    }

    // Para visualizar el rango en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public virtual AudioClip GetClip() { return null; }
}