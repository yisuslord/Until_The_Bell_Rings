using UnityEngine;

public class Bateria : BaseItem, ICorruptible
{
    [Header("Ajustes de Recarga")]
    [SerializeField] private float restoreAmount = 40f; // Cuánta energía restaura al usarse

    // Se ejecuta cuando el jugador presiona la Q teniendo este objeto seleccionado en la Hotbar
    public override void Use()
    {
        // Buscamos el controlador de la linterna en el jugador
        FlashlightController flashlight = Object.FindFirstObjectByType<FlashlightController>();

        if (flashlight != null)
        {
            flashlight.RechargeBattery(restoreAmount);
            Debug.Log($"<color=cyan>{itemName} usada con éxito.</color>");
        }
        else
        {
            Debug.LogWarning("No se encontró FlashlightController en la escena.");
        }
    }
}