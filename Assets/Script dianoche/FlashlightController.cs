using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject flashlightLightComponent; // El objeto que tiene la luz y el emisor de ruido
    private bool isOn = false;

    private void Awake()
    {
        // Nos aseguramos de que empiece apagada
        if (flashlightLightComponent != null) flashlightLightComponent.SetActive(false);
    }

    void Update()
    {
        // Solo permite usar la linterna si es de noche
        if (LevelManager.Instance.currentState == GameState.Night)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                isOn = !isOn;
                if (flashlightLightComponent != null) flashlightLightComponent.SetActive(isOn);
                Debug.Log(isOn ? "Linterna encendida" : "Linterna apagada");
            }
        }
        else if (isOn)
        {
            // Si se hace de día de golpe, la forzamos a apagarse
            isOn = false;
            if (flashlightLightComponent != null) flashlightLightComponent.SetActive(false);
        }
    }
}