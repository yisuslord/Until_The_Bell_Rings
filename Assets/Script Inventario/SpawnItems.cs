using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs de Objetos (Deben ser exactamente 3)")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Puntos de Spawn (Deben ser 9)")]
    [SerializeField] private Transform[] spawnPoints;

    // Listas para saber qué objetos específicos están tirados en el suelo y qué puntos están libres
    private List<GameObject>[] spawnedItemsByGroup;
    private List<Transform> availablePoints = new List<Transform>();

    private void Start()
    {
        // Creamos las tres listas (una para cada tipo de objeto) antes de empezar a spawnear
        spawnedItemsByGroup = new List<GameObject>[3];
        for (int i = 0; i < 3; i++)
        {
            spawnedItemsByGroup[i] = new List<GameObject>();
        }

        CheckAndRepopulate();
    }

    private void Update()
    {
        // Primero borramos de la lista los objetos que el jugador ya recogió o que se destruyeron
        CleanInactiveReferences();

        // Si quedan menos de 6 objetos en total en el suelo, rellenamos el mapa
        if (TotalSpawnedCount() < 6)
        {
            CheckAndRepopulate();
        }
    }

    // Revisa qué lugares quedan libres y crea los objetos que hagan falta
    private void CheckAndRepopulate()
    {
        UpdateAvailablePoints();

        if (availablePoints.Count == 0) return;

        // Guardamos cuántos objetos hay de cada tipo en este momento
        int tipo0 = spawnedItemsByGroup[0].Count;
        int tipo1 = spawnedItemsByGroup[1].Count;
        int tipo2 = spawnedItemsByGroup[2].Count;

        // Spawneamos solo si quedan lugares libres y si no nos pasamos del límite de 6 objetos en el suelo
        while (availablePoints.Count > 0 && (tipo0 + tipo1 + tipo2) < 6)
        {
            int tipoASpawnear = -1;

            // Regla 1: Nos aseguramos de que siempre haya por lo menos 2 objetos de cada tipo en el mapa
            if (tipo0 < 2) { tipoASpawnear = 0; tipo0++; }
            else if (tipo1 < 2) { tipoASpawnear = 1; tipo1++; }
            else if (tipo2 < 2) { tipoASpawnear = 2; tipo2++; }
            else
            {
                // Regla 2: Si ya cumplimos el mínimo de 2 de cada uno, el resto se elige al azar
                tipoASpawnear = Random.Range(0, itemPrefabs.Length);
                if (tipoASpawnear == 0) tipo0++;
                else if (tipoASpawnear == 1) tipo1++;
                else if (tipoASpawnear == 2) tipo2++;
            }

            if (tipoASpawnear != -1)
            {
                // Elegimos un punto libre al azar y creamos el objeto ahí
                int randomPointIndex = Random.Range(0, availablePoints.Count);
                Transform targetPoint = availablePoints[randomPointIndex];

                GameObject newObject = Instantiate(itemPrefabs[tipoASpawnear], targetPoint.position, targetPoint.rotation);

                // Guardamos el nuevo objeto en la lista que le toca
                spawnedItemsByGroup[tipoASpawnear].Add(newObject);

                // Sacamos este punto de la lista para que no se spawnee otro objeto encima en el mismo frame
                availablePoints.RemoveAt(randomPointIndex);

                Debug.Log($"[Spawner] Creado objeto Tipo {tipoASpawnear} en {targetPoint.name}. Total en mapa: {tipo0 + tipo1 + tipo2}");
            }
            else
            {
                break;
            }
        }
    }

    // Revisa los puntos de spawn con un círculo de física para ver cuáles están vacíos
    private void UpdateAvailablePoints()
    {
        availablePoints.Clear();

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            Collider2D hit = Physics2D.OverlapCircle(point.position, 0.4f);

            // Un punto está libre si no hay nada encima, o si el objeto que toca se apagó (porque ya está en el inventario)
            if (hit == null || !hit.gameObject.activeInHierarchy)
            {
                availablePoints.Add(point);
            }
        }
    }

    // Limpia las listas quitando los objetos que ya no sirven o cambiaron de estado
    private void CleanInactiveReferences()
    {
        for (int i = 0; i < 3; i++)
        {
            // Borramos de la lista si el objeto se destruyó, si se desactivó o si ahora es hijo de un inventario
            spawnedItemsByGroup[i].RemoveAll(item => item == null || !item.activeInHierarchy || item.transform.parent != null);
        }
    }

    private int TotalSpawnedCount()
    {
        return spawnedItemsByGroup[0].Count + spawnedItemsByGroup[1].Count + spawnedItemsByGroup[2].Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.green;
        foreach (var point in spawnPoints)
        {
            if (point != null) Gizmos.DrawWireSphere(point.position, 0.4f);
        }
    }
}