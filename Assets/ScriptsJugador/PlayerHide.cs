using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public bool IsHidden { get; private set; }

    [SerializeField] private SpriteRenderer playerSprite;

    private Collider2D playerCollider;
    private IHideable currentHideSpot;

    private void Start()
    {
        // Buscamos el componente de colisiones que tenga el jugador en su objeto
        playerCollider = GetComponent<Collider2D>();
    }

    // Activa el estado de escondido cuando interactuamos con un lugar valido
    public void Hide(IHideable hideSpot)
    {
        if (IsHidden) return;

        IsHidden = true;
        currentHideSpot = hideSpot;

        // Ocultamos el renderizador del sprite para que el personaje no se vea en pantalla
        playerSprite.enabled = false;

        // Apagamos sus colisiones para que los enemigos o ataques lo ignoren por completo
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        hideSpot.Hide(transform);
        Debug.Log("Jugador escondido: Sprite y Colisiones desactivados");
    }

    // Saca al jugador del escondite y restablece su estado normal en el juego
    public void Unhide()
    {
        if (!IsHidden) return;

        // Volvemos a hacer visible el sprite del personaje
        playerSprite.enabled = true;

        // Encendemos las colisiones para que el mundo vuelva a interactuar con el cuerpo del jugador
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        currentHideSpot.Unhide(transform);
        IsHidden = false;
        currentHideSpot = null;
        Debug.Log("Jugador salió: Sprite y Colisiones reactivados");
    }
}