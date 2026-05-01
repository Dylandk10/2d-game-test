using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private string enemyKey = "Enemy";
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnWave(Vector2 portalPos, int normalCount, int majorCount)
    {
        SpawnNormals(portalPos, normalCount);
    }

    public void SpawnWaveMinorsOnly(Vector2 portalPos, int count)
    {
        if (count <= 0) return;

        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float xOffset = startX + (i * spacing);
            Vector2 spawnPos = portalPos + new Vector2(xOffset, 0);

            Spawn(enemyKey, spawnPos);
        }
    }

    void SpawnNormals(Vector2 center, int count)
    {
        if (count <= 0) return;

        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float xOffset = startX + (i * spacing);
            Vector2 spawnPos = center + new Vector2(xOffset, 0);

            Spawn(enemyKey, spawnPos);
        }
    }

    void Spawn(string key, Vector2 pos)
    {
        GameObject obj = PoolManager.Instance.Get(key);

        if (obj == null)
        {
            Debug.LogError($"Failed to spawn enemy with key: {key}");
            return;
        }

        obj.transform.position = pos;

        // ✅ Initialize the enemy after spawning
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.Initialize(playerTransform);
    }
}