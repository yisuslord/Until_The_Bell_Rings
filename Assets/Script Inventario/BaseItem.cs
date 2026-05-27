using UnityEngine;

public abstract class BaseItem : MonoBehaviour, IInteractable, IInventoryItem
{
    [Header("Item Info")]
    [SerializeField] protected string itemName;
    [SerializeField] private Sprite inventoryIcon;

    // Propiedades publicas de lectura para los componentes de la interfaz de usuario (UI)
    public Sprite InventoryIcon => inventoryIcon;
    public string ItemName => itemName;

    [Header("Physics Settings")]
    public LayerMask enemyLayer;

    // Implementacion por defecto de IInventoryItem. Devuelve falso al no requerir logica de corrupcion en items base.
    public bool IsCorrupted => false;

    protected virtual void Start()
    {
        // Metodo virtual para inicializaciones especificas en clases derivadas
    }

    // Gestion de la interaccion fisica del jugador con el item en el escenario
    public virtual void Interact()
    {
        // Localizacion del componente central de inventario para procesar la recoleccion
        var inv = Object.FindFirstObjectByType<Inventario>();
        if (inv != null && inv.addItem(this))
        {
            OnCollected();
        }
    }

    // Define el comportamiento inmediato del objeto tras ser aceptado por el inventario
    protected virtual void OnCollected()
    {
        // Se desactiva el objeto de la escena para conservarlo en memoria dentro del buffer del inventario
        gameObject.SetActive(false);
    }

    // Metodo abstracto obligatorio para definir la logica de consumo o activacion de cada item derivado
    public abstract void Use();

    // Metodo virtual auxiliar para la obtencion de clips de audio especificos del item
    public virtual AudioClip GetClip() { return null; }
}