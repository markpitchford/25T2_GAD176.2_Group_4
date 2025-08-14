using UnityEngine;

namespace SimpleAI.Minimal
{
    /// <summary>Base: physics movement, vision raycast, hearing overlap.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Perception")]
        [SerializeField, Range(1f, 360f)] protected float fov = 90f;
        [SerializeField] protected float visionRange = 10f;
        [SerializeField] protected float hearingRange = 8f;
        [SerializeField] protected LayerMask visionBlockers = ~0;

        [Header("Movement")]
        [SerializeField] protected float maxSpeed = 3f;
        [SerializeField] protected float acceleration = 10f;
        [SerializeField] protected float turnSpeed = 10f;

        protected Rigidbody rb;
        protected Transform player;
        protected Vector3 desiredVelocity; // xz only

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        protected virtual void FixedUpdate()
        {
            // Physics movement: accelerate velocity toward desiredVelocity (acceleration + velocity)
            Vector3 v = rb.velocity;
            Vector3 vXZ = new Vector3(v.x, 0f, v.z);
            Vector3 dXZ = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
            Vector3 next = Vector3.MoveTowards(vXZ, dXZ, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector3(next.x, v.y, next.z);
        }

        protected void MoveTo(Vector3 target, float speed)
        {
            // Math: subtraction, normalisation, magnitude
            Vector3 dir = target - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) { desiredVelocity = Vector3.zero; return; }

            desiredVelocity = dir.normalized * Mathf.Min(speed, maxSpeed);

            // Math: rotation
            Quaternion face = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, face, turnSpeed * Time.deltaTime);
        }

        // OOP: virtual so children can change vision rules if needed
        protected virtual bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector3 eye = transform.position + Vector3.up * 1.6f;
            Vector3 toPlayer = (player.position + Vector3.up * 1.2f) - eye;

            if (toPlayer.magnitude > visionRange) return false;                                // magnitude
            if (Vector3.Angle(transform.forward, toPlayer.normalized) > fov * 0.5f) return false; // angle + normalisation

            // Physics detection: Raycast LoS
            return Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, visionRange, visionBlockers, QueryTriggerInteraction.Ignore)
                   && hit.transform == player;
        }

        // Encapsulation: public hook (hearing) using OverlapSphere
        public virtual void OnNoiseHeard(Vector3 noisePos, float loudness = 1f)
        {
            float radius = hearingRange * Mathf.Clamp(loudness, 0.2f, 3f);
            Collider[] heard = Physics.OverlapSphere(noisePos, radius, ~0, QueryTriggerInteraction.Ignore);
            Debug.Log($"Noise at {noisePos} (r={radius}) heard by {name}. Hits: {heard.Length}");
        }

        // OOP: virtual attack
        public virtual void Attack() { /* base no-op */ }
    }
}