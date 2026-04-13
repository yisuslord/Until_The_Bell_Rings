public interface IInventoryItem
{
    string ItemName { get; }
    void Use(); // La acción principal (Curar, Escudo, Onda)
}