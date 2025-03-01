using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Target Settings")]
    //[Tooltip("The player GameObject this enemy will follow")]
    public Transform playerTransform;

    [Header("Movement Settings")]
    //[Tooltip("Constant movement speed in units per second")]
    public float moveSpeed = 3.0f;

    //[Tooltip("How quickly the enemy rotates to face movement direction")]
    public float rotationSpeed = 8.0f;

    [Header("Detection Settings")]
    //[Tooltip("Maximum distance to detect and follow the player")]
    public float detectionRange = 20.0f;

    //[Tooltip("Minimum distance to maintain from player")]
    public float minimumDistance = 1.0f;

    // Reference to the enemy's rigidbody for physics movement
    private Rigidbody rb;

    // Flag to track movement method
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
    }

    void FixedUpdate()
    {
        // Only use FixedUpdate for physics movement
        if (usingRigidbody)
        {
            MoveWithPhysics();
        }
    }

    void Update()
    {
        // Skip if no player is assigned
        if (playerTransform == null)
            return;

        // For non-Rigidbody movement, handle in Update
        if (!usingRigidbody)
        {
            MoveWithTransform();
        }

        // Always handle rotation in Update for smoother visual updates
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