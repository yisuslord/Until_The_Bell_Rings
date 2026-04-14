using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject lightVisuals;
    public bool IsOn { get; private set; } // 🔥 Nueva variable pública

    void Update()
    {
        if (LevelManager.Instance.currentState == GameState.Night)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                IsOn = !IsOn; // Cambiamos el estado
                lightVisuals.SetActive(IsOn);
            }
        }
        else if (IsOn)
        {
            IsOn = false;
            lightVisuals.SetActive(false);
        }
    }
}