using System.Collections.Generic;
using UnityEngine;

public class ModuleLevelGenerator : MonoBehaviour
{
    [Header("Refs")]
    public Transform worldRoot;

    [Header("Modules (4 blocks)")]
    public ModulePrefab[] modules;

    [Header("Spawn")]
    public int prewarm = 8;          // sinh sẵn ban đầu
    public int keepBehind = 6;       // giữ lại mấy module phía sau rồi mới huỷ

    private Transform lastEndPoint;
    private readonly Queue<GameObject> spawned = new Queue<GameObject>();

    [System.Serializable]
    public class ModulePrefab
    {
        public GameObject prefab;
        public Transform startPoint; // không set ở đây, sẽ lấy từ instance
        public Transform endPoint;
    }

    void Start()
    {
        // Spawn module đầu tiên tại (0,0) hoặc vị trí bạn muốn
        for (int i = 0; i < prewarm; i++)
            SpawnNext();
    }

    void Update()
    {
        // progress = -worldRoot.position.x (vì worldRoot chạy ngược)
        float progressX = -worldRoot.position.x;

        // Khi tiến gần tới end của module cuối, spawn thêm
        // (ngưỡng này bạn có thể chỉnh)
        if (lastEndPoint != null && progressX > lastEndPoint.position.x - 20f)
        {
            SpawnNext();
            Cleanup();
        }
    }

    void SpawnNext()
    {
        if (modules == null || modules.Length == 0) return;

        var pick = modules[Random.Range(0, modules.Length)].prefab;
        var go = Instantiate(pick, worldRoot);

        // tìm start/end point trong prefab instance
        var sp = go.transform.Find("StartPoint");
        var ep = go.transform.Find("EndPoint");

        if (sp == null || ep == null)
        {
            Debug.LogError($"Prefab {pick.name} thiếu StartPoint hoặc EndPoint");
            Destroy(go);
            return;
        }

        if (lastEndPoint == null)
        {
            // module đầu: đặt tại gốc
            go.transform.position = Vector3.zero;
        }
        else
        {
            // đặt sao cho StartPoint của module mới trùng EndPoint module trước
            Vector3 offset = lastEndPoint.position - sp.position;
            go.transform.position += offset;
        }

        lastEndPoint = ep;
        spawned.Enqueue(go);
    }

    void Cleanup()
    {
        while (spawned.Count > keepBehind)
        {
            var old = spawned.Dequeue();
            Destroy(old);
        }
    }
}