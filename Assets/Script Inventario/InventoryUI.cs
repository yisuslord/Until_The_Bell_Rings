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
        // Resolucion dinamica del objeto de jugador mediante tags en caso de no estar serializado en el inspector
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

        // Evaluacion de transicion de indices en la Hotbar para actualizar el marco de seleccion activa
        if (inventoryLogic.actItemIndex != lastSelectedIndex)
        {
            UpdateSelectionVisual(inventoryLogic.actItemIndex);
        }

        // Sincronizacion de las texturas de los iconos de los items con el estado del buffer lógico
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

        // Desactivacion en bloque de todos los indicadores del arreglo visual
        for (int i = 0; i < selectionIndicators.Count; i++)
        {
            selectionIndicators[i].gameObject.SetActive(false);
        }

        // Activacion exclusiva del indicador correspondiente al slot activo de la Hotbar
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
            // Ocultamiento preventivo si el indice consultado excede los limites asignados al inventario
            if (i >= inventoryLogic.Inventory.Count)
            {
                itemIcons[i].sprite = null;
                SetIconAlpha(itemIcons[i], 0f);
                continue;
            }

            IInventoryItem item = inventoryLogic.Inventory[i];

            // NOTA DE CONTROL: Si el elemento recuperado es nulo o equivalente a la instancia base por defecto,
            // el contenedor de interfaz se interpreta como vacio y se limpia el mapa de bits residual.
            if (item == null || item == (IInventoryItem)inventoryLogic.defaultItem)
            {
                itemIcons[i].sprite = null;
                SetIconAlpha(itemIcons[i], 0f);
            }
            else
            {
                // Asignacion del recurso grafico asignado al item polimorfico
                if (item.InventoryIcon != null)
                {
                    itemIcons[i].sprite = item.InventoryIcon;
                    SetIconAlpha(itemIcons[i], 1f);
                }
                else
                {
                    // Control de error visual: Evita el despliegue del recuadro blanco por defecto de Unity si falta el Sprite
                    itemIcons[i].sprite = null;
                    SetIconAlpha(itemIcons[i], 0f);
                }
            }
        }
    }

    // Modifica de forma segura el canal alpha del componente Image sin alterar los flags de activacion del Canvas Renderer
    private void SetIconAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
        img.enabled = true;
    }
}
