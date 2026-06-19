using System.Collections.Generic;
using UnityEngine;

public class ChunkLevelGeneratorH : MonoBehaviour
{
    [Header("References")]
    public Transform player;               // kéo Bird vào
    public Camera cam;                     // để trống = Camera.main

    [Header("Chunk Prefabs")]
    public GameObject[] chunkPrefabs;      // kéo các chunk prefab vào

    [Header("Spawn")]
    public int prewarmCount = 5;           // sinh sẵn lúc đầu
    public float spawnAhead = 20f;         // spawn thêm khi player cách end chunk cuối < 20u

    [Header("Align")]
    public float lockY = 0f;              // ép tất cả chunk cùng cao độ

    [Header("Despawn")]
    public float despawnMargin = 2f;       // xóa khi chunk ra khỏi màn hình bên trái 2 units

    [Header("Spacing")]
    public float gapX = 0f;               // khoảng cách giữa các chunk

    // ── Internal ──────────────────────────────────────────────
    private float lastEndX;               // X của EndPoint chunk cuối cùng
    private readonly Queue<(GameObject go, float endX)> spawned
        = new Queue<(GameObject go, float endX)>();

    // ── Lifecycle ──────────────────────────────────────────────
    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Spawn từ vị trí player trở đi
        lastEndX = player != null ? player.position.x : 0f;

        for (int i = 0; i < prewarmCount; i++)
            SpawnNext();
    }

    void Update()
    {
        if (player == null) return;
        if (cam == null) cam = Camera.main;

        // Spawn thêm khi player gần tới cuối chunk cuối
        if (player.position.x > lastEndX - spawnAhead)
            SpawnNext();

        DespawnOffscreen();
    }

    // ── Spawn ──────────────────────────────────────────────────
    void SpawnNext()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("[ChunkGen] chunkPrefabs rỗng!");
            return;
        }

        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];

        // Spawn tại gốc tọa độ world (không parented)
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        Transform sp = go.transform.Find("StartPoint");
        Transform ep = go.transform.Find("EndPoint");

        if (sp == null || ep == null)
        {
            Debug.LogError($"[ChunkGen] Prefab '{prefab.name}' thiếu StartPoint hoặc EndPoint.");
            Destroy(go);
            return;
        }

        // Tính offset để StartPoint của chunk mới khớp với lastEndX
        float offsetX = lastEndX + gapX - sp.position.x;
        go.transform.position = new Vector3(
            go.transform.position.x + offsetX,
            lockY,
            0f
        );

        // Cập nhật lastEndX
        lastEndX = ep.position.x;
        spawned.Enqueue((go, lastEndX));

        Debug.Log($"[ChunkGen] Spawned '{prefab.name}' | endX: {lastEndX:F1}");
    }

    // ── Despawn ────────────────────────────────────────────────
    void DespawnOffscreen()
    {
        if (cam == null) return;

        while (spawned.Count > 0)
        {
            var (go, endX) = spawned.Peek();

            if (go == null)
            {
                spawned.Dequeue();
                continue;
            }

            // Chuyển endX sang viewport (0 = trái màn hình, 1 = phải)
            Vector3 viewportPos = cam.WorldToViewportPoint(new Vector3(endX, 0f, 0f));

            // Xóa khi chunk hoàn toàn ra khỏi màn hình bên trái
            if (viewportPos.x < -despawnMargin)
            {
                spawned.Dequeue();
                Destroy(go);
                Debug.Log($"[ChunkGen] Despawned chunk | endX: {endX:F1}");
            }
            else
            {
                break; // Queue theo thứ tự, còn lại chưa cần xóa
            }
        }
    }
}