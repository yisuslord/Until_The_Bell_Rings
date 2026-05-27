using TMPro;
using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable, IHideable
{
    [SerializeField] private Transform hidePoint;

    [Header("UI de Interaccion (Auto-asignada)")]
    private TextMeshProUGUI textoInteraccionUI;
    private bool playerInRange = false;

    private void Awake()
    {
        // Escaneo global en buffer para localizar el componente de texto sin importar su estado jerarquico activo/inactivo
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break;
            }
        }

        // Inicializacion de seguridad del buffer de texto
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[HideSpot] No se encontro el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        // Garantiza el estado apagado del componente visual al inicializar el escenario
        if (textoInteraccionUI != null && textoInteraccionUI.gameObject.activeSelf)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }

    // Procesa la solicitud de interaccion del jugador con el punto de escondite
    public void Interact()
    {
        PlayerHide player = FindFirstObjectByType<PlayerHide>();
        if (player == null) return;

        if (!player.IsHidden)
        {
            // Remocion del texto de interfaz previa transicion de estado para evitar persistencia visual flotante
            OcultarTexto();
            player.Hide(this);
        }
        else
        {
            player.Unhide();

            // Re-evaluacion del indicador visual si el jugador decide salir pero permanece en el area del trigger
            if (playerInRange)
            {
                MostrarTexto();
            }
        }
    }

    // Define la logica fisica del jugador al ingresar al contenedor de ocultamiento
    public void Hide(Transform player)
    {
        player.position = hidePoint.position;
        Debug.Log($"[Mecanica] Jugador ingresó al punto de ocultamiento en: {gameObject.name}");
    }

    // Define la logica fisica del jugador al egresar del contenedor de ocultamiento
    public void Unhide(Transform player)
    {
        Debug.Log($"[Mecanica] Jugador salio del punto de ocultamiento en: {gameObject.name}");
    }

    // --- DETECCION DEL JUGADOR PARA LA INTERFAZ ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Restriccion de la interfaz: Evita el despliegue del prompt si el jugador ya se encuentra en estado oculto
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
            OcultarTexto();
        }
    }

    private void MostrarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "Esconderse";
            textoInteraccionUI.gameObject.SetActive(true);
        }
    }

    private void OcultarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }
}