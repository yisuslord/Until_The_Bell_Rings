using UnityEngine;

public class CollectibleItem : MonoBehaviour, ICorruptible
{
    [Header("Ajustes de Item")]
    [SerializeField] private GameObject itemLogicPrefab;
    public bool isCorrupted = false;

    private bool playerInRange = false; // Nueva variable para saber si el player está cerca
    private PlayerInventory tempInventory; // Referencia temporal al inventario

    private void Update()
    {
        // Si el jugador está en el rango, no está corrompido y presiona E
        if (playerInRange && !isCorrupted && Input.GetKeyDown(KeyCode.E))
        {
            RecogerObjeto();
        }
    }

    private void RecogerObjeto()
    {
        if (tempInventory != null)
        {
            GameObject logicObj = Instantiate(itemLogicPrefab);
            IInventoryItem item = logicObj.GetComponent<IInventoryItem>();

            if (item != null)
            {
                tempInventory.AddItem(item);

                logicObj.transform.SetParent(tempInventory.transform);
                logicObj.SetActive(false);

                Debug.Log("<color=green>Item recogido con E.</color>");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            tempInventory = other.GetComponent<PlayerInventory>();

            // Opcional: Podrías activar aquí un mensaje de "Presiona E para recoger"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            tempInventory = null;
        }
    }

    public void Corrupt()
    {
        if (isCorrupted) return; // No corromper lo ya corrompido

        isCorrupted = true;
        Debug.Log($"<color=purple>El objeto {gameObject.name} ha sido corrompido y no puede recogerse.</color>");

        if (TryGetComponent(out SpriteRenderer sr))
            sr.color = Color.magenta;

        // Opcional: Emitir un sonido o partículas de corrupción aquí
    }
}