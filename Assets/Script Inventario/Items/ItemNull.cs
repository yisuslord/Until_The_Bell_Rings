using UnityEngine;

public class ItemNull : MonoBehaviour, IInventoryItem
{

    public void Action()
    {
        Debug.Log("Soy un Item nulo siendo usado, pero no hago nada");
    }

    public void Added()
    {
        Debug.Log("Soy un Item nulo siendo anadido al inventario");
    }

    public void Removed()
    {
        Debug.Log("Soy un Item nulo siendo removido del inventario");
    }

}
