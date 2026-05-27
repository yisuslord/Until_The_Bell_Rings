using UnityEngine;

public class AltarZone : MonoBehaviour
{
    public bool IsPlayerInside { get; private set; }

    // Detecta de manera fisica mediante colisiones dinamicas en 2D el ingreso de objetos a la zona del altar.
    // Se conecta con el componente PlayerController del jugador.
    // Activa una propiedad booleana publica que sirve para indicar a otros sistemas si el jugador se encuentra fisicamente dentro del area sagrada del altar.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            IsPlayerInside = true;
        }
    }

    // Detecta de manera fisica cuando un objeto abandona la zona delimitada por el disparador.
    // Se conecta con el componente PlayerController del jugador.
    // Restablece la propiedad booleana de control a falso para indicar que el personaje ya no se encuentra en el area.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            IsPlayerInside = false;
        }
    }
}