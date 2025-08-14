using UnityEngine;

public class SimpleGuard : BaseEnemy
{
    [Header("Speeds")]
    [SerializeField] private float investigateSpeed = 2.8f;
    [SerializeField] private float chaseSpeed = 3.6f;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstacleMask = ~0;

    private Vector3 lastKnownPos;
    private float investigateTimer;

    protected override void Sense()
    {
        if (!player) return;

        
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        //Vision cone check

        if (dist <= visionRange)
        {
            Vector3 dir = toPlayer.normalized;
            float ang = Vector3.Angle(transform.forward, dir);
            if (ang <= visionHalfAngle)
            {
                bool blocked = Physics.Raycast(transform.position + Vector3.up * 1.6f, dir, dist, obstacleMask);
                if (!blocked)
                {
                    lastKnownPos = player.position;
                    AddDetection(fillRate);
                    if (State != EnemyState.Alert) ChangeState(EnemyState.Suspicious);
                    return;
                }
            }
        }


        //If player is in hearing range, increase detection

        if (dist <= hearingRange)
        {
            lastKnownPos = player.position;
            ChangeState(EnemyState.Suspicious);
        }
    }

    protected override void Investigate()
    {
        MoveTowards(lastKnownPos, investigateSpeed);

        
        if ((lastKnownPos - transform.position).sqrMagnitude < 0.25f)
        {
            investigateTimer += Time.deltaTime;
            if (investigateTimer >= 1.5f)
            {
                investigateTimer = 0f;
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    protected override void Engage()
    {
        if (!player) return;
        lastKnownPos = player.position;
        MoveTowards(lastKnownPos, chaseSpeed);
        
    }

    
    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 to = target - transform.position;
        if (to.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 360f * Time.deltaTime);
        transform.position += to.normalized * speed * Time.deltaTime;
    }

    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPos, 0.2f);
    }

    public void ExternalAlert(Vector3 suspectPosition)
    {
        lastKnownPos = suspectPosition;   
        ChangeState(EnemyState.Alert);   
    }
}
