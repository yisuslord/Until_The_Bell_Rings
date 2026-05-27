using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    [Header("Settings")]
    public BaseItem defaultItem;
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
        // Mapeo de entrada numerica para la seleccion de slots en la Hotbar (Teclas 1-5)
        for (int i = 0; i < maxItems; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChooseItem(i);
            }
        }

        // Procesamiento del consumo del item activo
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseActiveItem();
        }

        // Procesamiento del descarte del item activo hacia el escenario mundial
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveItem(actItem);
        }
    }

    private void UseActiveItem()
    {
        // Validacion de seguridad para evitar la ejecucion de logica sobre el item vacio por defecto
        if (actItem != null && actItem != (IInventoryItem)defaultItem)
        {
            // Invocacion del comportamiento polimorfico del item seleccionado
            actItem.Use();

            // Remocion automatica del buffer de datos tras el consumo logico
            RemoveItem(actItem);
            Debug.Log($"[Inventario] Item en slot {actItemIndex} procesado y removido.");
        }
    }

    public void ChooseItem(int index)
    {
        if (index >= Inventory.Count) return;
        actItemIndex = index;
        actItem = Inventory[index];
        Debug.Log("[Inventario] Cambio de slot activo a: " + actItemIndex);
    }

    // Gestiona la insercion de un nuevo elemento interactuable dentro del contenedor logico
    public bool addItem(IInventoryItem newItem)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            // Busqueda de slots disponibles que contengan la referencia del item nulo por defecto
            if (Inventory[i] == (IInventoryItem)defaultItem)
            {
                MonoBehaviour itemMB = newItem as MonoBehaviour;

                // Transferencia de jerarquia al transform del inventario para su preservacion centralizada
                itemMB.gameObject.transform.SetParent(transform);
                itemMB.gameObject.SetActive(false);

                Inventory[i] = newItem;
                ChooseItem(i);
                return true;
            }
        }
        Debug.Log("[Inventario] Registro rechazado: Capacidad maxima alcanzada.");
        return false;
    }

    // Maneja la remocion de elementos, distinguiendo entre descarte fisico y vaciado por consumo
    public void RemoveItem(IInventoryItem oldItem)
    {
        if (oldItem == (IInventoryItem)defaultItem) return;

        int index = Inventory.IndexOf(oldItem);
        if (index == -1) return;

        MonoBehaviour itemMB = oldItem as MonoBehaviour;

        // Evaluacion del origen de la remocion: Si proviene de una entrada de descarte, se devuelve al escenario fisico
        if (Input.GetKeyDown(KeyCode.R))
        {
            itemMB.gameObject.transform.SetParent(null);
            itemMB.gameObject.SetActive(true);
            itemMB.gameObject.transform.position = transform.position + (Vector3)Random.insideUnitCircle;
        }
        else
        {
            // NOTA DE DESARROLLO: Si la remocion proviene del metodo Use(), la liberacion de memoria 
            // o desactivacion extendida del clon es delegada a la clase especifica del item derivado.
        }

        // Restauracion del slot al elemento por defecto del sistema
        Inventory[index] = (IInventoryItem)defaultItem;
        actItem = Inventory[actItemIndex];
    }

    // Rellena la estructura de datos interna con la instancia por defecto para evitar punteros nulos
    private void PopulateInventory()
    {
        while (Inventory.Count < maxItems)
        {
            Inventory.Add((IInventoryItem)defaultItem);
        }
    }
}