using UnityEngine;


public class Candle : MonoBehaviour, IInteractable
{
    private bool isLit = false;

    public void Interact()
    {
        if (!isLit)
        {
            LightCandle();
        }
    }

    private void LightCandle()
    {
        isLit = true;

        Debug.Log("Encendiendo vela");

    }
}