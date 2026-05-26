using TMPro;
using UnityEngine;
//using static UnityEditor.Progress;

public class CollectibleItem : MonoBehaviour
{
    [Header("Ajustes de Item")]
    //[SerializeField] private GameObject itemLogicPrefab;
    public bool isCorrupted = false;

    private bool playerInRange = false; // Nueva variable para saber si el player está cerca
    private PlayerInventory tempInventory; // Referencia temporal al inventario

    // 🔥 NUEVA VARIABLE: Aquí arrastrarás el sonido de "recoger" en el Inspector
    [Header("Audio")]
    [SerializeField] private AudioClip clipRecoger;

    [Header("UI de Interacción")]
    private TextMeshProUGUI textoInteraccionUI; // 🔥 Arrastra el texto aquí
    private IInventoryItem miItem;

    private void Awake()
    {
        miItem = GetComponent<IInventoryItem>();

        // 🔥 ¡LA MAGIA DE LA BÚSQUEDA AUTOMÁTICA!
        // Buscamos en toda la escena un objeto que tenga el componente TextMeshProUGUI
        // Nota: Si tienes varios textos TMP en tu Canvas, es mejor buscarlo por el nombre exacto de su GameObject.
        GameObject objetoTexto = GameObject.Find("TextoInteraccion");
        

        if (objetoTexto != null)
        {
            textoInteraccionUI = objetoTexto.GetComponent<TextMeshProUGUI>();
            textoInteraccionUI.text = "";

        }
        else
        {
            Debug.LogWarning($"[CollectibleItem] No se encontró el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        // 🔥 EL CANDADO: Si la variable es null, ni siquiera intentamos revisar el gameObject
        if (textoInteraccionUI != null && textoInteraccionUI.gameObject.activeSelf)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Si el jugador está en el rango, no está corrompido y presiona E
        if (playerInRange && !isCorrupted && (Input.GetKeyDown(KeyCode.E)||Input.GetButtonDown("Interact")))
        {
            RecogerObjeto();
        }
        else { 
        }
    }

    public void RecogerObjeto()
    {
        if (tempInventory != null)
        {
            // 🔥 Buscamos la lógica (BaseItem) en este MISMÍSIMO objeto del suelo
            IInventoryItem item = GetComponent<IInventoryItem>();

            if (item != null && miItem != null)
            {
                // Ocultamos el texto inmediatamente al recogerlo
                OcultarTexto();
                // Lo añadimos al inventario
                tempInventory.AddItem(item);

                // 🔥 En lugar de clonar, transformamos a este mismo objeto en hijo del jugador
                transform.SetParent(tempInventory.transform);

                // Lo posicionamos en el centro del jugador (opcional, por orden)
                transform.localPosition = Vector3.zero;

                // Lo desactivamos por completo del mundo real (físicas, render, triggers, etc.)
                gameObject.SetActive(false);

                // Reproducir Audio
                if (AudioManager.Instance != null && clipRecoger != null)
                {
                    AudioManager.Instance.PlaySFX2D(clipRecoger, 1f);
                }

                Debug.Log($"<color=green>{gameObject.name} guardado y desactivado (Sin clones).</color>");

                // 🔥 IMPORTANTE: Limpiamos la referencia temporal para evitar bugs al desaparecer
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
            // 🔥 ¡JUGADOR EN RANGO!: Mostramos las instrucciones dinámicas
            MostrarTexto();

            // Opcional: Podrías activar aquí un mensaje de "Presiona E para recoger"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            tempInventory = null;
            // 🔥 ¡JUGADOR SE ALEJÓ!: Ocultamos las instrucciones
            OcultarTexto();
        }
    }

    private void MostrarTexto()
    {
        if (textoInteraccionUI != null && miItem != null)
        {
            // Personaliza el texto con el nombre real del objeto (ej: "[E] Recoger Batería")
            textoInteraccionUI.text = $"{miItem.ItemName}";
            textoInteraccionUI.gameObject.SetActive(true); // Lo encendemos
        }
    }

    /*private void MotrarTextoCorrompido()
    {
        if (textoInteraccionUI !=null && isCorrupted)
        {
            textoInteraccionUI.text = $"La {miItem.ItemName} ha sido corrompida";
            textoInteraccionUI.gameObject.SetActive(true);
        }
    }*/

    private void OcultarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false); // Lo apagamos
        }
    }
    /*public void Corrupt()
    {

        if (isCorrupted) return; // No corromper lo ya corrompido

        isCorrupted = true;
        Debug.Log($"<color=purple>El objeto {gameObject.name} ha sido corrompido y no puede recogerse.</color>");

        if (TryGetComponent(out SpriteRenderer sr))
            sr.color = Color.magenta;

        // Opcional: Emitir un sonido o partículas de corrupción aquí
    }*/
}