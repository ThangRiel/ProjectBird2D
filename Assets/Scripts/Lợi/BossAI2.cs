using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossAI2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Laser Component")]
    public LineRenderer mainLaser;
    public LineRenderer previewLaser;
    public Transform firePoint;

    [Header("Laser Settings")]
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private float laserDuration = 0.5f;
    [SerializeField] private float laserMaxDistance = 30f;

    [Header("Fire Rain (Glow Animation)")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float delayBetweenBalls = 0.4f;
    [SerializeField] private float fireRainRange = 10f;

    [Header("Summon Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform summonPoint;

    private bool hasSummonedEnemy = false;

    [Header("AI Settings")]
    [SerializeField] private float timeBetweenAttacks = 3.5f;
    [SerializeField] private float detectRange = 15f;

    [Header("Animation")]
    public Animator animator;

    [Header("Boss HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private Slider hpSlider;

    private int currentHP;
    private bool isDead;
    private bool isAttacking;
    private bool isPhase2;

    private void Start()
    {
        if (mainLaser != null) mainLaser.enabled = false;
        if (previewLaser != null) previewLaser.enabled = false;

        if (animator == null)
            animator = GetComponent<Animator>();

        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        StartCoroutine(AILoopRoutine());
    }

    private void Update()
    {
        if (player == null || isDead) return;

        // Chỉ lật mặt Boss khi đang KHÔNG thực hiện kỹ năng để cố định hướng bắn
        if (!isAttacking)
        {
            Vector3 scale = transform.localScale;
            if (player.position.x > transform.position.x)
                scale.x = Mathf.Abs(scale.x);
            else
                scale.x = -Mathf.Abs(scale.x);

            transform.localScale = scale;
        }
    }

    IEnumerator AILoopRoutine()
    {
        yield return new WaitForSeconds(2f);

        int attackIndex = 0;

        while (!isDead)
        {
            if (player == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector2.Distance(transform.position, player.position);

            // Nếu player ngoài tầm phát hiện thì đứng im tại chỗ
            if (distance > detectRange)
            {
                yield return null;
                continue;
            }

            if (isAttacking)
            {
                yield return null;
                continue;
            }

            isAttacking = true;

            // Chuyển Phase 2: Ưu tiên gọi đệ trước khi tiếp tục chuỗi chiêu thức
            if (isPhase2 && !hasSummonedEnemy)
            {
                yield return StartCoroutine(SummonEnemyRoutine());
            }
            else
            {
                // Luân phiên thay đổi kỹ năng dựa theo Animator Parameters của bạn
                if (attackIndex % 2 == 0)
                {
                    yield return StartCoroutine(LaserAttackRoutine());
                }
                else
                {
                    if (distance <= fireRainRange)
                    {
                        yield return StartCoroutine(FireRainRoutine());
                    }
                    else
                    {
                        // Nếu quá xa tầm mưa lửa, tự động đổi sang bắn Laser
                        yield return StartCoroutine(LaserAttackRoutine());
                    }
                }

                attackIndex++;
            }

            isAttacking = false;
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }
    //====================================
    // TRIGGER: Laser (ĐÃ SỬA: KHÓA HƯỚNG TỪ ĐẦU ĐỂ PLAYER NÉ ĐƯỢC)
    //====================================
    IEnumerator LaserAttackRoutine()
    {
        Debug.Log("📢 Boss dùng chiêu: Laser");

        if (animator != null)
            animator.SetTrigger("Laser");

        // 1. CHUẨN BOSSAI: Khóa cứng hướng bắn và điểm đích ngay tại thời điểm bắt đầu gồng chiêu
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetCenter = player.position + new Vector3(0, 0.5f, 0); // Nhắm vào giữa người player lúc đó
        Vector3 direction = (targetCenter - startPos).normalized;
        Vector3 targetPos = startPos + direction * laserMaxDistance;

        previewLaser.enabled = true;
        float timer = 0f;

        // 2. Giai đoạn gồng ngắm tĩnh: Tia preview giữ nguyên vị trí cố định để báo trước vùng nguy hiểm
        while (timer < chargeDuration)
        {
            if (player == null || isDead) break;

            // Vị trí gốc của Boss có thể dịch chuyển nhẹ theo animation, nhưng điểm đích targetPos đã bị khóa cứng
            Vector3 currentStart = firePoint != null ? firePoint.position : transform.position;

            previewLaser.SetPosition(0, currentStart);
            previewLaser.SetPosition(1, targetPos);

            timer += Time.deltaTime;
            yield return null;
        }

        previewLaser.enabled = false;
        mainLaser.enabled = true;

        bool damaged = false;
        timer = 0f;

        // 3. Giai đoạn bắn thật vào vị trí đã khóa
        while (timer < laserDuration)
        {
            if (isDead) break;

            Vector3 currentStart = firePoint != null ? firePoint.position : transform.position;

            mainLaser.SetPosition(0, currentStart);
            mainLaser.SetPosition(1, targetPos);

            if (!damaged && player != null)
            {
                // Bắn tia Raycast quét dọc theo hướng đã khóa từ đầu
                RaycastHit2D hit = Physics2D.Raycast(currentStart, direction, laserMaxDistance);

                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    hit.collider.SendMessage("TakeDamage", 2, SendMessageOptions.DontRequireReceiver);
                    damaged = true; // Chỉ tính sát thương 1 lần trong suốt loạt bắn
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        mainLaser.enabled = false;
    }

    //====================================
    // TRIGGER: Glow (Dùng làm hiệu ứng gọi mưa lửa)
    //====================================
    IEnumerator FireRainRoutine()
    {
        if (fireBallPrefab == null || player == null) yield break;

        Debug.Log("📢 Boss dùng chiêu: Fire Rain");

        if (animator != null)
            animator.SetTrigger("Glow"); // Sử dụng trigger Glow để làm động tác gồng sấm sét/mưa lửa

        // Chờ 0.5 giây cho animation gồng lên rồi mới bắt đầu rơi thiên thạch xuống
        yield return new WaitForSeconds(0.5f);

        int ballCount = isPhase2 ? 4 : 3;
        float targetX = player.position.x;

        for (int i = 0; i < ballCount; i++)
        {
            float offset = 0f;
            if (i == 1) offset = -1.5f;
            if (i == 2) offset = 1.5f;
            if (i == 3) offset = -3f;

            Vector3 spawnPos = new Vector3(
                targetX + offset,
                transform.position.y + spawnHeight,
                0
            );

            Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(delayBetweenBalls);
        }
    }

    //====================================
    // TRIGGER: Shoot (Dùng khi Boss kích hoạt Phase 2 để gọi đệ)
    //====================================

     IEnumerator SummonEnemyRoutine()
    {
        hasSummonedEnemy = true;

        Debug.Log("🔥 Boss triệu hồi Skeleton!");

        if (animator != null)
            animator.SetTrigger("Shoot");

        if (enemyPrefab != null)
        {
            Vector3 spawnPos =
                summonPoint != null
                ? summonPoint.position
                : transform.position + Vector3.left * 2f;

            GameObject enemy =
                Instantiate(
                    enemyPrefab,
                    spawnPos,
                    Quaternion.identity);

            enemy.name = "Summoned_Skeleton";
        }

        yield return new WaitForSeconds(1f);
    }

    //====================================
    // TAKE DAMAGE
    //====================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        Debug.Log($"Boss HP: {currentHP}/{maxHP}");

        if (!isPhase2 && currentHP <= maxHP * 0.3f)
        {
            isPhase2 = true;
            Debug.LogWarning("⚠️ Boss chuyển sang Phase 2!");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    //====================================
    // TRIGGER: Death / bool: IsDead
    //====================================
    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        if (mainLaser != null) mainLaser.enabled = false;
        if (previewLaser != null) previewLaser.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
            animator.SetBool("IsDead", true); // Bật cả Trigger và Bool theo hệ thống biến của bạn
        }

        Debug.Log("💀 Boss đã bị tiêu diệt!");
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, fireRainRange);
    }
}