using System.Collections.Generic;
using UnityEngine;
//! đã sửa để có thể thêm nhiều zone, mỗi zone có thể có nhiều chunk prefab khác nhau. Khi player đi qua mốc startX của zone tiếp theo, sẽ chuyển sang zone đó và sinh chunk từ mảng prefabs của zone mới
// Khai báo class này để hiển thị được trong Inspector
[System.Serializable]
public class MapZone
{
    public string zoneName;
    public float startX; //! Tọa độ X mà mốc này bắt đầu được sinh ra
    public GameObject[] chunkPrefabs;
}

public class ChunkLevelGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera cam;
    public float testGapX = 0f; // Khoảng cách giữa các chunk, dùng để test khi chuyển zone
    public float zoom = 1f;
    [Header("Zones Setup")]
    public MapZone[] zones; // Kéo các mảng map vào đây
    private int currentZoneIndex = 0;

    [Header("Spawn")]
    public int prewarmCount = 5;
    public float spawnAhead = 20f;

    [Header("Align")]
    public float lockY = 0f;

    [Header("Despawn")]
    public float despawnMargin = 2f;

    [Header("Spacing")]
    public float gapX = 0f;

    private float lastEndX;
    private readonly Queue<(GameObject go, float endX)> spawned = new Queue<(GameObject go, float endX)>();

    void Start()
    {
        if (cam == null) cam = Camera.main;
        lastEndX = player != null ? player.position.x : 0f;

        for (int i = 0; i < prewarmCount; i++)
            SpawnNext();
    }

    void Update()
    {
        if (player == null) return;
        if (cam == null) cam = Camera.main;

        if (player.position.x > lastEndX - spawnAhead)
            SpawnNext();

        DespawnOffscreen();
    }

    void SpawnNext()
    {
        if (zones == null || zones.Length == 0) return;

        // Check xem lastEndX đã vượt ngưỡng của Zone tiếp theo chưa
        if (currentZoneIndex < zones.Length - 1 && lastEndX >= zones[currentZoneIndex + 1].startX)
        {
            currentZoneIndex++;
            Debug.Log($"[ChunkGen] Chuyển sang zone: {zones[currentZoneIndex].zoneName}");

            if (lastEndX >= testGapX)
            {
                cam.orthographicSize += zoom;
                Debug.Log($"[ChunkGen] Camera orthographicSize tăng lên: {cam.orthographicSize}");
            }
        }



        // Sinh map từ mảng prefabs của Zone hiện tại
        GameObject[] currentPrefabs = zones[currentZoneIndex].chunkPrefabs;
        if (currentPrefabs == null || currentPrefabs.Length == 0) return;

        GameObject prefab = currentPrefabs[Random.Range(0, currentPrefabs.Length)];
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        Transform sp = go.transform.Find("StartPoint");
        Transform ep = go.transform.Find("EndPoint");

        if (sp == null || ep == null)
        {
            Debug.LogError($"[ChunkGen] Prefab '{prefab.name}' thiếu StartPoint hoặc EndPoint.");
            Destroy(go);
            return;
        }

        float offsetX = lastEndX + gapX - sp.position.x;
        go.transform.position = new Vector3(go.transform.position.x + offsetX, lockY, 0f);

        lastEndX = ep.position.x;
        spawned.Enqueue((go, lastEndX));
    }

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

            Vector3 viewportPos = cam.WorldToViewportPoint(new Vector3(endX, 0f, 0f));
            if (viewportPos.x < -despawnMargin)
            {
                spawned.Dequeue();
                Destroy(go);
            }
            else
            {
                break;
            }
        }
    }
}