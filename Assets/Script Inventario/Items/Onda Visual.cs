using UnityEngine;

public class Onda_Visual : MonoBehaviour, IVisualEffect
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void PlayEffect(Vector3 position)
    {
        transform.position = position;
        ps.Play();
    }
}