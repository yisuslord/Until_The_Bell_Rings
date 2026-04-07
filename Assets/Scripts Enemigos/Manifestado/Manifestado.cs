using UnityEngine;

public class Manifestado : EnemyBase 
{
    protected override void Awake()
    {
        base.Awake();
        // En cuanto spawnea, su único objetivo es el jugador
    }

    private void Update()
    {
        if (player != null)
        {
            MoveTo(player.position);
            CheckAttack();
        }
    }
}