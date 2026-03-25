using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private List<string> items = new List<string>();

    public void AddItem(string item)
    {
        items.Add(item);
        Debug.Log("Item recolectado: " + item);
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        Debug.Log("Usando item: " + items[index]);

        items.RemoveAt(index);
    }
}