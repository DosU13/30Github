using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed in units per second")]
    public float maxSpeed = 5.0f;

    [Tooltip("How quickly the player accelerates")]
    public float acceleration = 10.0f;

    [Tooltip("How quickly the player rotates to face movement direction")]
    public float rotationSpeed = 10.0f;

    [Header("Movement Mode")]
    [Tooltip("Toggle between mouse-follow (true) and WASD (false) movement")]
    public bool mouseFollowMode = true;

    [Tooltip("Key to toggle between movement modes")]
    public KeyCode toggleModeKey = KeyCode.Tab;

    [Header("Weapon Settings")]
    [Tooltip("Weapon attached to player")]
    public WeaponController weaponController;

    [Tooltip("Whether to auto-fire or require input")]
    public bool autoFire = true;

    [Tooltip("Key to manual fire if not using auto-fire")]
    public KeyCode fireKey = KeyCode.Space;

    // Current velocity of the player
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        // If no weapon controller is assigned, try to find one in children
        if (weaponController == null)
        {
            weaponController = GetComponentInChildren<WeaponController>();
        }
    }

    void Update()
    {
        // Check for mode toggle
        if (Input.GetKeyDown(toggleModeKey))
        {
            mouseFollowMode = !mouseFollowMode;
        }

        // Handle movement based on selected mode
        if (mouseFollowMode)
        {
            HandleMouseFollowMovement();
        }
        else
        {
            HandleWASDMovement();
        }

        // Handle mouse aiming and shooting
        HandleAimingAndShooting();
    }

    void HandleMouseFollowMovement()
    {
        // Cast a ray from the camera to the y=0 plane based on mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Define the y=0 plane
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        // Variable to store the ray hit distance
        float rayDistance;

        // Target position if the ray hits the ground plane
        Vector3 targetPosition = transform.position;

        // If the ray hits the ground plane, update the target position
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // Get the point where the ray hits the plane
            targetPosition = ray.GetPoint(rayDistance);

            // Ensure we keep the original y position of the player
            targetPosition.y = transform.position.y;
        }

        // Calculate direction to the target
        Vector3 directionToTarget = targetPosition - transform.position;

        // Only move if we're not already at the target
        if (directionToTarget.magnitude > 0.1f)
        {
            // Normalize the direction
            Vector3 moveDirection = directionToTarget.normalized;

            // Calculate desired velocity (direction * speed)
            Vector3 desiredVelocity = moveDirection * maxSpeed;

            // Smoothly interpolate from current velocity to desired velocity
            currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);

            // Move the player using the calculated velocity
            transform.position += currentVelocity * Time.deltaTime;

            // Rotate player to face the movement direction
            if (currentVelocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Gradually slow down if close to target
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }
    }

    void HandleWASDMovement()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal"); // A & D keys or left/right arrows
        float vertical = Input.GetAxis("Vertical");     // W & S keys or up/down arrows

        // Create movement vector
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude > 0.1f)
        {
            // Calculate desired velocity (direction * speed)
            Vector3 desiredVelocity = inputDirection * maxSpeed;

            // Smoothly interpolate from current velocity to desired velocity
            currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);

            // Move the player using the calculated velocity
            transform.position += currentVelocity * Time.deltaTime;

            // Rotate player to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Gradually slow down if no input
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }
    }

    void HandleAimingAndShooting()
    {
        // Skip if no weapon controller
        if (weaponController == null)
            return;

        // In WASD mode, use the mouse for aiming
        if (!mouseFollowMode)
        {
            // Cast ray for mouse position to determine aim direction
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 targetPoint = ray.GetPoint(rayDistance);
                targetPoint.y = transform.position.y; // Keep same height

                // Look at the mouse position for aiming
                Vector3 lookDirection = (targetPoint - transform.position).normalized;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }

        // If using manual fire, check for input
        if (!autoFire && Input.GetKeyDown(fireKey))
        {
            weaponController.Fire();
        }
        else if (autoFire && weaponController != null)
        {
            // Note: For auto-fire, the WeaponController handles timing internally
        }
    }

    // Draws movement gizmos in the editor for debugging
    void OnDrawGizmos()
    {
        // Draw a line showing the current velocity direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + currentVelocity.normalized * 2);

        // Show current mode
        string mode = mouseFollowMode ? "Mouse Follow" : "WASD";

        // Cast ray for current mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);

            // Draw a sphere at the target position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPoint, 0.3f);

            // Draw a line from player to target (yellow for mouse follow, green for WASD aim)
            Gizmos.color = mouseFollowMode ? Color.yellow : Color.green;
            Gizmos.DrawLine(transform.position, targetPoint);
        }
    }
}