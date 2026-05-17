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
            // CAMBIO: Si la lista técnica no tiene este índice, significa que el slot está vacío
            if (i >= inventoryLogic.Inventory.Count)
            {
                itemIcons[i].enabled = false;
                itemIcons[i].sprite = null;
                continue; // 🔥 'continue' salta al siguiente número (i++), NO rompe el bucle
            }

            IInventoryItem item = inventoryLogic.Inventory[i];

            // Si hay un item real y no es el default
            if (item != null && item != (IInventoryItem)inventoryLogic.defaultItem)
            {
                // Intentamos convertir a BaseItem para leer el Sprite
                if (item is BaseItem baseItem && baseItem.InventoryIcon != null)
                {
                    itemIcons[i].sprite = baseItem.InventoryIcon; // Aquí se asigna la foto real
                    itemIcons[i].enabled = true; // Encendemos el portarretratos
                }
                else
                {
                    itemIcons[i].enabled = false;
                }
            }
            else
            {
                // Slot vacío dentro de la lista
                itemIcons[i].enabled = false;
                itemIcons[i].sprite = null;
            }
        }
    }
}