using UnityEngine;

public class SadnessMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;  // Slow movement speed
    public float rotationSpeed = 3.0f;

    private EnemyController enemyController;
    private Transform playerTransform;

    void Start()
    {
        // Get reference to the enemy controller
        enemyController = GetComponent<EnemyController>();

        if (enemyController != null)
        {
            playerTransform = enemyController.playerTransform;
        }
    }

    void Update()
    {
        if (enemyController == null || playerTransform == null)
            return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Only follow if within detection range and beyond minimum distance
        if (distanceToPlayer <= enemyController.detectionRange && distanceToPlayer > enemyController.minimumDistance)
        {
            // Get direction to player
            Vector3 directionToPlayer = GetDirectionToPlayer();

            // Move slowly toward player
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;

            // Rotate to face movement direction
            RotateTowardDirection(directionToPlayer);
        }
    }

    Vector3 GetDirectionToPlayer()
    {
        // Calculate direction vector to player
        Vector3 direction = playerTransform.position - transform.position;

        // Keep movement on XZ plane only
        direction.y = 0;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        // Return normalized direction
        return direction.normalized;
    }

    void RotateTowardDirection(Vector3 direction)
    {
        // Create rotation that looks in the target direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate to face movement direction
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}