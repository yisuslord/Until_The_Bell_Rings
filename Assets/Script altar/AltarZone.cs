using UnityEngine;

public class AltarZone : MonoBehaviour
{
    public bool IsPlayerInside { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            IsPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            IsPlayerInside = false;
        }
    }
}