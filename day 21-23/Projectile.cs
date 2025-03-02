using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("How fast the projectile moves")]
    public float speed = 15f;

    [Tooltip("Damage dealt to enemies on hit")]
    public int damage = 1;

    [Tooltip("Lifetime in seconds before auto-destruction")]
    public float lifetime = 5f;

    [Tooltip("Visual effect prefab to spawn on hit")]
    public GameObject hitEffectPrefab;

    [Header("Properties")]
    [Tooltip("Whether this projectile came from the player")]
    public bool isPlayerProjectile = true;

    // Internal variables
    private Vector3 direction;
    private float lifetimeCounter;

    void Start()
    {
        // Initialize lifetime counter
        lifetimeCounter = lifetime;
    }

    void Update()
    {
        // Move in the set direction at constant speed
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Track lifetime
        lifetimeCounter -= Time.deltaTime;
        if (lifetimeCounter <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Set direction of projectile movement
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;

        // Rotate projectile to face movement direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit an enemy (if player projectile) or player (if enemy projectile)
        bool hitTarget = false;

        if (isPlayerProjectile)
        {
            // Player projectiles damage enemies
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            EnemyController enemyController = other.GetComponent<EnemyController>();

            // If we hit something with enemy health component, damage it
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                hitTarget = true;
            }
            // Alternative check if there's no health component but there is an enemy controller
            else if (enemyController != null)
            {
                // Fallback "kill enemy" if no health system
                Destroy(other.gameObject);
                hitTarget = true;
            }
        }
        else
        {
            // Enemy projectiles damage player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hitTarget = true;
            }
        }

        // Collide with environment layers
        if (other.CompareTag("Environment"))
        {
            hitTarget = true;
        }

        // If we hit a valid target, spawn effect and destroy projectile
        if (hitTarget)
        {
            // Spawn hit effect if set
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}