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

    [Header("UI de Interacción")]
    private TextMeshProUGUI textoInteraccionUI;
    private IInventoryItem miItem;

    private void Awake()
    {
        miItem = GetComponent<IInventoryItem>();

        // 🔥 NUEVA BÚSQUEDA BLINDADA: Busca en TODA la escena, incluídos objetos desactivados
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break; // Lo encontramos, salimos del bucle
            }
        }

        // Verificación de seguridad por si acaso cambiaste el nombre en el Canvas
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[CollectibleItem] No se encontró el GameObject 'TextoInteraccion' (ni activo ni inactivo) para {gameObject.name}");
        }
    }

    private void Start()
    {
        // El primer frame se asegura de ocultarlo globalmente de forma segura si está asignado
        if (textoInteraccionUI != null && textoInteraccionUI.gameObject.activeSelf)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
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

                transform.SetParent(tempInventory.transform);
                transform.localPosition = Vector3.zero;
                gameObject.SetActive(false);

                if (AudioManager.Instance != null && clipRecoger != null)
                {
                    AudioManager.Instance.PlaySFX2D(clipRecoger, 1f);
                }

                Debug.Log($"<color=green>{gameObject.name} guardado y desactivado (Sin clones).</color>");

                playerInRange = false;
                tempInventory = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            tempInventory = other.GetComponent<PlayerInventory>();
            MostrarTexto();
        }
    }

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