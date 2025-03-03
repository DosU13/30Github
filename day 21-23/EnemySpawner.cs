using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to spawn")]
    public List<GameObject> enemyPrefabs;

    [Tooltip("Target for spawned enemies to follow (usually the player)")]
    public Transform playerTarget;

    [Tooltip("Time between enemy spawns in seconds")]
    public float spawnInterval = 3.0f;

    [Tooltip("Maximum number of enemies that can be active at once (0 = unlimited)")]
    public int maxEnemies = 10;

    [Tooltip("Whether to start spawning automatically on start")]
    public bool spawnOnStart = true;

    [Header("Spawn Area")]
    [Tooltip("Minimum distance from player to spawn enemies")]
    public float minSpawnDistance = 10.0f;

    [Tooltip("Maximum distance from player to spawn enemies")]
    public float maxSpawnDistance = 20.0f;

    [Tooltip("Whether to spawn in a circle around player or in a rectangle around this spawner")]
    public bool spawnAroundPlayer = true;

    [Tooltip("Size of rectangular spawn area (used if spawnAroundPlayer is false)")]
    public Vector2 spawnAreaSize = new Vector2(20f, 20f);

    [Header("Wave Settings")]
    [Tooltip("Enable wave-based spawning")]
    public bool useWaveSystem = false;

    [Tooltip("Number of enemies per wave")]
    public int enemiesPerWave = 5;

    [Tooltip("Time between waves in seconds")]
    public float timeBetweenWaves = 10.0f;

    // Private tracking variables
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;

    void Start()
    {
        // If we should auto-start spawning
        if (spawnOnStart)
        {
            StartSpawning();
        }

        // Try to find player if not assigned
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            else
            {
                Debug.LogWarning("Player target not assigned to EnemySpawner. Enemies won't have a target.");
            }
        }
    }

    void Update()
    {
        // Clean up our active enemies list by removing any that were destroyed
        activeEnemies.RemoveAll(enemy => enemy == null);

        // If using wave system, check if we need to start a new wave
        if (useWaveSystem && isSpawning && enemiesRemainingInWave <= 0 && activeEnemies.Count == 0)
        {
            StartCoroutine(StartNextWave());
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;

            if (useWaveSystem)
            {
                StartCoroutine(StartNextWave());
            }
            else
            {
                StartCoroutine(SpawnEnemyRoutine());
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (isSpawning)
        {
            // Check if we're under the max enemy limit
            if (maxEnemies <= 0 || activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }

            // Wait for the next spawn interval
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator StartNextWave()
    {
        currentWave++;
        Debug.Log($"Starting Wave {currentWave}");

        // Wait between waves
        if (currentWave > 1)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // Set up the new wave
        enemiesRemainingInWave = enemiesPerWave;

        // Spawn all enemies in the wave with a short delay between each
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (maxEnemies <= 0 || activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
                enemiesRemainingInWave--;

                // Small delay between spawns within a wave
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // Wait until we're under the limit again
                yield return new WaitForSeconds(1.0f);
                i--; // Try this spawn again
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null)
        {
            Debug.LogError("Enemy Prefab not assigned to spawner!");
            return;
        }

        // Determine spawn position
        Vector3 spawnPosition = DetermineSpawnPosition();

        var enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        // Instantiate the enemy
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Add to our tracking list
        activeEnemies.Add(newEnemy);

        // Configure the enemy controller if it exists
        EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
        if (enemyController != null && playerTarget != null)
        {
            enemyController.playerTransform = playerTarget;
        }
    }

    Vector3 DetermineSpawnPosition()
    {
        Vector3 spawnPos;

        if (spawnAroundPlayer && playerTarget != null)
        {
            // Spawn in a ring around the player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            float xPos = playerTarget.position.x + distance * Mathf.Cos(angle);
            float zPos = playerTarget.position.z + distance * Mathf.Sin(angle);

            spawnPos = new Vector3(xPos, 0.3f, zPos);
        }
        else
        {
            // Spawn in a rectangle around this spawner
            float xPos = transform.position.x + Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float zPos = transform.position.z + Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);

            spawnPos = new Vector3(xPos, 0f, zPos);
        }

        // Adjust Y position based on ground if needed
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(spawnPos.x, 100f, spawnPos.z), Vector3.down, out hit, 200f, LayerMask.GetMask("Default")))
        {
            spawnPos.y = hit.point.y;
        }
        else
        {
            spawnPos.y = transform.position.y;
        }

        return spawnPos;
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        if (spawnAroundPlayer && playerTarget != null)
        {
            // Draw min and max spawn distance circles around player
            Gizmos.DrawWireSphere(playerTarget.position, minSpawnDistance);
            Gizmos.DrawWireSphere(playerTarget.position, maxSpawnDistance);
        }
        else
        {
            // Draw rectangular spawn area
            Gizmos.DrawCube(transform.position, new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
        }
    }

    // Public methods to control the spawner from other scripts
    public void SetMaxEnemies(int max)
    {
        maxEnemies = max;
    }

    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }

    public int GetActiveEnemyCount()
    {
        // Clean the list first
        activeEnemies.RemoveAll(enemy => enemy == null);
        return activeEnemies.Count;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
}