using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//! đã sửa để có thể thêm nhiều zone, mỗi zone có thể có nhiều chunk prefab khác nhau. Khi player đi qua mốc startX của zone tiếp theo, sẽ chuyển sang zone đó và sinh chunk từ mảng prefabs của zone mới
// Khai báo class này để hiển thị được trong Inspector
[System.Serializable]
public class MapZone
{
    public string zoneName;
    public float zoneLength = 100f;                  // * MỚI: độ dài ước lượng của zone, để tính % tiến trình
    public float startX; //! Tọa độ X mà mốc này bắt đầu được sinh ra
    public ZoneConfigSO zoneConfig;   // MỚI: kéo asset Zone vào đây
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

    // ! === MỚI: để tính % tiến trình ===
    [Header("Peak stage")]
    [Range(0f, 1f)]
    public float peakTrapBiasChance = 0.7f; // % cơ hội ưu tiên chunk có gai/cột thụt ở peak

    [Header("Overlap/Gap Feature")]
    [Range(0f, 1f)] public float chunkOverlapChance = 0.1f;   // % cơ hội xảy ra hiệu ứng
    public float chunkOverlapMinOffset = 0f; // có thể cho âm = chồng lên chunk trước
    public float chunkOverlapMaxOffset = 3f;  // dương = cách xa ra thêm (khoảng nghỉ)

    private int chunkSpawnIndex = 0;
    private const int SORTING_ORDER_STEP = 100; // phải LỚN HƠN khoảng order nội bộ đang dùng trong 1 chunk

    // ! === END MỚI ===

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

    // ! MỚI: để tính % tiến trình
    private float zoneStartX;
    private readonly Queue<string> recentChunkIds = new Queue<string>();
    private int consecutiveHardCount = 0;
    private string lastLoggedStage = null; // theo gõi stage hiện tại để log khi chuyển stage
    // Const mới
    private const float EASY_DIFFICULTY_THRESHOLD = 1f; // "dễ" = difficultyRating <= 1
    private const float HARD_DIFFICULTY_THRESHOLD = 2.5f;
    private const int MAX_HARD_STREAK = 2; // sau 2 chunk khó liên tiếp -> chèn 1 chunk nghỉ



    void Start()
    {
        if (cam == null) cam = Camera.main;
        lastEndX = player != null ? player.position.x : 0f;
        zoneStartX = lastEndX; //* MỚI

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

            zoneStartX = lastEndX;        //* MỚI: reset mốc % tiến trình cho zone mới
            recentChunkIds.Clear();       //* MỚI: qua zone mới thì quên lịch sử no-repeat của zone cũ
            lastLoggedStage = null; // MỚI: qua zone mới thì reset log stage

            Debug.Log($"[ChunkGen] Chuyển sang zone: {zones[currentZoneIndex].zoneName}");

            if (lastEndX >= testGapX)
            {
                cam.orthographicSize += zoom;
                Debug.Log($"[ChunkGen] Camera orthographicSize tăng lên: {cam.orthographicSize}");
            }
        }



        // Sinh map từ mảng prefabs của Zone hiện tại
        MapZone zone = zones[currentZoneIndex];
        if (zone.zoneConfig == null)
        {
            Debug.LogWarning($"[ChunkGen] Zone '{zone.zoneName}' CHƯA gán zoneConfig!");
            return;
        }
        if (zone.zoneConfig.chunkPool == null || zone.zoneConfig.chunkPool.Count == 0)
        {
            Debug.LogWarning($"[ChunkGen] Zone '{zone.zoneName}' có chunkPool rỗng!");
            return;
        }

        ChunkDataSO chosenChunk = PickNextChunk(zone);   // ! thay Random.Range prefab thô bằng thuật toán
        GameObject prefab = chosenChunk.chunkPrefab;
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        Transform sp = go.transform.Find("StartPoint");
        Transform ep = go.transform.Find("EndPoint");

        if (sp == null || ep == null)
        {
            Debug.LogError($"[ChunkGen] Prefab '{prefab.name}' thiếu StartPoint hoặc EndPoint.");
            Destroy(go);
            return;
        }
        Debug.Log($"[ChunkGen] Spawn '{prefab.name}' | sp.localPos.x={sp.localPosition.x:F2} ep.localPos.x={ep.localPosition.x:F2} scale={go.transform.localScale}");

