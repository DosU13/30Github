using UnityEngine;

using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTransform;

    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 8.0f;

    [Header("Detection Settings")]
    public float detectionRange = 20.0f;
    public float minimumDistance = 1.0f;

    [Header("Damage Settings")]
    public int damageAmount = 10;  // Add this to set damage amount

    private Rigidbody rb;
    private bool usingRigidbody = false;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Check if we have a valid Rigidbody
        usingRigidbody = (rb != null);

        // Configure Rigidbody for better movement
        if (usingRigidbody)
        {
            // Prevent the Rigidbody from rotating due to physics
            rb.freezeRotation = true;

            // Set gravity to act properly based on mass
            rb.useGravity = true;
        }

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found. Please assign the player transform manually.");
            }
        }

        rb = GetComponent<Rigidbody>();
        usingRigidbody = (rb != null);

        if (usingRigidbody)
        {
            rb.freezeRotation = true;
            rb.useGravity = true;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found. Please assign the player transform manually.");
            }
        }
    }

    // Add this method to detect collisions
    void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Try to get the PlayerHealth component
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            // If the player has a health component, damage them
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // Destroy this enemy after hitting the player
            Destroy(gameObject);
        }
    }

    // Add this alternative method for trigger colliders
    void OnTriggerEnter(Collider other)
    {
        // Check if we collided with the player
        if (other.CompareTag("Player"))
        {
            // Try to get the PlayerHealth component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // If the player has a health component, damage them
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // Destroy this enemy after hitting the player
            Destroy(gameObject);
        }
    }

    // Rest of your existing code...
    void FixedUpdate()
    {
        if (usingRigidbody)
        {
            MoveWithPhysics();
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        if (!usingRigidbody)
        {
            MoveWithTransform();
        }

        Vector3 directionToPlayer = GetDirectionToPlayer();
        if (directionToPlayer != Vector3.zero)
        {
            RotateTowardDirection(directionToPlayer);
        }
    }

    void MoveWithPhysics()
    {
        if (playerTransform == null || rb == null)
            return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Only follow if within detection range and beyond minimum distance
        if (distanceToPlayer <= detectionRange && distanceToPlayer > minimumDistance)
        {
            // Get direction to player
            Vector3 directionToPlayer = GetDirectionToPlayer();

            // Calculate target velocity
            Vector3 targetVelocity = directionToPlayer * moveSpeed;

            // Apply movement only on XZ plane
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 newVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);

            // Set the velocity directly for constant speed movement
            rb.linearVelocity = newVelocity;
        }
        else if (distanceToPlayer <= minimumDistance)
        {
            // If too close to player, maintain Y velocity but stop XZ movement
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void MoveWithTransform()
    {
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Only follow if within detection range and beyond minimum distance
        if (distanceToPlayer <= detectionRange && distanceToPlayer > minimumDistance)
        {
            // Get direction to player
            Vector3 directionToPlayer = GetDirectionToPlayer();

            // Move the transform directly
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
        }
    }

    Vector3 GetDirectionToPlayer()
    {
        // Calculate direction vector to player
        Vector3 direction = playerTransform.position - transform.position;

        // Keep movement on XZ plane only
        direction.y = 0;

        // Return normalized direction
        return direction.normalized;
    }

    void RotateTowardDirection(Vector3 direction)
    {
        // Create rotation that looks in the target direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate to face target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Visual debugging in the editor
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw minimum distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minimumDistance);

        // Draw line to player if assigned
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}