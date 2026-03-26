using UnityEngine;
public enum StimulusType { Noise, Light }

public interface IStimulusReceiver
{
    void OnStimulusReceived(Vector2 position, StimulusType type);
}