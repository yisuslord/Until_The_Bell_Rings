using TMPro;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Ajustes de Item")]
    public bool isCorrupted = false;

    private bool playerInRange = false;
    private PlayerInventory tempInventory;

    [Header("Audio")]
    [SerializeField] private AudioClip clipRecoger;

    [Header("UI de Interaccion")]
    private TextMeshProUGUI textoInteraccionUI;
    private IInventoryItem miItem;

    private void Awake()
    {
        miItem = GetComponent<IInventoryItem>();

        // Buscamos el texto de interaccion por toda la escena, este prendido o apagado
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break;
            }
        }

        // Limpiamos el texto al arrancar por seguridad
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[CollectibleItem] No se encontro el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        // Nos aseguramos de que el texto de la interfaz empiece apagado
        if (textoInteraccionUI != null && textoInteraccionUI.gameObject.activeSelf)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Si el jugador esta cerca, el objeto no esta corrompido y presiona el boton, lo recoge
        if (playerInRange && !isCorrupted && (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Interact")))
        {
            RecogerObjeto();
        }
    }

    public void RecogerObjeto()
    {
        if (tempInventory != null)
        {
            IInventoryItem item = GetComponent<IInventoryItem>();

            if (item != null && miItem != null)
            {
                OcultarTexto();
                tempInventory.AddItem(item);

                // Metemos el objeto dentro del inventario, lo acomodamos y lo desactivamos del mundo
                transform.SetParent(tempInventory.transform);
                transform.localPosition = Vector3.zero;
                gameObject.SetActive(false);

                // Reproducimos el sonido de recoger objeto
                if (AudioManager.Instance != null && clipRecoger != null)
                {
                    AudioManager.Instance.PlaySFX2D(clipRecoger, 1f);
                }

                Debug.Log($"<color=green>[Inventario] {gameObject.name} guardado y desactivado exitosamente.</color>");

                // Limpiamos las variables locales porque el objeto ya se guardo
                playerInRange = false;
                tempInventory = null;
            }
        }
    }

    // Cuando el jugador se acerca al objeto
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            tempInventory = other.GetComponent<PlayerInventory>();
            MostrarTexto();
        }
    }

    // Cuando el jugador se aleja del objeto
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            tempInventory = null;
            OcultarTexto();
        }
    }

    private void MostrarTexto()
    {
        if (textoInteraccionUI != null && miItem != null)
        {
            // Ponemos el nombre del objeto en el texto y lo encendemos
            textoInteraccionUI.text = $"{miItem.ItemName}";
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