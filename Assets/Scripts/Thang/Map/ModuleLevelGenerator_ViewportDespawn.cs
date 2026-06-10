using System.Collections.Generic;
using UnityEngine;

public class ModuleLevelGenerator_ViewportDespawn : MonoBehaviour
{
    [Header("Refs")]
    public Transform worldRoot;      // kéo World vào đây
    public Camera cam;              // để trống cũng được (auto Camera.main)

    [Header("Modules (your 4 blocks)")]
    public GameObject[] modulePrefabs; // size = 4, kéo 4 prefab vào

    [Header("Spawn")]
    public int prewarmCount = 8;     // sinh sẵn ban đầu
    public float spawnAhead = 25f;   // còn cách end cuối ~ bao nhiêu world units thì spawn thêm

    [Header("Align")]
    public float lockY = 0f;         // ép tất cả module về cùng cao độ (0)

    [Header("Despawn (when off-screen)")]
    public float despawnMargin = 0.2f; // viewport margin: 0.2 => ra khỏi màn hình 20% mới xoá

    private Transform lastEndPoint;  // EndPoint của module cuối cùng (instance)
    private readonly Queue<(GameObject go, Transform endPoint)> spawned
        = new Queue<(GameObject go, Transform endPoint)>();

    [Header("Spacing")]
    public float gapX = 0.5f; // khoảng cách giữa các block theo world units

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Spawn sẵn vài module để có map ban đầu
        for (int i = 0; i < prewarmCount; i++)
            SpawnNext();
    }

    void Update()
    {
        if (worldRoot == null) return;
        if (cam == null) cam = Camera.main;

        // Vì bạn mô phỏng "đi sang phải" bằng cách kéo worldRoot sang trái
        float progressX = -worldRoot.position.x;

        // Nếu gần tới điểm end của module cuối thì spawn thêm
        if (lastEndPoint != null)
        {
            float lastEndX = lastEndPoint.position.x;
            if (progressX > lastEndX - spawnAhead)
            {
                SpawnNext();
            }
        }
        else
        {
            // lỡ lastEndPoint null thì spawn 1 cái để khởi động lại
            SpawnNext();
        }

        CleanupByViewport();
    }

    void SpawnNext()
    {
        if (modulePrefabs == null || modulePrefabs.Length == 0)
        {
            Debug.LogError("[Generator] modulePrefabs rỗng. Hãy kéo 4 prefab vào.");
            return;
        }

        if (worldRoot == null)
        {
            Debug.LogError("[Generator] worldRoot chưa set (World).");
            return;
        }

        GameObject prefab = modulePrefabs[Random.Range(0, modulePrefabs.Length)];
        GameObject go = Instantiate(prefab, worldRoot);

        Transform sp = go.transform.Find("StartPoint");
        Transform ep = go.transform.Find("EndPoint");

        if (sp == null || ep == null)
        {
            Debug.LogError($"[Generator] Prefab {prefab.name} thiếu StartPoint hoặc EndPoint (đúng tên, đúng cấp con).");
            Destroy(go);
            return;
        }

        // 1) Đặt module mới nối theo local-space của worldRoot để tránh lệch khi worldRoot di chuyển
        if (lastEndPoint == null)
        {
            go.transform.localPosition = new Vector3(0f, lockY, 0f);
        }
        else
        {
            Vector3 lastEndLocal = worldRoot.InverseTransformPoint(lastEndPoint.position);
            Vector3 startLocal = worldRoot.InverseTransformPoint(sp.position);

            // deltaLocal để StartPoint khớp EndPoint
            Vector3 deltaLocal = lastEndLocal - startLocal;

            // cộng thêm khoảng cách theo X (để không dính nhau)
            deltaLocal.x += gapX;

            go.transform.localPosition += deltaLocal;

            // khóa Y (và giữ z)
            var p = go.transform.localPosition;
            go.transform.localPosition = new Vector3(p.x, lockY, p.z);
        }

        // Cập nhật lastEndPoint (dùng ep của instance mới)
        lastEndPoint = ep;

        // Lưu để despawn theo camera
        spawned.Enqueue((go, ep));
    }

    void CleanupByViewport()
    {
        if (cam == null) return;

        while (spawned.Count > 0)
        {
            var (go, endPoint) = spawned.Peek();
            if (go == null || endPoint == null)
            {
                spawned.Dequeue();
                continue;
            }

            // EndPoint ra khỏi màn hình bên trái -> xoá
            float vx = cam.WorldToViewportPoint(endPoint.position).x;

            if (vx < -despawnMargin)
            {
                spawned.Dequeue();
                Destroy(go);
            }
            else break;
        }
    }
}