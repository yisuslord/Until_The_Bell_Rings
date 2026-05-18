using UnityEngine;

public class Bateria : BaseItem, ICorruptible
{
    [Header("Ajustes de Recarga")]
    [SerializeField] private float restoreAmount = 40f; // Cuánta energía restaura al usarse
    [SerializeField] private AudioClip clipRecarga;

    // Se ejecuta cuando el jugador presiona la Q teniendo este objeto seleccionado en la Hotbar
    public override void Use()
    {
        /// 🔥 LLAMADA AL AUDIO MANAGER ANTES DE DESTRUIR EL OBJETO
        if (AudioManager.Instance != null && clipRecarga != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipRecarga, 1f);
        }
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