using UnityEngine;

public abstract class BaseItem : MonoBehaviour, IInteractable, IInventoryItem
{
    [Header("Item Info")]
    [SerializeField] protected string itemName;
    [SerializeField] private Sprite inventoryIcon;

    // Propiedad pública para que la UI pueda leer el icono
    public Sprite InventoryIcon => inventoryIcon;
    public string ItemName => itemName;

    public LayerMask enemyLayer;

    // Nota: Dejamos esto solo por si la interfaz IInventoryItem o algún script viejo aún pregunta por ello, 
    // pero siempre devolverá falso ya que no hay mecánicas de corrupción aquí.
    public bool IsCorrupted => false;

    protected virtual void Start()
    {
        // Ya no emite presencia ni busca enemigos porque el corruptor ignora los ítems sueltos.
    }

    public virtual void Interact()
    {
        // El objeto se puede recolectar directamente sin trabas de corrupción
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

    public virtual AudioClip GetClip() { return null; }
}