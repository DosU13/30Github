using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 50f;
    public LayerMask enemyLayer;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 direction = transform.right;
        float distance = speed * Time.deltaTime;

        // Raycast from last position to new position
        RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction, distance, enemyLayer);
        if (hit.collider != null)
        {
            // If hit enemy, destroy it and the projectile
            Destroy(hit.collider.gameObject);
            Destroy(gameObject);
            return;
        }

        // Move normally if no enemy hit
        transform.position += direction * distance;
        lastPosition = transform.position;
    }
}
