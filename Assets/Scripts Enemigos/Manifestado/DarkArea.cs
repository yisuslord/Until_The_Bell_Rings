using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DarkArea : MonoBehaviour
{
    [SerializeField] private GameObject manifestadoPrefab;
    [SerializeField] private float timeToSpawn = 5f;
    [SerializeField] private Transform spawnPoint;

    private float timer;
    private bool playerInside;
    private GameObject currentEnemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null) playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other) => playerInside = false;

    private void Update()
    {
        // Si el enemigo ya está afuera cazando, no hacemos nada
        if (currentEnemy != null && currentEnemy.activeInHierarchy) return;

        if (playerInside)
        {
            timer += Time.deltaTime;
            if (timer >= timeToSpawn)
            {
                ActivateEnemy();
                timer = 0;
            }
        }
        else timer = 0;
    }

    private void ActivateEnemy()
    {
        if (currentEnemy == null)
        {
            currentEnemy = Instantiate(manifestadoPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            currentEnemy.SetActive(true);
            // Usamos Warp para posicionar al agente correctamente en el NavMesh
            if (currentEnemy.TryGetComponent(out NavMeshAgent agent))
            {
                agent.Warp(spawnPoint.position);
            }
        }
    }
}