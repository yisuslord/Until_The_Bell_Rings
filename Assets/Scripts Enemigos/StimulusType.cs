using UnityEngine;
public enum StimulusType { Noise, Light, Corruptible, Darkness }

public interface IStimulusReceiver
{
    void OnStimulusReceived(Vector2 position, StimulusType type);
}