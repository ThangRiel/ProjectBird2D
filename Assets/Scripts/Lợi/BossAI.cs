using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossAI : MonoBehaviour
{
    public Transform player;

    [Header("Laser Component")]
    public LineRenderer mainLaser;      
    public LineRenderer previewLaser;   
    public Transform firePoint;       

    [Header("Laser Settings")]
    [SerializeField] private float chargeDuration = 0.8f; 
    [SerializeField] private float laserDuration = 0.5f;  
    [SerializeField] private float laserMaxDistance = 30f; 

    [Header("Skill: Fire Rain (Phase 2 Upgrade)")]
    [SerializeField] private GameObject fireBallPrefab; 
    [SerializeField] private float spawnHeight = 8f;     
    [SerializeField] private float delayBetweenBalls = 0.4f; 

    [Header("Skill: Summon Enemy (Phase 2 Only)")]
    [SerializeField] private GameObject enemyPrefab; // Kéo Prefab quái Skeleton vào đây
    [SerializeField] private Transform summonPoint;  // Vị trí quái xuất hiện (nếu không có sẽ tự lấy vị trí Boss)
    private bool hasSummonedEnemy = false;           // Biến kiểm tra chỉ triệu hồi ĐÚNG 1 LẦN

    [Header("AI Loop Settings")]
    [SerializeField] private float timeBetweenAttacks = 5f; 

    [Header("Animation Setup")]
    public Animator animator;        
    [SerializeField] private string attackTriggerName = "attack"; 

    [Header("Boss HP System")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private Slider hpSlider; 
    private int currentHP;
    private bool isDead = false;
    private bool isAttacking = false; 

    // Biến trạng thái Phase 2
    private bool isPhase2 = false; 

    private void Start()
    {
        if (mainLaser != null) mainLaser.enabled = false;
        if (previewLaser != null) previewLaser.enabled = false;
        if (animator == null) animator = GetComponent<Animator>();

        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        StartCoroutine(AILoopRoutine());
    }

    private IEnumerator AILoopRoutine()
    {
        yield return new WaitForSeconds(3f);
        int attackIndex = 0; 

        while (!isDead)
        {
            if (player != null && !isAttacking)
            {
                isAttacking = true;

                // 🔥 KIỂM TRA TRƯỚC: Nếu đang ở Phase 2 và CHƯA từng triệu hồi quái
                if (isPhase2 && !hasSummonedEnemy)
                {
                    yield return StartCoroutine(SummonEnemyRoutine());
                }
                else
                {
                    // Vòng lặp đổi chiêu thức thông thường
                    if (attackIndex % 2 == 0)
                    {
                        yield return StartCoroutine(LaserAttackRoutine());
                    }
                    else
                    {
                        yield return StartCoroutine(FireRainRoutine());
                    }
                    attackIndex++;
                }

                isAttacking = false;
                yield return new WaitForSeconds(timeBetweenAttacks);
            }
            else
            {
                yield return null;
            }
        }
    }

    // CHIÊU 1: KHẠC LASER (Giữ nguyên)
    IEnumerator LaserAttackRoutine()
    {
        Debug.Log("📢 BOSS: Chuẩn bị khạc LASER!");
        if (animator != null) animator.SetTrigger(attackTriggerName);

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 playerCurrentPos = player.position;
        Vector3 direction = (playerCurrentPos - startPos).normalized;
        Vector3 finalTargetPosition = startPos + direction * laserMaxDistance; 

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

        mainLaser.enabled = true;
        bool damagedPlayer = false; 
        float laserElapsed = 0f;
        while (laserElapsed < laserDuration)
        {
            Vector3 currentStartPos = firePoint != null ? firePoint.position : transform.position;
            mainLaser.SetPosition(0, currentStartPos);
            mainLaser.SetPosition(1, finalTargetPosition);

            if (!damagedPlayer)
            {
                RaycastHit2D hitInfo = Physics2D.Raycast(currentStartPos, direction, laserMaxDistance);
                if (hitInfo.collider != null && hitInfo.collider.CompareTag("Player"))
                {
                    hitInfo.collider.SendMessage("TakeDamage", 2, SendMessageOptions.DontRequireReceiver);
                    damagedPlayer = true; 
                }
            }
            laserElapsed += Time.deltaTime;
            yield return null;
        }
        mainLaser.enabled = false;
    }

    // 🔥 CHIÊU 2: NÂNG CẤP THẢ FIREBALL (Phase 1: 3 quả | Phase 2: 4 quả)
    IEnumerator FireRainRoutine()
    {
        if (fireBallPrefab == null) yield break;

        // Tự động điều chỉnh số lượng dựa theo Phase
        int ballCount = isPhase2 ? 4 : 3;
        Debug.Log($"📢 BOSS: Triệu hồi MƯA THIÊN THẠCH! (Số lượng: {ballCount} quả)");

        if (animator != null) animator.SetTrigger(attackTriggerName);

        float targetPlayerX = player != null ? player.position.x : transform.position.x;

        for (int i = 0; i < ballCount; i++)
        {
            float offsetX = 0f;
            // Cấu hình vị trí rơi lệch nhẹ cho các quả cầu nối đuôi nhau
            if (i == 1) offsetX = -1.5f;
            if (i == 2) offsetX = 1.5f;
            if (i == 3) offsetX = -3.0f; // Quả thứ 4 (Phase 2) dội rộng hơn sang trái

            float finalSpawnX = targetPlayerX + offsetX;
            Vector3 spawnPosition = new Vector3(finalSpawnX, transform.position.y + spawnHeight, 0f);

            Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(delayBetweenBalls);
        }
    }

    // 🔥 CHIÊU 3: TRIỆU HỒI ĐÚNG 1 ENEMY (Chỉ chạy 1 lần duy nhất khi lên Phase 2)
    IEnumerator SummonEnemyRoutine()
    {
        hasSummonedEnemy = true; // Khóa ngay lập tức, không bao giờ dùng lại hàm này nữa
        Debug.Log("🔥 BOSS BẬT PHASE 2: Triệu hồi đệ tử Skeleton trợ chiến!");

        if (animator != null) animator.SetTrigger(attackTriggerName);

        if (enemyPrefab != null)
        {
            // Xác định điểm sinh quái (nếu chưa gán transform thì sinh ngay cạnh Boss)
            Vector3 spawnPos = summonPoint != null ? summonPoint.position : transform.position + new Vector3(-2f, 0f, 0f);
            
            // Tiến hành tạo ra 1 quái vật độc lập
            GameObject summonedEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            summonedEnemy.name = "Summoned_Skeleton"; 
        }
        else
        {
            Debug.LogWarning("❌ Chưa kéo Prefab Enemy (Skeleton) vào bảng quản lý của Boss!");
        }

        yield return new WaitForSeconds(1f); // Khựng nhẹ hành động tung chiêu gọi đệ
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); 

        if (hpSlider != null) hpSlider.value = currentHP;

        Debug.Log($"😱 Boss trúng đòn! Mất {damage} HP. Máu còn lại: {currentHP}/{maxHP}");

        // 🔥 LOGIC KIỂM TRA CHUYỂN PHASE 2 (Khi máu dưới hoặc bằng 30%)
        if (currentHP <= 30 && !isPhase2)
        {
            isPhase2 = true;
            Debug.LogWarning("⚠️⚠️⚠️ CẢNH BÁO: BOSS ĐÃ BƯỚC SANG PHASE 2! SỨC MẠNH TĂNG CAO!");
            // Tại đây bạn có thể đổi màu Boss hoặc bật hiệu ứng nổ phẫn nộ nếu có
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("💀 BOSS ĐÃ BỊ TIÊU DIỆT!");
        StopAllCoroutines();
        if (mainLaser != null) mainLaser.enabled = false;
        if (previewLaser != null) previewLaser.enabled = false;
        if (animator != null) animator.SetTrigger("die");
        Destroy(gameObject, 1.5f);
    }
}