using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string key;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] private List<Pool> pools;

    private Dictionary<string, List<GameObject>> poolDict;
    private Dictionary<string, GameObject> prefabLookup;

    private void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        poolDict = new Dictionary<string, List<GameObject>>();
        prefabLookup = new Dictionary<string, GameObject>();

        foreach (var pool in pools)
        {
            if (string.IsNullOrWhiteSpace(pool.key))
            {
                Debug.LogError("Pool has empty key!");
                continue;
            }

            if (poolDict.ContainsKey(pool.key))
            {
                Debug.LogError($"Duplicate pool key: {pool.key}");
                continue;
            }

            List<GameObject> list = new List<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                list.Add(obj);
            }

            poolDict.Add(pool.key, list);
            prefabLookup.Add(pool.key, pool.prefab);
        }
    }

    public GameObject Get(string key)
    {
        if (!poolDict.TryGetValue(key, out var list))
        {
            Debug.LogError($"Pool with key '{key}' not found!");
            return null;
        }

        // Try reuse
        foreach (var obj in list)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // Expand safely
        if (!prefabLookup.TryGetValue(key, out var prefab) || prefab == null)
        {
            Debug.LogError($"No prefab found for key '{key}'!");
            return null;
        }

        GameObject newObj = Instantiate(prefab, transform);
        newObj.SetActive(true);
        list.Add(newObj);

        Debug.LogWarning($"Pool expanded for key '{key}'");

        return newObj;
    }
}