using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Zone_", menuName = "LevelDesign/Zone Config")]
public class ZoneConfigSO : ScriptableObject
{
    public ZoneType zoneType; // Loại zone, địa hình (enum: IcyZone, RockyMossZone, LavaZone, PlainsZone)
    public List<ChunkDataSO> chunkPool; // Danh sách các ChunkDataSO thuộc zone này
    // public AnimationCurve difficultyOverDistance; // Đường cong: trục X = % khoảng cách đã đi, trục Y = % độ khó mục tiêu tại điểm đó
    public int noRepeatWindow = 3; // Khoảng cách tối thiểu giữa hai chunk giống nhau
    public float easyOnlyProgress = 0.2f; // Phần trăm khoảng cách mà chỉ xuất hiện các chunk dễ
    public float peakStageProgress = 0.7f; // MỚI: mốc % bắt đầu giai đoạn peak
    public float weatherEffectStartProgress = 0.7f; // Phần trăm khoảng cách mà hiệu ứng thời tiết bắt đầu xuất hiện
    public AnimationCurve windForceCurve; // Đường cong: trục X = % khoảng cách đã đi, trục Y = lực gió tại điểm đó
}