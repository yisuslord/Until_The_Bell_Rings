using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Configuración")]
    public int maxItems = 5;
    public int actItemIndex = 0;
    public BaseItem defaultItem; // El item vacío (Placeholder)

    [Header("Estado del Inventario")]
    public List<IInventoryItem> Inventory = new List<IInventoryItem>();

    [Header("Audio")]
    [SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioClip Recoger;
    void Awake()
    {
        // Inicializamos con espacios vacíos
        for (int i = 0; i < maxItems; i++)
        {
            Inventory.Add(defaultItem);

        }
    }

    void Update()
    {
        // 1. Selección de Slots (1-5)
        ManejarSeleccion();

        // 2. Usar Item (Tecla Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UsarItemActual();
        }

        // 3. Soltar Item (Tecla R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            SoltarItemActual();
        }
    }

    private void ManejarSeleccion()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) actItemIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) actItemIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) actItemIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) actItemIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) actItemIndex = 4;

        // Limitar el índice por seguridad
        actItemIndex = Mathf.Clamp(actItemIndex, 0, maxItems - 1);
    }

    private void UsarItemActual()
    {
        IInventoryItem item = Inventory[actItemIndex];

        if (item != null && item != (IInventoryItem)defaultItem)
        {
            // 1. Antes de hacer nada, disparamos el sonido si el item tiene uno
            // Intentamos convertir el item a BaseItem para leer su clip de sonido
            if (item is BaseItem baseItem && baseItem.GetClip() != null)
            {
                playerSource.PlayOneShot(baseItem.GetClip());
            }

            item.Use();
            RemoverItemActual();
        }
    }

    private void SoltarItemActual()
    {
        IInventoryItem item = Inventory[actItemIndex];

        if (item != null && item != (IInventoryItem)defaultItem)
        {
            // Convertimos a componente para volverlo a activar en el mundo
            MonoBehaviour itemComponent = item as MonoBehaviour;
            if (itemComponent != null)
            {
                itemComponent.gameObject.SetActive(true);
                itemComponent.transform.SetParent(null); // Lo sacamos del hijo del jugador
                itemComponent.transform.position = transform.position + (Vector3)Random.insideUnitCircle; // Lo suelta cerca
            }

            RemoverItemActual();
            Debug.Log("Item soltado.");
        }
    }

    public void RemoverItemActual()
    {
        Inventory[actItemIndex] = defaultItem;
    }

    public void AddItem(IInventoryItem newItem)
    {
        //playerSource.PlayOneShot(Recoger);


        for (int i = 0; i < Inventory.Count; i++)
        {
            if (Inventory[i] == defaultItem || Inventory[i] == null)
            {
                Inventory[i] = newItem;
                return;
            }
        }
    }
}