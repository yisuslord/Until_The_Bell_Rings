using UnityEngine;


public interface IInteractable
{
    void Interact();
}
public interface ICollectible
{
    void Collect();
}

public interface IHideable
{
    void Hide(Transform player);
    void Unhide(Transform player);
}

public interface ICorruptible
{
    bool IsCorrupted { get; }
    void Corrupt();
    void Restore();
}

public interface IDamageable
{
    void TakeDamage(int amount);
}