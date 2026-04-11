using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactLayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);

        if (hit != null)
        {
            Debug.Log("<color=green>Detectó: " + hit.name + "</color>");

            // Quitamos el bloqueador de corrupción. Ahora directamente intentamos interactuar.
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
        else
        {
            Debug.Log("No detectó nada");
        }
    }
}