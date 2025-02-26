using UnityEngine;
using System.Linq;

public class ShootingSystem : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 5f; // Maximum shooting range

    void Update()
    {
        if (ShouldFire())
        {
            FireAtNearestEnemy();
        }
    }

    bool ShouldFire()
    {
        if (Input.anyKeyDown)
        {
            return true;
        }

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

        // Calculate direction and fire
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
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
