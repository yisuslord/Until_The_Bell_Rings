using UnityEngine;

public class DarkArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject manifestadoPrefab;
    [SerializeField] private float timeToSpawn = 5f;
    [SerializeField] private Transform spawnPoint;

    private float timer;
    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = true;
            Debug.Log("Jugador en zona oscura");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (playerInside)
        {
            timer += Time.deltaTime;
            if (timer >= timeToSpawn)
            {
                SpawnAndTarget();
                timer = 0;
            }
        }
        else
        {
            timer = 0;
        }
    }

    private void SpawnAndTarget()
    {
        GameObject enemy = Instantiate(manifestadoPrefab, spawnPoint.position, Quaternion.identity);

        if (enemy.TryGetComponent(out IStimulusReceiver receiver))
        {
            // ¡Adiós a FindGameObjectWithTag! Usamos nuestra referencia global súper rápida
            if (PlayerController.Instance != null)
            {
                receiver.OnStimulusReceived(PlayerController.Instance.transform.position, StimulusType.Darkness);
            }
        }
    }
}