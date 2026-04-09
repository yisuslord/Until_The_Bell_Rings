using UnityEngine;

public class ItemDummy : MonoBehaviour, IInventoryItem  // Hereda de IInventoryItem
{
    // Accion que hara el objeto al usarse con "Q"
    public void Action()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        Debug.Log("Dummy Action");
    }

    // Lo que hara el objeto al ser anadido al inventario
    public void Added()
    {
        gameObject.GetComponent<CollectibleItem>().enabled = false;
        Debug.Log("Dummy Added");
    }

    // Lo que hara el objeto al ser removido del inventario
    public void Removed()
    {
        gameObject.SetActive(true);
        gameObject.GetComponent<CollectibleItem>().enabled = true;
        Debug.Log("Dummy Removed");
    }

}
