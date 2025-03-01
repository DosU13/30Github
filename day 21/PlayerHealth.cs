using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 10;

    [Tooltip("Current health points")]
    public int currentHealth;

    [Tooltip("Invincibility time after taking damage (seconds)")]
    public float invincibilityTime = 1.0f;

    [Header("UI References")]
    [Tooltip("Health bar UI element")]
    public Image healthBar;

    [Tooltip("Text to display current/max health")]
    public Text healthText;

    [Header("Audio")]
    [Tooltip("Sound to play when taking damage")]
    public AudioClip damageSound;

    [Tooltip("Sound to play when healing")]
    public AudioClip healSound;

    [Tooltip("Sound to play when player dies")]
    public AudioClip deathSound;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;

    // Internal variables
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Set up UI if assigned
        UpdateHealthUI();

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (damageSound != null || healSound != null || deathSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                // Stop blinking effect
                SetRendererBlinking(false);
            }
        }
    }

    // Public method to apply damage
    public void TakeDamage(int damage)
    {
        // Skip if invincible
        if (isInvincible)
            return;

        // Apply damage
        currentHealth -= damage;

        // Trigger damaged event
        OnDamaged.Invoke();

        // Play hit sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityTime;

        // Visual feedback for being hit
        SetRendererBlinking(true);

        // Update UI
        UpdateHealthUI();

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Public method for healing
    public void Heal(int amount)
    {
        // Apply healing
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Trigger healed event
        OnHealed.Invoke();

        // Play heal sound
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        // Update UI
        UpdateHealthUI();
    }

    // Handle player death
    void Die()
    {
        // Trigger death event
        OnDeath.Invoke();

        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Disable player controls
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable weapon
        WeaponController weaponController = GetComponentInChildren<WeaponController>();
        if (weaponController != null)
        {
            weaponController.enabled = false;
        }

        // You could show game over screen here
        Debug.Log("Player died!");

        // Don't destroy the player object, just disable it
        // Alternatively, you could implement respawn logic here
    }

    // Update health UI elements
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    // Visual effect for invincibility
    void SetRendererBlinking(bool blinking)
    {
        if (blinking)
        {
            StartCoroutine(BlinkEffect());
        }
        else
        {
            StopAllCoroutines();

            // Make sure renderer is visible
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                color.a = 1.0f;
                renderer.material.color = color;
            }
        }
    }

    // Coroutine for blinking effect
    System.Collections.IEnumerator BlinkEffect()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Blink while invincible
        while (isInvincible)
        {
            // Toggle visibility
            foreach (Renderer renderer in renderers)
            {
                Color color = renderer.material.color;
                color.a = color.a > 0.5f ? 0.3f : 1.0f;
                renderer.material.color = color;
            }

            // Wait a bit
            yield return new WaitForSeconds(0.1f);
        }

        // Ensure visibility is restored
        foreach (Renderer renderer in renderers)
        {
            Color color = renderer.material.color;
            color.a = 1.0f;
            renderer.material.color = color;
        }
    }
}