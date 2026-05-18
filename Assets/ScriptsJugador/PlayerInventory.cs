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
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Use"))
        {
            UsarItemActual();
        }

        // 3. Soltar Item (Tecla R)
        if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Drop"))
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

        if (Input.GetButtonDown("ObjL")) actItemIndex -= 1; Debug.Log(actItemIndex); ;
        if (Input.GetButtonDown("ObjR")) actItemIndex += 1; Debug.Log(actItemIndex); ;

        

        // Limitar el índice por seguridad
        actItemIndex = Mathf.Clamp(actItemIndex, 0, maxItems - 1);
    }

    private void UsarItemActual()
    {
        IInventoryItem item = Inventory[actItemIndex];

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
                // 1. Lo liberamos: deja de ser hijo del jugador
                itemComponent.transform.SetParent(null);

                // 2. Lo posicionamos cerca de los pies del jugador con un ligero desfase
                Vector3 posicionSoltado = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
                itemComponent.transform.position = posicionSoltado;

                // 3. ¡Lo reactivamos! Al encenderse, volverá a activar sus Triggers y su SpriteRenderer
                itemComponent.gameObject.SetActive(true);

                // 4. Si el objeto original usa físicas, las reseteamos al tocar el suelo
                if (itemComponent.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                Debug.Log($"<color=orange>{item.ItemName} devuelto al mundo real.</color>");
            }

            // Limpiamos el slot y refrescamos la UI
            RemoverItemActual();
        }
    }

    public void RemoverItemActual()
    {
        Inventory[actItemIndex] = defaultItem;

        // 🔥 NUEVO: Forzamos a la UI a enterarse INMEDIATAMENTE de que este slot se vació
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
            // 🔥 Buscamos el primer slot vacío (ya sea null o el defaultItem)
            if (Inventory[i] == null || Inventory[i] == (IInventoryItem)defaultItem)
            {
                Inventory[i] = newItem;
                Debug.Log($"<color=green>Objeto colocado en el slot libre: {i}</color>");

                // Forzamos a la UI a dibujar el nuevo ícono que acaba de entrar en ese hueco
                InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                if (ui != null) ui.UpdateInventoryIcons();

                return; // Cortamos el método para que no lo duplique en otros slots
            }
        }
        Debug.Log("<color=red>Inventario lleno, no hay huecos vacíos.</color>");
    }
}