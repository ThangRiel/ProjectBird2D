using UnityEngine;
using System.Collections.Generic;

// ? Khiến Unity hiện thêm mục trong menu Assets -> Create -> LevelDesign -> Chunk Data
[CreateAssetMenu(fileName = "Chunk_", menuName = "LevelDesign/Chunk Data")]
public class ChunkDataSO : ScriptableObject // : ScriptableObject để Unity biết nó là một asset, không phải component gắn vào GameObject
{
    public string chunkId; // ID duy nhất của chunk, e.g.: "IceA", "IceB" -> để thuật toán spawn chunk biết được chunk nào vừa được spawn
    public GameObject chunkPrefab; // trỏ tới Prefab của chunk
    public ZoneType zone; // Vùng mà chunk thuộc về (enum)
    [Range(0, 5)] public int difficultyRating; // Đánh giá độ khó của chunk
    public float chunkLength; // Độ dài của chunk
    public List<TrapType> containedTraps; // Danh sách các bẫy chứa trong chunk
    public int enemySlotCount; // Số lượng vị trí enemy trong chunk
    [TextArea] public string designNotes;
}
