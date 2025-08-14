using UnityEngine;


    public class SecurityCamera : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField] float scanSpeed = 30f;     
        [SerializeField] float scanAngle = 45f;     
        [SerializeField] Rigidbody rb;              
        float baseYaw;
        float phaseDeg;                              

        [Header("Vision")]
        [SerializeField] float visionRange = 12f;
        [SerializeField] float visionHalfAngle = 35f;
        [SerializeField] float eyeHeight = 1.8f;
        [SerializeField] LayerMask obstacleMask = ~0;  
        [SerializeField] LayerMask playerMask = 0;     
        [SerializeField] string playerTag = "Player";

        [Header("Physics Detection")]
        [SerializeField] float proximityRadius = 2f;   
        [SerializeField] float sweepRadius = 0.35f;    

        [Header("Alerting")]
        [SerializeField] float alertCooldown = 1f;
        float lastAlertTime = -999f;

        Transform player;

        void Start()
        {

            //Find player

            player = GameObject.FindGameObjectWithTag(playerTag)?.transform;
            baseYaw = transform.eulerAngles.y;
            if (!rb) rb = GetComponent<Rigidbody>();
            if (rb)
            {

                

                // Only spin around Y

                rb.freezeRotation = false;
                rb.constraints = RigidbodyConstraints.FreezePositionX |
                                 RigidbodyConstraints.FreezePositionY |
                                 RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationZ;
            }
        }

        void FixedUpdate()
        {
            if (!rb) return;

            
            phaseDeg += scanSpeed * Time.fixedDeltaTime;
            float targetYaw = baseYaw + Mathf.Sin(phaseDeg * Mathf.Deg2Rad) * scanAngle;

            
            float currentYaw = rb.rotation.eulerAngles.y;
            float yawError = Mathf.DeltaAngle(currentYaw, targetYaw);

            
            float currentSpeedDeg = Vector3.Dot(rb.angularVelocity, Vector3.up) * Mathf.Rad2Deg;  

            float kp = 0.6f, kd = 0.1f;  

            float desiredAccelDeg = kp * yawError - kd * currentSpeedDeg;                         

            
            float torque = desiredAccelDeg * Mathf.Deg2Rad;                                       

            rb.AddTorque(Vector3.up * torque, ForceMode.Acceleration); //Forces

            rb.AddTorque(Vector3.up * torque, ForceMode.Acceleration);

        }

        void Update()
        {
            if (!player) return;

            
            Vector3 eye = transform.position + Vector3.up * eyeHeight;                            

            //Check if player is within vision range

            Vector3 toPlayer = player.position - eye;                                             
            float dist = toPlayer.magnitude;                                                      
            if (dist > visionRange) return;

            Vector3 dir = toPlayer.normalized;                                                    
            float ang = Vector3.Angle(transform.forward, dir);                                    
            if (ang > visionHalfAngle) return;

            

            if (Physics.OverlapSphere(eye, proximityRadius, playerMask, QueryTriggerInteraction.Ignore).Length > 0) //Overlap shapes

            if (Physics.OverlapSphere(eye, proximityRadius, playerMask, QueryTriggerInteraction.Ignore).Length > 0)

            {
                TryBroadcast(player.position, "[Camera] Proximity alert (OverlapSphere).");
                return;
            }

            

            if (Physics.SphereCast(eye, sweepRadius, dir, out RaycastHit hit, dist, playerMask, QueryTriggerInteraction.Ignore) //Raycasting and Spherecasting

                && hit.collider.CompareTag(playerTag))
            {
                TryBroadcast(player.position, "[Camera] Player via SphereCast.");
                return;
            }

            
            bool blocked = Physics.Raycast(eye, dir, dist, obstacleMask, QueryTriggerInteraction.Ignore); 
            if (!blocked)
                TryBroadcast(player.position, "[Camera] Player via Raycast.");
        }

        public void ForceAlert(Vector3 where) 
        {
            TryBroadcast(where, "[Camera] Forced alert.");
        }

        void TryBroadcast(Vector3 suspectPos, string msg)
        {
            if (Time.time < lastAlertTime + alertCooldown) return;
            lastAlertTime = Time.time;
            foreach (var g in FindObjectsOfType<SimpleGuard>())
                g.ExternalAlert(suspectPos);

            //Log for debugging

            Debug.Log(msg);
        }

        void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);

            Vector3 fwd = transform.forward;
            Quaternion L = Quaternion.AngleAxis(-visionHalfAngle, Vector3.up);
            Quaternion R = Quaternion.AngleAxis(visionHalfAngle, Vector3.up);
            Gizmos.DrawRay(transform.position, L * fwd * visionRange);
            Gizmos.DrawRay(transform.position, R * fwd * visionRange);

            
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(eye, proximityRadius);
            Gizmos.color = Color.magenta; Gizmos.DrawRay(eye, fwd * 1.5f);
        }
    }

