using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    [Header("Settings")]
    public BaseItem defaultItem; // Cambiado a BaseItem para consistencia
    public int maxItems = 5;

    [Header("Current Status")]
    public int actItemIndex = 0;
    public IInventoryItem actItem;
    public List<IInventoryItem> Inventory;

    void Start()
    {
        Inventory = new List<IInventoryItem>();
        PopulateInventory();
        actItem = Inventory[actItemIndex];
    }

    void Update()
    {
        // Selección de items (Teclas 1-5)
        for (int i = 0; i < maxItems; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChooseItem(i);
            }
        }

        // Usar objeto activo (Tecla Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseActiveItem();
        }

        // Soltar objeto activo (Tecla R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveItem(actItem);
        }
    }

    private void UseActiveItem()
    {
        // Si el objeto actual no es el vacío/default
        if (actItem != null && actItem != (IInventoryItem)defaultItem)
        {
            actItem.Use(); // Llamamos al Use() de BaseItem

            // Si es un consumible (como la poción), lo eliminamos tras usarlo
            RemoveItem(actItem);
            Debug.Log($"Item en slot {actItemIndex} usado y consumido.");
        }
    }

    public void ChooseItem(int index)
    {
        if (index >= Inventory.Count) return;
        actItemIndex = index;
        actItem = Inventory[index];
        Debug.Log("Item activo: Slot " + actItemIndex);
    }

    public bool addItem(IInventoryItem newItem)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            // Buscamos un slot que tenga el item por defecto
            if (Inventory[i] == (IInventoryItem)defaultItem)
            {
                MonoBehaviour itemMB = newItem as MonoBehaviour;
                itemMB.gameObject.transform.SetParent(transform);
                itemMB.gameObject.SetActive(false); // Nos aseguramos de que no se vea en el mundo

                Inventory[i] = newItem;
                ChooseItem(i); // Lo seleccionamos automáticamente al recogerlo
                return true;
            }
        }
        Debug.Log("Inventario lleno");
        return false;
    }

    public void RemoveItem(IInventoryItem oldItem)
    {
        if (oldItem == (IInventoryItem)defaultItem) return;

        int index = Inventory.IndexOf(oldItem);
        if (index == -1) return;

        MonoBehaviour itemMB = oldItem as MonoBehaviour;

        // Si lo soltamos con R (no por uso), lo devolvemos al mundo
        if (Input.GetKeyDown(KeyCode.R))
        {
            itemMB.gameObject.transform.SetParent(null);
            itemMB.gameObject.SetActive(true);
            itemMB.gameObject.transform.position = transform.position + (Vector3)Random.insideUnitCircle;
        }
        else
        {
            // Si se usó (Q), podrías destruirlo o simplemente dejarlo desactivado
            // Destroy(itemMB.gameObject); 
        }

        Inventory[index] = (IInventoryItem)defaultItem;
        actItem = Inventory[actItemIndex];
    }

    private void PopulateInventory()
    {
        while (Inventory.Count < maxItems)
        {
            Inventory.Add((IInventoryItem)defaultItem);
        }
    }
}