using System.Collections;
using TMPro;
using UnityEngine;

public abstract class BaseItem : MonoBehaviour, IInteractable, ICorruptible, IInventoryItem
{
    // Modifica la línea de tu variable existente para que cumpla con la interfaz:
    [Header("Item Info")]
    [SerializeField] protected string itemName;
    [SerializeField] private Sprite inventoryIcon; // Ponlo en minúscula si quieres como variable

    // Y añade esta propiedad pública para que la UI pueda leerla:
    public Sprite InventoryIcon => inventoryIcon;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] public LayerMask enemyLayer;

    [Header("UI de Interacción")]
    [SerializeField] private TextMeshProUGUI textoInteraccionUI; // 🔥 Arrastra el texto aquí
    private IInventoryItem miItem;

    protected bool isCorrupted = false;
    public bool IsCorrupted => isCorrupted;
    public string ItemName => itemName;

    public float corruptionDuration = 10f;

    //public Sprite InventoryIcon;

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
            MostrarTextoCorrompido();
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

    private void MostrarTextoCorrompido()
    {
        if (textoInteraccionUI != null && isCorrupted)
        {
            textoInteraccionUI.text = $"La {miItem.ItemName} ha sido corrompida";
            textoInteraccionUI.gameObject.SetActive(true);

            // 🔥 Cancelamos cualquier cuenta atrás previa para evitar que se pisen entre sí
            StopAllCoroutines();

            // 🔥 Iniciamos la cuenta atrás de 2 segundos
            StartCoroutine(TemporizadorTextoCorrompido(2f));
        }
    }

    // 🔥 LA CORRUTINA: Se encarga de esperar y apagar el texto
    private System.Collections.IEnumerator TemporizadorTextoCorrompido(float tiempoDeEspera)
    {
        // El juego sigue corriendo, pero este método se frena aquí por 'tiempoDeEspera' segundos
        yield return new WaitForSeconds(tiempoDeEspera);

        // Pasados los 2 segundos, el código avanza y apaga la UI
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false);
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