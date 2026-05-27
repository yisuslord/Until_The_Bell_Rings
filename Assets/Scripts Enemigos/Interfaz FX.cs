using UnityEngine;

// Interfaz de abstracción para el manejo de componentes visuales. Facilita disparar efectos estéticos (como partículas de destrucción) de manera estandarizada.
public interface IVisualEffect
{
    void PlayEffect(Vector3 position);
}