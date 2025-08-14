using UnityEngine;

public enum EnemyState { Patrol, Suspicious, Alert }


public abstract class BaseEnemy : MonoBehaviour 
{
    
    [Header("Patrol")]
    //Waypoint list for patrol routs
    [SerializeField] protected Transform[] waypoints; //Protected variables and SerializeField

    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float turnSpeed = 240f;

    [Header("Sensing")]
    [SerializeField] protected float visionRange = 10f;
    [SerializeField] protected float visionHalfAngle = 35f;
    [SerializeField] protected float hearingRange = 5f;

    [Header("Detection")]

    //Detection meter rates

    [SerializeField] protected float fillRate = 60f;  
    [SerializeField] protected float decayRate = 30f; 

    [Header("Debug")]
    [SerializeField] protected bool drawGizmos = true;

    
    protected EnemyState state = EnemyState.Patrol;
    protected float detection = 0f; 
    protected Transform player;
    protected int wpIndex = 0;

    
    public EnemyState State => state;
    public float Detection01 => Mathf.Clamp01(detection / 100f);


    protected virtual void Awake() //Virtual functions and protected functions

    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (!p) Debug.LogWarning($"{name}: No 'Player' found.");
        player = p ? p.transform : null;
    }

    protected virtual void Update()
    {
        if (!player) return; 


        Sense(); //Attempt to detect player

        Sense(); 


        if (state == EnemyState.Patrol) Patrol();
        else if (state == EnemyState.Suspicious) Investigate();
        else Engage();

        //Reduce detection over time if not fully alerted



        if (state != EnemyState.Alert) 
            detection = Mathf.Max(0f, detection - decayRate * Time.deltaTime);
    }

    
    protected virtual void Sense() { }

    protected virtual void Patrol() //Patrol logic

    {
        if (waypoints == null || waypoints.Length == 0) return;

        

        Vector3 to = waypoints[wpIndex].position - transform.position; //Subtraction
        if (to.magnitude < 0.2f) { wpIndex = (wpIndex + 1) % waypoints.Length; return; } //Magnitude
        
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);

        transform.position += to.normalized * moveSpeed * Time.deltaTime; //Normalisation of vectors

        transform.position += to.normalized * moveSpeed * Time.deltaTime;

    }
    protected virtual void Investigate() { }
    protected virtual void Engage() { }

    
    protected void ChangeState(EnemyState next)
    {
        if (state == next) return;
        state = next;
        Debug.Log($"{name} -> {state}");
    }

    protected void AddDetection(float perSecond)
    {
        detection = Mathf.Clamp(detection + perSecond * Time.deltaTime, 0f, 100f);
        if (detection >= 100f && state != EnemyState.Alert) ChangeState(EnemyState.Alert);
    }

    
    protected virtual void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, hearingRange);

        Vector3 fwd = transform.forward;

        Quaternion L = Quaternion.AngleAxis(-visionHalfAngle, Vector3.up); //Angles


        Quaternion R = Quaternion.AngleAxis(visionHalfAngle, Vector3.up);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, L * fwd * visionRange);
        Gizmos.DrawRay(transform.position, R * fwd * visionRange);
    }
}
