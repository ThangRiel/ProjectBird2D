using UnityEngine;

public class LightningTrap : MonoBehaviour
{
    [Header("Chu kỳ (giây)")]
    public float delayBeforeOn = 0.5f;
    public float activeDuration = 0.3f;
    public float restDuration = 1.0f;

    public Collider2D strikeCollider;

    enum Phase { WaitingToTurnOn, Active, Resting }
    Phase phase;
    float timer;

    void Start()
    {
        EnterPhase(Phase.WaitingToTurnOn);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // while (không phải if) để chuyển hết các phase có duration = 0 ngay trong cùng 1 frame,
        // không để collider bị "lọt" bật lên dù chỉ 1 frame.
        int safety = 0;
        while (timer <= 0f && safety < 10)
        {
            switch (phase)
            {
                case Phase.WaitingToTurnOn: EnterPhase(Phase.Active); break;
                case Phase.Active: EnterPhase(Phase.Resting); break;
                case Phase.Resting: EnterPhase(Phase.WaitingToTurnOn); break;
            }
            safety++;
        }
    }

    void EnterPhase(Phase newPhase)
    {
        phase = newPhase;
        switch (phase)
        {
            case Phase.WaitingToTurnOn:
                if (strikeCollider != null) strikeCollider.enabled = false;
                timer = delayBeforeOn;
                break;
            case Phase.Active:
                // Chỉ thực sự bật khi activeDuration > 0 — chặn hẳn trường hợp = 0
                if (strikeCollider != null) strikeCollider.enabled = activeDuration > 0f;
                timer = activeDuration;
                break;
            case Phase.Resting:
                if (strikeCollider != null) strikeCollider.enabled = false;
                timer = restDuration;
                break;
        }
    }
}