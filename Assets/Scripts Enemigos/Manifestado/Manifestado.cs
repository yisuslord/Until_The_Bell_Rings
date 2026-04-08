using UnityEngine;

public class Manifestado : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        currentState = State.Chasing;
    }

    private void Update()
    {

        if (PlayerController.Instance != null)
        {
            Transform target = PlayerController.Instance.transform;

       
            agent.stoppingDistance = attackDistance;

            MoveTo(target.position);
            CheckAttack();
        }
    }
}