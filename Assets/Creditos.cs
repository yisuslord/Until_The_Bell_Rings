using UnityEngine;
using UnityEngine.SceneManagement;

public class Creditos : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 80f; // Píxeles por segundo
    [SerializeField] private float posicionFinalY = 2000f; // El punto en Y donde se detiene

    private RectTransform rectTransform;
    private bool animacionActiva = true;

    void Start()
    {
        // Obtenemos el RectTransform del contenedor de textos
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!animacionActiva) return;

        // Movemos de forma fluida hacia arriba multiplicando por Time.deltaTime
        rectTransform.anchoredPosition += Vector2.up * velocidad * Time.deltaTime;

        // Si sobrepasa la posición final, detenemos la animación
        if (rectTransform.anchoredPosition.y >= posicionFinalY)
        {
            FinalizarCreditos();
        }
    }

    void FinalizarCreditos()
    {
        animacionActiva = false;
        Debug.Log("Los créditos han terminado.");

        SceneManager.LoadScene("MainMenu");

        // Aquí puedes poner lógica extra más adelante, como volver al menú:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }
}