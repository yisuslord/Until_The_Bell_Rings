using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Conexiones")]
    [SerializeField] private GameObject playerObject;
    private PlayerInventory inventoryLogic;

    [Header("UI References")]
    [SerializeField] private List<Image> selectionIndicators = new List<Image>();
    [SerializeField] private List<Image> itemIcons = new List<Image>();

    private int lastSelectedIndex = -1;

    void Start()
    {
        // 1. Buscar al player si no está asignado
        if (playerObject == null) playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            inventoryLogic = playerObject.GetComponent<PlayerInventory>();
        }

        RefreshUI();
    }

    void Update()
    {
        if (inventoryLogic == null) return;

        // 2. Si el índice cambió, actualizar marco dorado
        if (inventoryLogic.actItemIndex != lastSelectedIndex)
        {
            UpdateSelectionVisual(inventoryLogic.actItemIndex);
        }

        // 3. Actualizar iconos constantemente
        UpdateInventoryIcons();
    }

    private void RefreshUI()
    {
        if (inventoryLogic != null)
        {
            UpdateSelectionVisual(inventoryLogic.actItemIndex);
            UpdateInventoryIcons();
        }
    }

    private void UpdateSelectionVisual(int currentSelectedIndex)
    {
        if (selectionIndicators.Count < 5) return;

        // Apagar todos los marcos
        for (int i = 0; i < selectionIndicators.Count; i++)
        {
            selectionIndicators[i].gameObject.SetActive(false);
        }

        // Encender solo el actual
        if (currentSelectedIndex >= 0 && currentSelectedIndex < selectionIndicators.Count)
        {
            selectionIndicators[currentSelectedIndex].gameObject.SetActive(true);
        }

        lastSelectedIndex = currentSelectedIndex;
    }

    public void UpdateInventoryIcons()
    {
        if (itemIcons.Count < 5 || inventoryLogic == null) return;

        for (int i = 0; i < 5; i++)
        {
            // Si el inventario no tiene este índice inicializado, ocultamos
            if (i >= inventoryLogic.Inventory.Count)
            {
                itemIcons[i].sprite = null;
                SetIconAlpha(itemIcons[i], 0f); // Invisible
                continue;
            }

            IInventoryItem item = inventoryLogic.Inventory[i];

            // 🔥 COMPARACIÓN CLAVE: Si el ítem es null o es exactamente el defaultItem, el slot está VACÍO
            if (item == null || item == (IInventoryItem)inventoryLogic.defaultItem)
            {
                itemIcons[i].sprite = null;     // Borramos la imagen vieja del objeto usado
                SetIconAlpha(itemIcons[i], 0f); // Volvemos el slot 100% invisible
            }
            else
            {
                // Si hay un objeto real, intentamos leer su icono
                if (item.InventoryIcon != null)
                {
                    itemIcons[i].sprite = item.InventoryIcon; // Asignamos el dibujo
                    SetIconAlpha(itemIcons[i], 1f);          // Lo mostramos (100% opaco)
                }
                else
                {
                    // Si el objeto no tiene icono configurado, lo ocultamos para que no salga el cuadro blanco
                    itemIcons[i].sprite = null;
                    SetIconAlpha(itemIcons[i], 0f);
                }
            }
        }
    }

    // Función auxiliar (recuerda mantenerla al final de tu InventoryUI)
    private void SetIconAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
        img.enabled = true; // Lo dejamos encendido para evitar bugs de canvas de Unity
    }
}
    
