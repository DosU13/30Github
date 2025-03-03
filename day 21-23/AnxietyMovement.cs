using UnityEngine;

public class AnxietyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float chaosFactor = 3.0f;
    public float directionChangeInterval = 0.5f;

    private EnemyController enemyController;
    private Transform playerTransform;
    private Vector3 moveDirection;
    private float directionTimer;
    private Vector3 lookDirection;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();

        if (enemyController != null)
        {
            playerTransform = enemyController.playerTransform;
        }

        PickNewDirection();
    }

    void Update()
    {
        if (enemyController == null || playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= enemyController.detectionRange && distanceToPlayer > enemyController.minimumDistance)
        {
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0)
            {
                PickNewDirection();
                directionTimer = directionChangeInterval;
            }

            // Move in the chaotic direction
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Rotate while keeping the enemy parallel to the ground
            RotateTowardDirection(lookDirection);
        }
    }

    void PickNewDirection()
    {
        Vector3 directionToPlayer = GetDirectionToPlayer();

        Vector3 randomOffset = new Vector3(
            Random.Range(-1f, 1f) * chaosFactor,
            0, // Ensure no vertical movement
            Random.Range(-1f, 1f) * chaosFactor
        );

        moveDirection = (directionToPlayer + randomOffset).normalized;
        moveDirection.y = 0; // Ensure movement stays on the ground

        if (Random.value > 0.7f)
        {
            lookDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }
        else
        {
            lookDirection = moveDirection;
        }
        lookDirection.y = 0; // Ensure no looking up/down
    }

    Vector3 GetDirectionToPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Prevent movement in Y-axis
        return direction.normalized;
    }

    void RotateTowardDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Preserve only the Y-axis rotation
            transform.rotation = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);

            // Keep position on the ground
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }

}
