using UnityEngine;


public class HideSpot : MonoBehaviour, IInteractable, IHideable
{
    [SerializeField] private Transform hidePoint;

    public void Interact()
    {
        // 🔥 Obtenemos al jugador desde el collider más cercano
        PlayerHide player = FindFirstObjectByType<PlayerHide>();

        if (player == null) return;

        if (!player.IsHidden)
        {
            player.Hide(this);
        }
        else
        {
            player.Unhide();
        }
    }

    public void Hide(Transform player)
    {
        player.position = hidePoint.position;

        Debug.Log("Se metió en la caja");
    }

    public void Unhide(Transform player)
    {
        Debug.Log("Salió de la caja");
    }
}