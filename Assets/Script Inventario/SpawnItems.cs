using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs de Objetos (Deben ser exactamente 3)")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Puntos de Spawn (Deben ser 9)")]
    [SerializeField] private Transform[] spawnPoints;

    // Listas internas para rastrear qué objetos están ACTIVOS en el suelo
    private List<GameObject>[] spawnedItemsByGroup;
    private List<Transform> availablePoints = new List<Transform>();

    private void Start()
    {
        spawnedItemsByGroup = new List<GameObject>[3];
        for (int i = 0; i < 3; i++)
        {
            spawnedItemsByGroup[i] = new List<GameObject>();
        }

        CheckAndRepopulate();
    }

    private void Update()
    {
        // 1. Limpiamos de las listas los objetos que ya NO están activos en el mundo
        CleanInactiveReferences();

        // 2. Si hay menos de 6 objetos en total en el suelo, spawneamos los que falten
        if (TotalSpawnedCount() < 6)
        {
            CheckAndRepopulate();
        }
    }

    private void CheckAndRepopulate()
    {
        UpdateAvailablePoints();

        if (availablePoints.Count == 0) return;

        // Guardamos cuántos hay de cada uno actualmente en el suelo
        int tipo0 = spawnedItemsByGroup[0].Count;
        int tipo1 = spawnedItemsByGroup[1].Count;
        int tipo2 = spawnedItemsByGroup[2].Count;

        // 🔥 CONTROL ESTRICTO: Solo spawnea si hay puntos libres Y el total es menor a 6
        while (availablePoints.Count > 0 && (tipo0 + tipo1 + tipo2) < 6)
        {
            int tipoASpawnear = -1;

            // Prioridad 1: Asegurar que haya mínimo 2 de cada uno
            if (tipo0 < 2) { tipoASpawnear = 0; tipo0++; }
            else if (tipo1 < 2) { tipoASpawnear = 1; tipo1++; }
            else if (tipo2 < 2) { tipoASpawnear = 2; tipo2++; }
            else
            {
                // Prioridad 2: Si ya hay 2 de cada uno pero falta para llegar a 6, elige al azar
                tipoASpawnear = Random.Range(0, itemPrefabs.Length);
                if (tipoASpawnear == 0) tipo0++;
                else if (tipoASpawnear == 1) tipo1++;
                else if (tipoASpawnear == 2) tipo2++;
            }

            if (tipoASpawnear != -1)
            {
                int randomPointIndex = Random.Range(0, availablePoints.Count);
                Transform targetPoint = availablePoints[randomPointIndex];

                GameObject newObject = Instantiate(itemPrefabs[tipoASpawnear], targetPoint.position, targetPoint.rotation);

                // Lo guardamos en su lista correspondiente
                spawnedItemsByGroup[tipoASpawnear].Add(newObject);

                availablePoints.RemoveAt(randomPointIndex);

                Debug.Log($"<color=cyan>[Spawner]</color> Spawneado objeto Tipo {tipoASpawnear} en {targetPoint.name}. Total en mapa: {tipo0 + tipo1 + tipo2}");
            }
            else
            {
                break;
            }
        }
    }

    private void UpdateAvailablePoints()
    {
        availablePoints.Clear();

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            Collider2D hit = Physics2D.OverlapCircle(point.position, 0.4f);

            // Un punto está libre si no hay colisionador, O si el objeto con el que choca está desactivado (en el inventario)
            if (hit == null || !hit.gameObject.activeInHierarchy)
            {
                availablePoints.Add(point);
            }
        }
    }

    /// <summary>
    /// 🔥 LA CLAVE: Borra de la lista los objetos destruidos (null) O los que se desactivaron (activeSelf == false)
    /// </summary>
    private void CleanInactiveReferences()
    {
        for (int i = 0; i < 3; i++)
        {
            // Quitamos de la lista si el objeto fue destruido o si fue desactivado por tu inventario
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