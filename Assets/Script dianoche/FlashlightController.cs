using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject lightVisuals; // El objeto hijo que tiene la luz y el ruido
    private bool isOn = false;

    void Update()
    {
        // Solo funciona si es de noche
        if (LevelManager.Instance.currentState == GameState.Night)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOn = !isOn;
                lightVisuals.SetActive(isOn);
            }
        }
        else if (isOn)
        {
            // Si amanece, apagar automáticamente
            isOn = false;
            lightVisuals.SetActive(false);
        }
    }
}