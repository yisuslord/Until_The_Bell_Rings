using UnityEngine;

public class Bateria : BaseItem
{
    [Header("Ajustes de Recarga")]
    [Tooltip("Cantidad de energia que se restaurara a la linterna.")]
    [SerializeField] private float restoreAmount = 40f;

    [SerializeField] private AudioClip clipRecarga;

    // Esta funcion la llama automaticamente el inventario cuando usamos la bateria
    public override void Use()
    {
        // Reproducimos el sonido de recarga en 2D a traves del AudioManager
        if (AudioManager.Instance != null && clipRecarga != null)
        {
            AudioManager.Instance.PlaySFX2D(clipRecarga, 1f);
        }

        // Buscamos la linterna que esta activa en la escena
        FlashlightController flashlight = Object.FindFirstObjectByType<FlashlightController>();

        if (flashlight != null)
        {
            // Si encontramos la linterna, le sumamos la energia de la bateria
            flashlight.RechargeBattery(restoreAmount);
            Debug.Log($"[Inventario] {itemName} consumida. Energia restaurada: {restoreAmount}");
        }
        else
        {
            // Si no hay ninguna linterna en el mapa, avisamos con una advertencia en la consola
            Debug.LogWarning("[Inventario] Objeto no consumido: No se encontro el FlashlightController en la escena.");
        }
    }
}