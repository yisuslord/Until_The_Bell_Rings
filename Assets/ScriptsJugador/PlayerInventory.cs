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

    void Awake()
    {
        // Al arrancar, llenamos la lista con el item vacio por defecto
        for (int i = 0; i < maxItems; i++)
        {
            Inventory.Add(defaultItem);
        }
    }

    void Update()
    {
        // 1. Controlamos el cambio de slot activo (Teclas 1-5 o botones)
        ManejarSeleccion();

        // 2. Usar Item (Tecla Q o boton asignado)
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Use"))
        {
            UsarItemActual();
        }

        // 3. Soltar Item (Tecla R o boton asignado)
        if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Drop"))
        {
            SoltarItemActual();
        }
    }

    private void ManejarSeleccion()
    {
        // Cambios directos usando los numeros del teclado
        if (Input.GetKeyDown(KeyCode.Alpha1)) actItemIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) actItemIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) actItemIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) actItemIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) actItemIndex = 4;

        // Cambios graduales (por ejemplo, usando los gatillos o flechas)
        if (Input.GetButtonDown("ObjL")) actItemIndex -= 1;
        if (Input.GetButtonDown("ObjR")) actItemIndex += 1;

        // Nos aseguramos de que el indice no se salga de los limites del inventario
        actItemIndex = Mathf.Clamp(actItemIndex, 0, maxItems - 1);
    }

    private void UsarItemActual()
    {
        IInventoryItem item = Inventory[actItemIndex];

        // Si hay un objeto real en el slot, activa su funcion de uso y lo quita del inventario
        if (item != null && item != (IInventoryItem)defaultItem)
        {
            item.Use();
            RemoverItemActual();
        }
    }

    private void SoltarItemActual()
    {
        IInventoryItem item = Inventory[actItemIndex];

        if (item != null && item != (IInventoryItem)defaultItem)
        {
            MonoBehaviour itemComponent = item as MonoBehaviour;

            if (itemComponent != null)
            {
                // 1. Desvinculamos el objeto del jugador para que vuelva a ser independiente
                itemComponent.transform.SetParent(null);

                // 2. Lo dejamos en el suelo cerca del jugador con una posicion un poco aleatoria
                Vector3 posicionSoltado = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
                itemComponent.transform.position = posicionSoltado;

                // 3. Volvemos a prender el objeto para que se vea y se pueda volver a recoger
                itemComponent.gameObject.SetActive(true);

                // 4. Si el objeto tiene fisicas, las frenamos para que no salga disparado
                if (itemComponent.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                Debug.Log($"[Inventario] {item.ItemName} devuelto al mundo real.");
            }

            // Vaciamos el slot donde estaba el objeto
            RemoverItemActual();
        }
    }

    public void RemoverItemActual()
    {
        // Reemplazamos el objeto por el elemento vacio por defecto
        Inventory[actItemIndex] = defaultItem;

        // Buscamos la UI y le avisamos de inmediato que actualice los iconos en pantalla
        InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.UpdateInventoryIcons();
        }
    }

    public void AddItem(IInventoryItem newItem)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            // Buscamos el primer hueco que este vacio o que tenga el item por defecto
            if (Inventory[i] == null || Inventory[i] == (IInventoryItem)defaultItem)
            {
                // Guardamos el nuevo item en ese espacio
                Inventory[i] = newItem;
                Debug.Log($"[Inventario] Objeto colocado en el slot libre: {i}");

                // Buscamos la UI para que dibuje el nuevo icono en su lugar correspondiente
                InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                if (ui != null) ui.UpdateInventoryIcons();

                return; // Cortamos la funcion para evitar que el objeto se duplique en otros slots
            }
        }
        Debug.Log("[Inventario] Inventario lleno, no hay huecos vacíos.");
    }
}