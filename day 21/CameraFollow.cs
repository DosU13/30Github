using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Assign the player's transform in the Inspector
    public float smoothSpeed = 5f; // Adjust this for smoothness
    public Vector3 offset = new Vector3(0, 10, -10); // Default offset for 30-degree view

    private void LateUpdate()
    {
        if (player == null) return;

        // Calculate the target position
        Vector3 targetPosition = player.position + offset;

        // Smoothly interpolate the camera's position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Ensure the camera always looks at the player
        transform.LookAt(player.position);
    }
}
