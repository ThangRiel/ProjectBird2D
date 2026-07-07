using UnityEngine;

[CreateAssetMenu(fileName = "Trap_", menuName = "LevelDesign/Trap Config")]
public class TrapConfigSO : ScriptableObject
{
    public TrapType type;
    public float damage;
    public float slowMultiplier; // cho bẫy làm chậm, ví dụ 0.5 (còn 50% tốc độ)
    public float slowDuration; // cho bẫy làm chậm, ví dụ 1.5 (giây)
    public float cycleInterval;   // cho cột thụt lên/xuống, ví dụ 1.5 (giây/chu kỳ)
    public int iceLayerHits;      // cho cầu băng vỡ, ví dụ 2 (số lần đi qua trước khi vỡ)
    public float windForce; // cho bẫy gió, ví dụ 5 (lực đẩy)
}