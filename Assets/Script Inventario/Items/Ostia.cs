using UnityEngine;

public class Ostia : BaseItem
{
    [Header("Potion Settings")]
    [SerializeField] private int healAmount = 1;

    public override void Use()
    {
        // Buscamos al jugador por su componente de salud
        // Podrías usar un Singleton o buscar por Tag si prefieres
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Debug.Log($"<color=green>{itemName} usada. Se restauró {healAmount} de vida.</color>");

            // Aquí es donde el inventario debería encargarse de eliminar este objeto
            // ya que ha sido consumido.
        }
    }
}