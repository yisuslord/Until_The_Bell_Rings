using UnityEngine;

public class CollectibleItem : MonoBehaviour, ICorruptible
{
    [Header("Ajustes de Item")]
    //[SerializeField] private GameObject itemLogicPrefab;
    public bool isCorrupted = false;

    private bool playerInRange = false; // Nueva variable para saber si el player está cerca
    private PlayerInventory tempInventory; // Referencia temporal al inventario

    // 🔥 NUEVA VARIABLE: Aquí arrastrarás el sonido de "recoger" en el Inspector
    [Header("Audio")]
    [SerializeField] private AudioClip clipRecoger;

    private void Update()
    {
        // Si el jugador está en el rango, no está corrompido y presiona E
        if (playerInRange && !isCorrupted && (Input.GetKeyDown(KeyCode.E)||Input.GetButtonDown("Interact")))
        {
            RecogerObjeto();
        }
    }

    public void RecogerObjeto()
    {
        if (tempInventory != null)
        {
            // 🔥 Buscamos la lógica (BaseItem) en este MISMÍSIMO objeto del suelo
            IInventoryItem item = GetComponent<IInventoryItem>();

            if (item != null)
            {
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