using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkTilemapGenerator : MonoBehaviour
{
    [Header("Refs")]
    public Transform worldRoot;          // object bạn đang dịch trái/phải
    public Tilemap groundTilemap;

    [Header("Tiles")]
    public TileBase groundTile;

    [Header("Generation")]
    public int chunkWidth = 16;          // 16 tiles / chunk
    public int groundY = -2;             // hàng Y đặt ground
    public int preGenerateChunks = 6;    // sinh sẵn ban đầu
    public int keepChunksBehind = 4;     // giữ lại phía sau để không xoá sớm

    private int nextChunkStartX = 0;
    private readonly Queue<int> chunkStarts = new Queue<int>();

    void Start()
    {
        for (int i = 0; i < preGenerateChunks; i++)
            GenerateNextChunk();
    }

    void Update()
    {
        // Vì bạn "đi tới" bằng cách kéo worldRoot sang trái,
        // tiến độ theo trục X sẽ là -worldRoot.position.x
        float progressX = -worldRoot.position.x;

        // Nếu đã gần tới vùng chunk tiếp theo thì sinh thêm
        if (progressX > nextChunkStartX - chunkWidth * 2)
        {
            GenerateNextChunk();
            CleanupOldChunks(progressX);
        }
    }

    void GenerateNextChunk()
    {
        int startX = nextChunkStartX;

        // random gap nhỏ (0..2) để bắt đầu có “platformer feeling”
        int gapStart = Random.Range(4, chunkWidth - 4);
        int gapLen = Random.Range(0, 3);

        for (int x = 0; x < chunkWidth; x++)
        {
            bool isGap = (x >= gapStart && x < gapStart + gapLen);
            if (!isGap)
            {
                groundTilemap.SetTile(new Vector3Int(startX + x, groundY, 0), groundTile);
            }
        }

        chunkStarts.Enqueue(startX);
        nextChunkStartX += chunkWidth;
    }

    void CleanupOldChunks(float progressX)
    {
        // chunk hiện tại mà "camera" đang ở gần
        int currentChunkIndex = Mathf.FloorToInt(progressX / chunkWidth);
        int minChunkToKeep = currentChunkIndex - keepChunksBehind;

        while (chunkStarts.Count > 0)
        {
            int startX = chunkStarts.Peek();
            int chunkIndex = startX / chunkWidth;

            if (chunkIndex >= minChunkToKeep) break;

            // xoá chunk cũ
            for (int x = 0; x < chunkWidth; x++)
            {
                groundTilemap.SetTile(new Vector3Int(startX + x, groundY, 0), null);
            }
            chunkStarts.Dequeue();
        }
    }
}