using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class Inventario : MonoBehaviour
{

    public ItemNull defaultItem; // Item por defecto en el inventario - Placeholder en el inventario

    public int maxItems = 5;    // Cantidad maxima de items en el inventario

    public int actItemIndex = 0;    // Index del Item activo inicializado en 0
    public IInventoryItem actItem;  // Item Activo

    public List<IInventoryItem> Inventory;  // Lista de Items en el Inventario

    void Start()
    {
        Inventory = new List<IInventoryItem>(); // Al iniciar se crea la lista vacía
        populateInventory();    // Se llena el inventario de items nulos
        actItem= Inventory[actItemIndex];
    }

    // Update is called once per frame
    void Update()
    {

        // Se asignan a los botones de numero su item correspondiente en el inventario
        for (int i = 0; i < maxItems; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                chooseItem(i);
            }
        }

        // Se asigna a "Q" la accion del objeto activo
        if (Input.GetKeyDown(KeyCode.Q))
        {
            actItem.Action();
            Debug.Log("Action " + actItemIndex);
        }

        // Se asigna a "R" el remover el objeto activo del inventario
        if (Input.GetKeyDown(KeyCode.R))
        {
            removeItem(actItem);
            actItem = Inventory[actItemIndex];
        }

    }

    public void chooseItem(int index)
    {
        if (index >= Inventory.Count) return;

        actItemIndex = index;
        actItem = Inventory[index];

        Debug.Log("Active Item is at index " + actItemIndex);

    }

    public bool addItem(IInventoryItem newItem)
    {
        if (newItem == actItem) return false;


        for (int index = 0; index < Inventory.Count; index++)
        {
            if (Inventory[index] == defaultItem)
            {


                // Se obtiene el mb del item nuevo para poder tener el GameObject
                MonoBehaviour itemMB = newItem as MonoBehaviour;
                // Se anade el item como hijo del player, por si se requiere su uso posterior
                itemMB.gameObject.transform.SetParent(gameObject.transform);

                // Se anade el item al inventario
                Inventory[index] = newItem;
                newItem.Added();

                // Se establece como el objeto activo
                actItemIndex = index;
                actItem = Inventory[index];

                Debug.Log("New Item in Inventory. Pos: " + index);



                return true;
            }
        }

        Debug.Log("Full Inventory");
        return false;
    }

    public void removeItem(IInventoryItem oldItem)
    {
        int index = Inventory.IndexOf(oldItem);

        // Se obtiene el mb del item nuevo para poder tener el GameObject
        MonoBehaviour itemMB = oldItem as MonoBehaviour;
        // Se elimina el objeto de los hijos del jugador
        itemMB.gameObject.transform.SetParent(null);

        oldItem.Removed();

        Inventory[index] = defaultItem;

        Debug.Log("Objeto Removido del inventario en " + index);
    }

    public void populateInventory()
    {
        while (Inventory.Count < maxItems)
        {
            Inventory.Add(defaultItem);
        }
    }

}