        float extraOffset = 0f;
        if (Random.value < chunkOverlapChance)
        {
            extraOffset = Random.Range(chunkOverlapMinOffset, chunkOverlapMaxOffset);
            Debug.Log($"[ChunkGen] Overlap/Gap feature kích hoạt trên '{prefab.name}': offset thêm {extraOffset:F2}");
        }

        float offsetX = lastEndX + gapX + extraOffset - sp.position.x;
        go.transform.position = new Vector3(go.transform.position.x + offsetX, lockY, 0f);

        lastEndX = ep.position.x;
        spawned.Enqueue((go, lastEndX));

        ApplyChunkSortingOrder(go, chunkSpawnIndex);
        chunkSpawnIndex++;
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


    ChunkDataSO PickNextChunk(MapZone zone)
    {
        ZoneConfigSO config = zone.zoneConfig;
        ChunkDataSO[] fullPool = config.chunkPool.ToArray();

        float progress = Mathf.Clamp01((lastEndX - zoneStartX) / zone.zoneLength);
        bool isEarlyStage = progress < config.easyOnlyProgress;
        bool isPeakStage = progress >= config.peakStageProgress;
        // còn lại (không early, không peak) = mid stage


        // ! Log stage khi chuyển giai đoạn
        string stageName = isEarlyStage ? "EASY" : (isPeakStage ? "PEAK" : "MID");
        if (stageName != lastLoggedStage)
        {
            Debug.Log($"[ChunkGen] Zone '{zone.zoneName}' chuyển sang giai đoạn: {stageName} (progress={progress:F2})");
            lastLoggedStage = stageName;
        }


        List<ChunkDataSO> candidates;

        if (isEarlyStage)
        {
            // Early: chỉ level 0 + 1
            candidates = SelectWithFallback(fullPool, c => c.difficultyRating <= 1f);
        }
        else if (isPeakStage)
        {
            // Peak: random level 1+, nhưng ưu tiên chunk có gai / cột thụt nhiều hơn
            var spikeOrPillarPool = fullPool.Where(c =>
                c.difficultyRating >= 1f &&
                c.containedTraps != null &&
                c.containedTraps.Any(t => t == TrapType.FixedSpike || t == TrapType.MovingPillar)).ToArray();

            if (spikeOrPillarPool.Length > 0 && Random.value < peakTrapBiasChance)
                candidates = SelectWithFallback(spikeOrPillarPool, c => true);
            else
                candidates = SelectWithFallback(fullPool, c => c.difficultyRating >= 1f);
        }
        else
        {
            // Mid: random level 1 trở đi
            candidates = SelectWithFallback(fullPool, c => c.difficultyRating >= 1f);
        }

        ChunkDataSO chosen = candidates[Random.Range(0, candidates.Count)];
        RegisterChunk(zone, chosen.chunkId);
        return chosen;

    }

    // Ưu tiên 1: đúng độ khó + chưa lặp gần đây
    // Ưu tiên 2: đúng độ khó, chấp nhận lặp lại (thay vì bỏ luôn độ khó)
    // Cuối cùng mới bỏ điều kiện độ khó, và log cảnh báo để biết pool đang thiếu chunk loại đó
    List<ChunkDataSO> SelectWithFallback(ChunkDataSO[] pool, System.Func<ChunkDataSO, bool> difficultyFilter)
    {
        var matchingDifficulty = pool.Where(difficultyFilter).ToList();

        var freshMatches = matchingDifficulty.Where(c => !recentChunkIds.Contains(c.chunkId)).ToList();
        if (freshMatches.Count > 0) return freshMatches;

        if (matchingDifficulty.Count > 0) return matchingDifficulty;

        Debug.LogWarning("[ChunkGen] Không có chunk nào khớp độ khó yêu cầu trong pool hiện tại (đã loại level 0 nếu ở đoạn sau) -> dùng toàn bộ pool khả dụng.");
        var anyFresh = pool.Where(c => !recentChunkIds.Contains(c.chunkId)).ToList();
        return anyFresh.Count > 0 ? anyFresh : pool.ToList();
    }
    void RegisterChunk(MapZone zone, string chunkId)
    {
        recentChunkIds.Enqueue(chunkId);
        while (recentChunkIds.Count > zone.zoneConfig.noRepeatWindow)
        {
            recentChunkIds.Dequeue();
        }
    }

    void ApplyChunkSortingOrder(GameObject chunkRoot, int spawnIndex)
    {
        int offset = spawnIndex * SORTING_ORDER_STEP;
        foreach (var rend in chunkRoot.GetComponentsInChildren<Renderer>(true))
        {
            rend.sortingOrder += offset;
        }
    }
}