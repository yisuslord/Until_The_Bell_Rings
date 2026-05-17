using UnityEngine;

public interface IInventoryItem
{
    string ItemName { get; }
    Sprite InventoryIcon { get; } // 🔥 AÑADE ESTO
    void Use();
}