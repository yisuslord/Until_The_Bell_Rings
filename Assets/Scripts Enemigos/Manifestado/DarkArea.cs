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
        if (other.CompareTag("Player")) playerInside = true;
        Debug.Log("Jugador en zon oscura");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    private void Update()
    {
        if (playerInside == true)
        {
            timer += Time.deltaTime;
            if (timer >= timeToSpawn)
            {
                SpawnAndTarget();
                timer = 0;
            }
        }
        else timer = 0;
    }

    private void SpawnAndTarget()
    {
        GameObject enemy = Instantiate(manifestadoPrefab, spawnPoint.position, Quaternion.identity);

        // REGLA DE ORO: En cuanto nace, le enviamos el estímulo "Darkness" 
        // con la posición actual del jugador para que sepa a dónde ir.
        if (enemy.TryGetComponent(out IStimulusReceiver receiver))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            receiver.OnStimulusReceived(player.transform.position, StimulusType.Darkness);
        }
    }

    
}
