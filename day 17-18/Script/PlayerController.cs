// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float lookThreshold = 0.5f;
    private Vector3 targetPosition;

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        transform.position = Vector3.Lerp(transform.position, mousePosition, speed * Time.deltaTime);

        Vector3 direction = mousePosition - transform.position;
        if (direction.magnitude > lookThreshold)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}   