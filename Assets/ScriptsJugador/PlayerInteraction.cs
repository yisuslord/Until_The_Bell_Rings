using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactLayer;

    private void Update()
    {
        // Si el jugador presiona la tecla E o el boton asignado a interactuar, intenta la accion
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Interact"))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // Creamos un circulo invisible alrededor del jugador para buscar objetos en la capa de interaccion
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);

        if (hit != null)
        {
            Debug.Log("<color=green>Detectó: " + hit.name + "</color>");

            // Si el objeto que tocamos tiene el componente o interfaz para interactuar, activamos su funcion
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