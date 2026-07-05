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

    [Header("Skill: Fire Rain (New)")]
    [SerializeField] private GameObject fireBallPrefab; // Kéo Prefab quả cầu lửa vào đây
    [SerializeField] private float spawnHeight = 8f;     // Độ cao xuất phát của quả cầu lửa so với Boss
    [SerializeField] private float spawnRangeX = 10f;    // Phạm vi chiều ngang quanh Boss mà cầu lửa có thể rơi
    [SerializeField] private float delayBetweenBalls = 0.4f; // Độ trễ giữa mỗi quả cầu lửa rơi xuống

    [Header("AI Loop Settings")]
    [SerializeField] private float timeBetweenAttacks = 5f; // Độ trễ 5 giây giữa các lần dùng chiêu

    [Header("Animation Setup")]
    public Animator animator;        
    [SerializeField] private string attackTriggerName = "attack"; 

    [Header("Boss HP System")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private Slider hpSlider; 
    private int currentHP;
    private bool isDead = false;
    private bool isAttacking = false; // Trạng thái đang thực hiện chiêu

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

        // Bắt đầu vòng lặp tấn công tự động của Boss
        StartCoroutine(AILoopRoutine());
    }

    // 🔥 VÒNG LẶP ĐIỀU KHIỂN AI CHIÊU THỨC XEN KẼ ĐỘ TRỄ 5 GIÂY
    private IEnumerator AILoopRoutine()
    {
        // Chờ 3 giây đầu game rồi mới bắt đầu đánh đợt 1
        yield return new WaitForSeconds(3f);

        int attackIndex = 0; // Dùng để hoán đổi chiêu thức tuần tự

        while (!isDead)
        {
            if (player != null && !isAttacking)
            {
                isAttacking = true;

                // Hoán đổi chiêu thức luân phiên
                if (attackIndex % 2 == 0)
                {
                    yield return StartCoroutine(LaserAttackRoutine());
                }
                else
                {
                    yield return StartCoroutine(FireRainRoutine());
                }

                attackIndex++;
                isAttacking = false;

                // 🔥 KHÓA TRỄ ĐÚNG 5 GIÂY CHO PLAYER NÉ TRƯỚC KHI BẮT ĐẦU CHIÊU TIẾP THEO
                yield return new WaitForSeconds(timeBetweenAttacks);
            }
            else
            {
                yield return null;
            }
        }
    }

    // CHIÊU 1: KHẠC LASER
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
                    Debug.Log("⚡ TIA LASER CỦA BOSS ĐÃ THIÊU ĐỐT PLAYER!");
                }
            }

            laserElapsed += Time.deltaTime;
            yield return null;
        }

        mainLaser.enabled = false;
    }

   // 🔥 CHIÊU 2: TRIỆU HỒI 3 QUẢ CẦU LỬA NHẮM VÀO VỊ TRÍ PLAYER
    IEnumerator FireRainRoutine()
    {
        Debug.Log("📢 BOSS: Triệu hồi MƯA THIÊN THẠCH nhắm vào Player!");
        
        if (animator != null) animator.SetTrigger(attackTriggerName);

        if (fireBallPrefab == null)
        {
            Debug.LogWarning("❌ Chưa kéo Prefab quả cầu lửa vào Boss!");
            yield break;
        }

        // 🎯 KHÓA VỊ TRÍ: Lấy chính xác tọa độ X của Player ngay tại khung hình này
        float targetPlayerX = transform.position.x; // Phòng hờ nếu Player biến mất
        if (player != null)
        {
            targetPlayerX = player.position.x;
        }

        // Gọi đúng 3 quả cầu lửa dội xuống vị trí vừa khóa
        for (int i = 0; i < 3; i++)
        {
            // Quả 1 rơi trúng tim, quả 2 và 3 lệch nhẹ sang trái/phải một chút để tạo vùng nổ lan rộng
            float offsetX = 0f;
            if (i == 1) offsetX = -1.5f; // Quả thứ 2 lệch sang trái 1.5 mét
            if (i == 2) offsetX = 1.5f;  // Quả thứ 3 lệch sang phải 1.5 mét

            float finalSpawnX = targetPlayerX + offsetX;
            
            // Tọa độ xuất phát từ trên trời (Y cao hơn vị trí Boss 8 mét) dọc theo trục X của Player
            Vector3 spawnPosition = new Vector3(finalSpawnX, transform.position.y + spawnHeight, 0f);

            // Sinh ra quả cầu lửa
            GameObject fireBall = Instantiate(fireBallPrefab, spawnPosition, Quaternion.identity);
            
            Debug.Log($"☄️ Quả cầu lửa thứ {i + 1} đang dội xuống vị trí cũ của Player (X: {finalSpawnX})");

            // Chờ 0.4 giây trước khi thả quả tiếp theo
            yield return new WaitForSeconds(delayBetweenBalls);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); 

        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        Debug.Log($"😱 Boss trúng đòn! Mất {damage} HP. Máu còn lại: {currentHP}/{maxHP}");

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