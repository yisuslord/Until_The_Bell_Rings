using UnityEngine;
using System.Collections;

public class Manifestado : EnemyBase
{
    private Vector2 spawnPoint;
    private bool isRetreating = false;
    private float baseSpeed;
    //public void SetOrigin(DarkArea area, Vector2 pos) => spawnPoint = pos;
    protected override void Awake()
    {
        base.Awake();
        baseSpeed = agent.speed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        agent.speed = baseSpeed;
    }

    private void Update()
    {
        // Si el agente no está listo, no hacemos nada
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Persecución constante e implacable
        if (PlayerController.Instance != null)
        {
            agent.SetDestination(PlayerController.Instance.transform.position);
            CheckAttack();
        }
    }

    // SOBREESCRIBIMOS para que la onda NO le haga nada
    public override void GetRepelled(Vector2 shockwaveSource, float force)
    {
        // Dejamos esto vacío. La onda no tiene efecto sobre él.
        Debug.Log("<color=cyan>El Manifestado ignora la onda de choque.</color>");
    }
}