using System.Collections;
using System.Collections.Generic;
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

    [Header("Auto-Fire Settings")]
    [Tooltip("Whether weapon should automatically fire")]
    public bool autoFire = false;

    [Header("Weapon Stats")]
    [Tooltip("Damage per projectile")]
    public int damage = 1;

    [Tooltip("Speed of projectiles")]
    public float projectileSpeed = 15f;

    [Tooltip("How long projectiles exist before auto-destruction")]
    public float projectileLifetime = 5f;

    [Header("Weapon Type")]
    [Tooltip("Shot pattern type")]
    public ShotPatternType shotPattern = ShotPatternType.Single;

    [Tooltip("Number of projectiles per shot (for multi/spread patterns)")]
    public int projectilesPerShot = 1;

    [Tooltip("Angle between projectiles (for spread patterns)")]
    public float spreadAngle = 15f;

    [Header("Targeting")]
    [Tooltip("Whether to aim at mouse position or provided target")]
    public bool useMouseAiming = true;

    [Tooltip("External target transform (used when not using mouse aiming)")]
    public Transform targetTransform;

    // Enum for different shot patterns
    public enum ShotPatternType
    {
        Single,     // Standard single shot
        Spread,     // Multiple shots in a spread pattern
        Circle      // Shots in all directions
    }

    // Internal tracking
    private float nextFireTime = 0f;
    private Transform playerTransform;
    private AudioSource audioSource;

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
        // Handle auto-fire if enabled
        if (autoFire && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    // Public method to trigger shooting manually
    public void Fire()
    {
        if (projectilePrefab == null || Time.time < nextFireTime)
            return;

        // Based on shot pattern, fire appropriate number of projectiles
        switch (shotPattern)
        {
            case ShotPatternType.Single:
                FireSingleProjectile();
                break;

            case ShotPatternType.Spread:
                FireSpreadProjectiles();
                break;

            case ShotPatternType.Circle:
                FireCircleProjectiles();
                break;
        }

        // Play shoot sound if assigned
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }

    // Enable or disable autofire
    public void SetAutoFire(bool enabled)
    {
        autoFire = enabled;
    }

    // Set targeting mode
    public void SetTargetingMode(bool useMouseTarget, Transform newTarget = null)
    {
        useMouseAiming = useMouseTarget;
        targetTransform = newTarget;
    }

    // Fire a single projectile in the aiming direction
    void FireSingleProjectile()
    {
        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.rotation * projectileSpawnOffset;

        // Get aim direction based on targeting mode
        Vector3 aimDirection = GetAimDirection();

        // Spawn and setup projectile
        SpawnProjectile(spawnPosition, aimDirection);
    }

    // Fire multiple projectiles in a spread pattern
    void FireSpreadProjectiles()
    {
        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.rotation * projectileSpawnOffset;

        // Get base direction from aim
        Vector3 baseDirection = GetAimDirection();

        // Calculate total spread angle
        float totalSpreadAngle = spreadAngle * (projectilesPerShot - 1);
        float startAngle = -totalSpreadAngle / 2f;

        // Spawn projectiles across the spread
        for (int i = 0; i < projectilesPerShot; i++)
        {
            // Calculate angle for this projectile
            float angle = startAngle + (spreadAngle * i);

            // Rotate base direction by this angle
            Vector3 projectileDirection = Quaternion.Euler(0, angle, 0) * baseDirection;

            // Spawn projectile
            SpawnProjectile(spawnPosition, projectileDirection);
        }
    }

    // Fire projectiles in all directions (360 degrees)
    void FireCircleProjectiles()
    {
        // Calculate spawn position
        Vector3 spawnPosition = transform.position + transform.rotation * projectileSpawnOffset;

        // Calculate angle between projectiles
        float angleBetween = 360f / projectilesPerShot;

        // Spawn projectiles in a circle
        for (int i = 0; i < projectilesPerShot; i++)
        {
            // Calculate angle for this projectile
            float angle = i * angleBetween;

            // Calculate direction using sin/cos
            Vector3 projectileDirection = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0,
                Mathf.Cos(angle * Mathf.Deg2Rad)
            );

            // Spawn projectile
            SpawnProjectile(spawnPosition, projectileDirection);
        }
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

    // Get aim direction based on targeting mode
    Vector3 GetAimDirection()
    {
        if (useMouseAiming)
        {
            return GetMouseAimDirection();
        }
        else if (targetTransform != null)
        {
            return GetTargetAimDirection();
        }
        else
        {
            // Default to forward direction if no target
            return transform.forward;
        }
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

    // Get aim direction toward a specific target
    Vector3 GetTargetAimDirection()
    {
        // Calculate direction to target
        Vector3 direction = targetTransform.position - transform.position;

        // Keep aiming on XZ plane (ignore Y differences)
        direction.y = 0;

        return direction.normalized;
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
            Vector3 direction = GetAimDirection();
            Gizmos.DrawRay(spawnPos, direction * 3f);

            // Visualize spread (if using spread shot)
            if (shotPattern == ShotPatternType.Spread && projectilesPerShot > 1)
            {
                Gizmos.color = Color.yellow;
                float totalSpreadAngle = spreadAngle * (projectilesPerShot - 1);
                float startAngle = -totalSpreadAngle / 2f;

                // Draw line for first and last projectile in spread
                Vector3 leftDirection = Quaternion.Euler(0, startAngle, 0) * direction;
                Vector3 rightDirection = Quaternion.Euler(0, -startAngle, 0) * direction;

                Gizmos.DrawRay(spawnPos, leftDirection * 3f);
                Gizmos.DrawRay(spawnPos, rightDirection * 3f);
            }
        }
    }
}