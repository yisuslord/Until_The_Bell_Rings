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
            Debug.Log("Detectó: " + hit.name);

            IInteractable interactable = hit.GetComponent<IInteractable>();
            interactable?.Interact();
        }
        else
        {
            Debug.Log("No detectó nada");
        }
    }
}
