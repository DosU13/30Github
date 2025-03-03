using System.Collections;
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

    [Header("Weapon Settings")]
    [Tooltip("Weapon attached to player")]
    public WeaponController weaponController;

    [Header("Sound Settings")]
    [Tooltip("Audio source for movement sounds")]
    public AudioSource movementAudioSource;

    [Tooltip("Sound to play when moving")]
    public AudioClip movementSound;

    [Tooltip("Minimum velocity magnitude required to play movement sound")]
    public float movementSoundThreshold = 0.1f;

    [Tooltip("How quickly the sound volume changes based on speed")]
    public float volumeChangeSpeed = 5.0f;

    [Tooltip("Maximum volume for movement sound")]
    [Range(0, 1)]
    public float maxVolume = 1.0f;

    [Header("Character Models")]
    [Tooltip("Model to show when player is idle")]
    public GameObject idleModel;

    [Tooltip("Model to show when player is running")]
    public GameObject runningModel;

    [Header("Debuff Settings")]
    [Tooltip("Reference to the animator component")]
    public Animator animator;

    [Tooltip("Reference to the DebuffUI script")]
    public DebuffUI debuffUI;

    // Current velocity of the player
    private Vector3 currentVelocity = Vector3.zero;

    // Target volume for movement sound
    private float targetVolume = 0f;

    // Original speed value for restoring after debuffs
    private float originalSpeed;

    // Flag to track if player is under fear effect
    private bool isFeared = false;

    void Start()
    {
        // Store original speed for debuff recovery
        originalSpeed = maxSpeed;

        // If no weapon controller is assigned, try to find one in children
        if (weaponController == null)
        {
            weaponController = GetComponentInChildren<WeaponController>();
        }

        // If no animator is assigned, try to find one
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // If no debuffUI is assigned, try to find one
        if (debuffUI == null)
        {
            debuffUI = FindObjectOfType<DebuffUI>();
        }

        // Set up audio source if not assigned
        if (movementAudioSource == null)
        {
            // Try to find an existing audio source
            movementAudioSource = GetComponent<AudioSource>();

            // If none exists, create one
            if (movementAudioSource == null)
            {
                movementAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure the audio source
        if (movementAudioSource != null && movementSound != null)
        {
            movementAudioSource.clip = movementSound;
            movementAudioSource.loop = true;
            movementAudioSource.volume = 0f;
            movementAudioSource.playOnAwake = false;
            movementAudioSource.Play();
        }

        // Initialize models - default to idle
        UpdateModelVisibility(false);
    }

    void Update()
    {
        // Handle WASD movement
        HandleMovement();

        // Handle mouse aiming and shooting
        HandleAimingAndShooting();

        UpdateMovementSound();
    }

    private Vector3 fearDirection;
    void HandleMovement()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal"); // A & D keys or left/right arrows
        float vertical = Input.GetAxis("Vertical");     // W & S keys or up/down arrows

        // Create movement vector - use random direction if feared
        Vector3 inputDirection;

        if (isFeared)
        {
            inputDirection = fearDirection;
        }
        else
        {
            // Normal input when not feared
            inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        // Check if movement keys are pressed - used for model switching
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                        Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
                        Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                        isFeared; // Always show running model when feared

        // Update character model based on movement input
        UpdateModelVisibility(isMoving);

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

    // Draws movement gizmos in the editor for debugging
    void OnDrawGizmos()
    {
        // Draw a line showing the current velocity direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + currentVelocity.normalized * 2);

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

            // Draw a line from player to target for aiming
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPoint);
        }
    }

    void UpdateMovementSound()
    {
        if (movementAudioSource != null && movementSound != null)
        {
            // Determine target volume based on movement speed
            if (currentVelocity.magnitude > movementSoundThreshold)
            {
                // Scale volume based on current speed, capped at max speed
                float speedRatio = Mathf.Clamp01(currentVelocity.magnitude / maxSpeed);
                targetVolume = speedRatio * maxVolume;

                // Start sound if it's not playing
                if (!movementAudioSource.isPlaying)
                {
                    movementAudioSource.Play();
                }
            }
            else
            {
                targetVolume = 0f;
            }

            // Smoothly adjust volume
            movementAudioSource.volume = Mathf.Lerp(
                movementAudioSource.volume,
                targetVolume,
                volumeChangeSpeed * Time.deltaTime
            );

            // Stop audio if volume becomes very small
            if (movementAudioSource.volume < 0.01f && movementAudioSource.isPlaying)
            {
                movementAudioSource.Pause();
            }
        }
    }

    void UpdateModelVisibility(bool running)
    {
        // Ensure we have both models
        if (idleModel == null || runningModel == null)
            return;

        // Set appropriate model visibility
        if (running)
        {
            // Show running model, hide idle model
            runningModel.SetActive(true);
            idleModel.SetActive(false);
        }
        else
        {
            // Show idle model, hide running model
            idleModel.SetActive(true);
            runningModel.SetActive(false);
        }
    }

    // ---------------- Debuff Functions ----------------

    // Sadness debuff - slows player down by 10x
    public void GetSad()
    {
        // Slow down movement speed
        maxSpeed = originalSpeed / 10f;

        // Slow down animator if available
        if (animator != null)
        {
            animator.speed = 0.1f;
        }

        // Start coroutine to restore normal speed after delay
        StartCoroutine(ClearSadnessDebuff());
    }

    // Anxious debuff - applies to weapon controller
    public void GetAxious()
    {
        // Apply anxious effect to weapon controller
        if (weaponController != null)
        {
            weaponController.GetAxious();
        }
    }

    // Fear debuff - move randomly for 2 seconds
    public void GetFeared(Vector3 fearDirection)
    {
        // Set the feared state
        isFeared = true;
        this.fearDirection = fearDirection;

        // Start coroutine to restore normal control after delay
        StartCoroutine(ClearFearDebuff());
    }

    // Coroutine to clear the sadness debuff after duration
    private IEnumerator ClearSadnessDebuff()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Restore original speed
        maxSpeed = originalSpeed;

        // Restore animator speed
        if (animator != null)
        {
            animator.speed = 1.0f;
        }
    }

    // Coroutine to clear the fear debuff after duration
    private IEnumerator ClearFearDebuff()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Clear the feared state
        isFeared = false;
    }

    public void Debuff(EnemyController.EnemyType debuffType, Vector3 fearDirection)
    {
        switch (debuffType)
        {
            case EnemyController.EnemyType.Sadness:
                GetSad();
                break;
            case EnemyController.EnemyType.Fear:
                GetFeared(fearDirection);
                break;
            case EnemyController.EnemyType.Anxiety:
                GetAxious();
                break;
        }

        // Notify the DebuffUI
        if (debuffUI != null)
        {
            debuffUI.Debuff(debuffType);
        }
    }
}