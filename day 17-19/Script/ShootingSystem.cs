using UnityEngine;
using System.Linq;

public class ShootingSystem : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 5f;
    public RhythmManager rhythmManager;

    public float maxBulletSize = 2f; // Max bullet size for perfect timing
    public float minBulletSize = 0.3f; // Min bullet size for missed timing
    public float perfectHitWindow = 0.1f; // How close to a beat is considered a perfect hit

    void Update()
    {
        if (ShouldFire())
        {
            FireAtNearestEnemy();
        }
    }

    bool ShouldFire()
    {
        if (Input.anyKeyDown) return true;

        // Mobile: Fire when tapping the right half of the screen
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2)
            {
                return true;
            }
        }

        return false;
    }

    void FireAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        Vector3 direction;

        if (nearestEnemy != null)
        {
            // Check if enemy is within attack range
            float distance = Vector3.Distance(nearestEnemy.transform.position, transform.position);
            if (distance <= attackRange)
            {
                // Fire at enemy
                direction = (nearestEnemy.transform.position - firePoint.position).normalized;
            }
            else
            {
                direction = transform.right; // Default forward direction
            }
        }
        else
        {
            direction = transform.right; // Default forward direction
        }

        // Check rhythm accuracy
        float timeOffset = rhythmManager.GetDistanceToNearestBeat(Time.time);
        float accuracy = Mathf.Clamp01(1f - (timeOffset / perfectHitWindow)); // Scale accuracy between 0 and 1
        float bulletSize = Mathf.Lerp(minBulletSize, maxBulletSize, accuracy); // Interpolate size

        // Fire bullet
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        
        // Adjust bullet size
        bullet.transform.localScale = Vector3.one * bulletSize;

        // Adjust Trail Renderer size
        TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.startWidth *= bulletSize;
            trail.endWidth *= bulletSize;
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        return enemies
            .Where(e => Vector3.Distance(e.transform.position, transform.position) <= attackRange)
            .OrderBy(e => (e.transform.position - transform.position).sqrMagnitude)
            .FirstOrDefault();
    }
}
