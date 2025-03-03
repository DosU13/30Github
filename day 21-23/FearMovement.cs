using UnityEngine;

public class FearMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 8.0f;

    [Header("Leap Settings")]
    public float leapCooldown = 3.0f;
    public float leapSpeed = 10.0f;
    public float leapDuration = 0.5f;
    public float leapProbability = 0.7f;  // Chance to leap when cooldown is ready

    private EnemyController enemyController;
    private Transform playerTransform;
    private float leapTimer;
    private float currentLeapTime;
    private bool isLeaping = false;
    private Vector3 leapDirection;

    void Start()
    {
        // Get reference to the enemy controller
        enemyController = GetComponent<EnemyController>();

        if (enemyController != null)
        {
            playerTransform = enemyController.playerTransform;
        }

        // Initialize leap timer
        leapTimer = leapCooldown;
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

            if (isLeaping)
            {
                // Handle leap movement
                HandleLeap();
            }
            else
            {
                // Normal movement toward player
                transform.position += directionToPlayer * moveSpeed * Time.deltaTime;

                // Update leap cooldown
                leapTimer -= Time.deltaTime;

                // Check if we should leap
                if (leapTimer <= 0 && Random.value <= leapProbability)
                {
                    StartLeap(directionToPlayer);
                }
            }

            // Always rotate to face movement direction
            RotateTowardDirection(isLeaping ? leapDirection : directionToPlayer);
        }
    }

    void StartLeap(Vector3 direction)
    {
        isLeaping = true;
        leapDirection = direction;
        currentLeapTime = 0;
    }

    void HandleLeap()
    {
        currentLeapTime += Time.deltaTime;

        if (currentLeapTime < leapDuration)
        {
            // Move quickly in leap direction
            transform.position += leapDirection * leapSpeed * Time.deltaTime;
        }
        else
        {
            // End leap
            isLeaping = false;
            leapTimer = leapCooldown;
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