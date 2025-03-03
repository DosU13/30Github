using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 3;

    [Tooltip("Current health points")]
    public int currentHealth;

    [Header("Death Settings")]
    [Tooltip("Prefab to spawn on death")]
    public GameObject deathEffectPrefab;

    [Tooltip("Sound to play on death")]
    public AudioClip deathSound;

    [Tooltip("Sound to play when taking damage")]
    public AudioClip hitSound;

    [Tooltip("Chance to drop an item on death (0-1)")]
    [Range(0, 1)]
    public float dropChance = 0.3f;

    [Tooltip("Possible items to drop on death")]
    public GameObject[] dropItems;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    // Internal components
    private AudioSource audioSource;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (deathSound != null || hitSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }
    }

    // Public method to apply damage
    public void TakeDamage(int damage)
    {
        // Apply damage
        currentHealth -= damage;

        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Trigger damage event
        OnDamaged.Invoke();

        // Visual feedback (could be replaced with animation)
        StartCoroutine(FlashColor());

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle enemy death
    public void Die()
    {
        int score = PlayerPrefs.GetInt("Score", 0);
        score += 100;
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
        // Trigger death event
        OnDeath.Invoke();

        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Possibly drop an item
        DropItem();

        // Destroy the enemy
        Destroy(gameObject);
    }

    // Chance to drop an item on death
    void DropItem()
    {
        if (dropItems == null || dropItems.Length == 0)
            return;

        // Check if we should drop
        if (Random.value <= dropChance)
        {
            // Select a random item
            int itemIndex = Random.Range(0, dropItems.Length);

            // Spawn the item
            if (dropItems[itemIndex] != null)
            {
                Instantiate(dropItems[itemIndex], transform.position, Quaternion.identity);
            }
        }
    }

    // Visual feedback when taking damage
    System.Collections.IEnumerator FlashColor()
    {
        // Try to get renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Store original materials/colors
        Material[] originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;

            // Create flash material
            Material flashMaterial = new Material(renderers[i].material);
            flashMaterial.color = Color.red;
            renderers[i].material = flashMaterial;
        }

        // Wait a short time
        yield return new WaitForSeconds(0.1f);

        // Restore original materials
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) // Check if object still exists
            {
                renderers[i].material = originalMaterials[i];
            }
        }
    }
}