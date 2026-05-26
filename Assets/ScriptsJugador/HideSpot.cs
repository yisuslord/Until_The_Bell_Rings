using TMPro;
using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable, IHideable
{
    [SerializeField] private Transform hidePoint;

    [Header("UI de Interacción (Auto-asignada)")]
    private TextMeshProUGUI textoInteraccionUI;
    private bool playerInRange = false;

    private void Awake()
    {
        // 🔥 LA BÚSQUEDA BLINDADA: Escanea la escena buscando el objeto "TextoInteraccion" 
        // sin importar si está encendido o apagado por otros ítems.
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break; // Lo encontramos, salimos del bucle
            }
        }

        // Limpieza inicial de seguridad
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[HideSpot] No se encontró el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        // Nos aseguramos de que empiece oculto de forma segura al arrancar el nivel
        if (textoInteraccionUI != null && textoInteraccionUI.gameObject.activeSelf)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        PlayerHide player = FindFirstObjectByType<PlayerHide>();
        if (player == null) return;

        if (!player.IsHidden)
        {
            // 🔥 Al esconderse, ocultamos el texto inmediatamente para que no se quede flotando dentro del escondite
            OcultarTexto();
            player.Hide(this);
        }
        else
        {
            player.Unhide();
            // Si sale y sigue dentro del trigger, volvemos a mostrar el texto por si quiere volver a entrar
            if (playerInRange)
            {
                MostrarTexto();
            }
        }
    }

    public void Hide(Transform player)
    {
        player.position = hidePoint.position;
        Debug.Log("Se metió en la caja");
    }

    public void Unhide(Transform player)
    {
        Debug.Log("Salió de la caja");
    }

    // --- DETECCIÓN DEL JUGADOR PARA LA INTERFAZ ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // 🔥 Solo mostramos el texto si el jugador NO está escondido ya adentro
            PlayerHide player = other.GetComponent<PlayerHide>();
            if (player != null && !player.IsHidden)
            {
                MostrarTexto();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            OcultarTexto(); // Quita el mensaje "Esconderse" de la pantalla al alejarse
        }
    }

    private void MostrarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "Esconderse";
            textoInteraccionUI.gameObject.SetActive(true); // Encendemos la UI global
        }
    }

    private void OcultarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false); // Apagamos la UI global
        }
    }
}