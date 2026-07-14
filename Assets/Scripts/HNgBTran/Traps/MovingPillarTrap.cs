using UnityEngine;
// Bật tắt hitbox của MovingPillarTrap theo chu kỳ, dựa vào TrapConfigSO
public class MovingPillarTrap : MonoBehaviour
{
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private float extendedDuration = 1f; // thời gian ở trạng thái nguy hiểm (đỉnh)
    [SerializeField] private float cycleInterval = 1.5f;       // tổng thời gian 1 chu kỳ, khớp TrapConfigSO

    private void OnEnable()
    {
        hitbox.enabled = false;
        InvokeRepeating(nameof(Extend), 0f, cycleInterval);
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