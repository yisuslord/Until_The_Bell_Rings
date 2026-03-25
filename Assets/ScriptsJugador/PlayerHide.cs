using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    public bool IsHidden { get; private set; }

    private IHideable currentHideSpot;

    public void Hide(IHideable hideSpot)
    {
        if (IsHidden) return;

        IsHidden = true;
        currentHideSpot = hideSpot;

        hideSpot.Hide(transform);

        Debug.Log("Jugador escondido");
    }

    public void Unhide()
    {
        if (!IsHidden) return;

        currentHideSpot.Unhide(transform);

        IsHidden = false;

        Debug.Log("Jugador salió del escondite");
    }
}