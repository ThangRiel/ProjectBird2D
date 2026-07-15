using UnityEngine;
// Bật tắt hitbox của MovingPillarTrap theo chu kỳ, dựa vào TrapConfigSO
public class MovingPillarTrap : MonoBehaviour
{
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private float extendDelay = 0.6f;       // (A) thời điểm trong 1 vòng lặp cột bắt đầu nguy hiểm
    [SerializeField] private float extendedDuration = 0.6f; // (B - A) thời gian giữ trạng thái nguy hiểm
    [SerializeField] private float cycleInterval = 1.5f;    // tổng độ dài 1 vòng lặp animation

    private void OnEnable()
    {
        hitbox.enabled = false;
        InvokeRepeating(nameof(Extend), extendDelay, cycleInterval);
    }

    private void Extend()
    {
        hitbox.enabled = true;
        Invoke(nameof(Retract), extendedDuration);
    }

    private void Retract()
    {
        hitbox.enabled = false;
    }
}