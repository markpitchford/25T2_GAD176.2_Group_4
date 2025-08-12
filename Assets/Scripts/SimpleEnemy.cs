using UnityEngine;

namespace SimpleAI.Minimal
{
    /// <summary>Minimal chaser: move toward player, lunge + attack in range.</summary>
    public class SimpleEnemy : EnemyBase
    {
        [Header("Chase")]
        [SerializeField] private string enemyName = "Security";
        [SerializeField] private float chaseSpeed = 3.2f;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float lungeForce = 2.5f; // Physics: forces (small burst on attack)

        void Update()
        {
            if (!player) { desiredVelocity = Vector3.zero; return; }

            // See player? (Raycast + FOV)
            if (CanSeePlayer())
            {
                MoveTo(player.position, chaseSpeed);

                // Close enough? Attack + tiny force burst
                if (Vector3.Distance(transform.position, player.position) <= attackRange)
                    Attack();
            }
            else
            {
                desiredVelocity = Vector3.zero; // idle when not seeing the player
            }
        }

        public override void OnNoiseHeard(Vector3 noisePos, float loudness = 1f)
        {
            base.OnNoiseHeard(noisePos, loudness); // keeps the debug + OverlapSphere usage
            // Minimal reaction: face the noise and take one step toward it
            MoveTo(noisePos, Mathf.Min(chaseSpeed * 0.75f, maxSpeed));
        }

        public override void Attack()
        {
            Debug.Log($"{enemyName} attacks!");
            // Example damage: player.GetComponent<PlayerHealth>()?.TakeDamage(dmg);

            // Physics movement: explicit force use (in addition to velocity/acceleration)
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            rb.AddForce(forward * lungeForce, ForceMode.VelocityChange);
        }
    }
}