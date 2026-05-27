using UnityEngine;

public class Onda_Visual : MonoBehaviour, IVisualEffect
{
    private ParticleSystem ps;

    private void Awake()
    {
        // Guardamos el componente de particulas que tiene este objeto
        ps = GetComponent<ParticleSystem>();
    }

    // Esta funcion arranca el efecto visual en la posicion exacta que le indiquemos
    public void PlayEffect(Vector3 position)
    {
        // Movemos el objeto a la posicion donde debe explotar la onda
        transform.position = position;

        if (ps != null)
        {
            // Frenamos cualquier particula que haya quedado suelta del uso anterior y limpiamos la pantalla.
            // Esto evita que el efecto se vea cortado o bugeado si activamos el objeto varias veces seguidas.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear();

            // Encendemos el sistema de particulas desde cero
            ps.Play();
        }
    }
}