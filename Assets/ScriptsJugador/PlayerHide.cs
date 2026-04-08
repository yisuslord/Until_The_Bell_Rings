using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public bool IsHidden { get; private set; }

    [SerializeField] private SpriteRenderer playerSprite;

    // Referencia al collider para apagar las colisiones
    private Collider2D playerCollider;
    private IHideable currentHideSpot;

    private void Start()
    {
        // Buscamos automáticamente el collider que tenga tu jugador (BoxCollider2D, CapsuleCollider2D, etc.)
        playerCollider = GetComponent<Collider2D>();
    }

    public void Hide(IHideable hideSpot)
    {
        if (IsHidden) return;

        IsHidden = true;
        currentHideSpot = hideSpot;

        // 1. Apagamos el sprite (te vuelves invisible)
        playerSprite.enabled = false;

        // 2. Apagamos las colisiones (los enemigos, raycasts y triggers te ignoran por completo)
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        hideSpot.Hide(transform);
        Debug.Log("Jugador escondido: Sprite y Colisiones desactivados");
    }

    public void Unhide()
    {
        if (!IsHidden) return;

        // 1. Volvemos a encender el sprite
        playerSprite.enabled = true;

        // 2. Reactivamos las colisiones
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