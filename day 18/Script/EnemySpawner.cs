using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public float spawnRadius = 10f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnRate);
    }

    void SpawnEnemy()
    {
        Vector2 spawnPosition = (Vector2)Camera.main.transform.position + Random.insideUnitCircle.normalized * spawnRadius;
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
