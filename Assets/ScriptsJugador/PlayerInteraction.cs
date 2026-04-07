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
            // Verificamos si el objeto es corruptible Y si está corrompido
            if (hit.TryGetComponent(out ICorruptible corruptible) && corruptible.IsCorrupted)
            {
                Debug.Log("No puedes interactuar con esto, está corrompido.");
                return; // Bloqueamos la interacción
            }

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
