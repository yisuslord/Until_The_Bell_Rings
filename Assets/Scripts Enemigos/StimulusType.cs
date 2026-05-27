using UnityEngine;

// Enumerador global que tipifica todas las fuentes de interacción y alertas que los sistemas del mapa emiten hacia las inteligencias artificiales.
public enum StimulusType { Noise, Light, Corruptible, Darkness }

// Interfaz modular implementada por EnemyBase. Permite que cualquier objeto interactivo (velas, ruidos, linterna) envíe alertas a los enemigos de manera genérica.
public interface IStimulusReceiver
{
    void OnStimulusReceived(Vector2 position, StimulusType type);
}