using UnityEngine;

public interface IInventoryItem
{
    string ItemName { get; }
    Sprite InventoryIcon { get; } 
    void Use();
}