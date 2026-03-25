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