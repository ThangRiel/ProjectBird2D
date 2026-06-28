using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    public Transform player;

    [Header("Laser Component")]
    public LineRenderer mainLaser;      // Kéo GameObject con MainLaser vào đây
    public LineRenderer previewLaser;   // Kéo GameObject con PreviewLaser vào đây
    public Transform firePoint;       

    [Header("Laser Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float chargeDuration = 0.8f; // Thời gian hiện tia dự đoán màu mảnh
    [SerializeField] private float laserDuration = 0.5f;  // Thời gian tia laser sát thương tồn tại
    [SerializeField] private float laserMaxDistance = 30f; // Độ dài tia bắn ra

    [Header("Animation Setup")]
    public Animator animator;        
    [SerializeField] private string attackTriggerName = "attack"; 

    private float nextTime;

    private void Start()
    {
        if (mainLaser != null) mainLaser.enabled = false;
        if (previewLaser != null) previewLaser.enabled = false;
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null) return;

        if (Time.time >= nextTime)
        {
            StartCoroutine(LaserAttackRoutine());
            nextTime = Time.time + attackCooldown;
        }
    }

    IEnumerator LaserAttackRoutine()
    {
        if (animator != null) animator.SetTrigger(attackTriggerName);

        // 1. KHÓA TỌA ĐỘ: Chốt luôn vị trí Player đứng tại thời điểm bắt đầu sạc chiêu
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 playerCurrentPos = player.position;
        
        Vector3 direction = (playerCurrentPos - startPos).normalized;
        Vector3 finalTargetPosition = startPos + direction * laserMaxDistance; 

        // 2. HIỆN TIA DỰ BÁO: Bật tia chỉ định mảnh, găm cố định điểm ngắm cho Player nhìn để né
        previewLaser.enabled = true;

        float chargeElapsed = 0f;
        while (chargeElapsed < chargeDuration)
        {
            Vector3 currentStartPos = firePoint != null ? firePoint.position : transform.position;
            previewLaser.SetPosition(0, currentStartPos);
            previewLaser.SetPosition(1, finalTargetPosition);

            chargeElapsed += Time.deltaTime;
            yield return null;
        }

        previewLaser.enabled = false;

        // 3. KHẠC LASER CHÍNH: Nã tia laser sát thương cực mạnh vào đúng vị trí đã chốt chặn
        mainLaser.enabled = true;

        float laserElapsed = 0f;
        while (laserElapsed < laserDuration)
        {
            Vector3 currentStartPos = firePoint != null ? firePoint.position : transform.position;
            mainLaser.SetPosition(0, currentStartPos);
            mainLaser.SetPosition(1, finalTargetPosition);

            laserElapsed += Time.deltaTime;
            yield return null;
        }

        mainLaser.enabled = false;
    }

    // Hàm nhận sát thương được gọi từ Player khi lướt trúng
    public void TakeDamage(int damage)
    {
        Debug.Log("😱 Boss đã nhận " + damage + " sát thương từ cú lao kích của Player!");
        // Viết thêm logic trừ thanh máu hoặc xử lý chết của Boss tại đây
    }
}