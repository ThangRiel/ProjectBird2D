using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // Cấu trúc để quản lý nhiều loại prefab
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag)) return null;

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // TỰ ĐỘNG GÁN NHÃN: Đảm bảo đối tượng có script PoolItem
        PoolItem item = objectToSpawn.GetComponent<PoolItem>();
        if (item == null)
        {
            item = objectToSpawn.AddComponent<PoolItem>();
        }
        item.assignedTag = tag;

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }
    // Trong ObjectPooler.cs
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);

        // Đảm bảo tag tồn tại trong từ điển
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Tag không tồn tại trong Pool: " + tag);
        }
    }
}