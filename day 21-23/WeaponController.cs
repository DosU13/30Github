using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Projectile prefab to shoot")]
    public GameObject projectilePrefab;

    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.5f;

    [Tooltip("Offset from player for projectile spawn")]
    public Vector3 projectileSpawnOffset = new Vector3(0, 0.5f, 0);

    [Tooltip("Sound to play when shooting")]
    public AudioClip shootSound;

    [Header("Weapon Stats")]
    [Tooltip("Damage per projectile")]
    public int damage = 1;

    [Tooltip("Speed of projectiles")]
    public float projectileSpeed = 15f;

    [Tooltip("How long projectiles exist before auto-destruction")]
    public float projectileLifetime = 5f;

    // Internal tracking
    private float nextFireTime = 0f;
    private Transform playerTransform;
    private AudioSource audioSource;
    private bool isAnxious = false;

    void Start()
    {
        // Get player transform (usually parent of weapon)
        playerTransform = transform.parent;
        if (playerTransform == null)
        {
            playerTransform = transform; // Use own transform if no parent
        }

        // Get or add audio source for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.volume = 0.5f;
        }
    }

    void Update()
    {
        // Always auto-fire when possible
        if (Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    // Method to fire projectiles
    void Fire()
    {
        if (projectilePrefab == null || Time.time < nextFireTime)
            return;

        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.rotation * projectileSpawnOffset;

        // Get aim direction (either mouse direction or random if anxious)
        Vector3 aimDirection = isAnxious ? GetRandomDirection() : GetMouseAimDirection();

        // Spawn and setup projectile
        SpawnProjectile(spawnPosition, aimDirection);

        // Play shoot sound if assigned
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }

    // Helper method to spawn a single projectile
    GameObject SpawnProjectile(Vector3 position, Vector3 direction)
    {
        // Create rotation that faces the direction
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Instantiate projectile at spawn position with correct rotation
        GameObject projectileObj = Instantiate(projectilePrefab, position, rotation);

        // Get the projectile component and configure it
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.speed = projectileSpeed;
            projectile.damage = damage;
            projectile.lifetime = projectileLifetime;
            projectile.isPlayerProjectile = true;
        }
        else
        {
            // If no Projectile component, add velocity to Rigidbody if it exists
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;

                // Set up autodestruction if no Projectile script handles it
                Destroy(projectileObj, projectileLifetime);
            }
        }

        return projectileObj;
    }

    // Get aim direction based on mouse position
    Vector3 GetMouseAimDirection()
    {
        // Cast a ray from the camera to the y=0 plane based on mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        // Default direction (forward) if ray doesn't hit
        Vector3 targetPosition = transform.position + transform.forward;

        // If the ray hits the ground plane, use that point
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            targetPosition = ray.GetPoint(rayDistance);
        }

        // Calculate direction from weapon to target (ignoring Y)
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;

        return direction.normalized;
    }

    // Get random direction for anxious state
    Vector3 GetRandomDirection()
    {
        // Create a random direction on the XZ plane
        float randomAngle = Random.Range(0f, 360f);
        return new Vector3(
            Mathf.Sin(randomAngle * Mathf.Deg2Rad),
            0,
            Mathf.Cos(randomAngle * Mathf.Deg2Rad)
        ).normalized;
    }

    // Apply the Anxious debuff to make player shoot in random directions
    public void GetAxious()
    {
        // Don't apply if already anxious
        if (isAnxious)
            return;

        // Set the anxious state
        isAnxious = true;

        // Start coroutine to restore normal aiming after delay
        StartCoroutine(ClearAnxiousDebuff());
    }

    // Coroutine to clear the anxious debuff after duration
    private IEnumerator ClearAnxiousDebuff()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Clear the anxious state
        isAnxious = false;
    }

    // Draw gizmos for debugging
    void OnDrawGizmos()
    {
        // Draw projectile spawn point
        Gizmos.color = Color.red;
        Vector3 spawnPos = transform.position + transform.rotation * projectileSpawnOffset;
        Gizmos.DrawSphere(spawnPos, 0.1f);

        // Draw aim direction
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 direction = isAnxious ? GetRandomDirection() : GetMouseAimDirection();
            Gizmos.DrawRay(spawnPos, direction * 3f);
        }
    }
}