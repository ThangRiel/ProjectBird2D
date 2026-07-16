using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossAI : BossBase
{

    [Header("Melee")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float meleeRange = 3f;
    [SerializeField] private int meleeDamage = 3;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float meleeHitDelay = 0.35f;
    public Transform player;

    [Header("Laser Component")]
    public LineRenderer mainLaser;
    public LineRenderer previewLaser;
    public Transform firePoint;

    [Header("Laser Settings")]
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private float laserDuration = 0.5f;
    [SerializeField] private float laserMaxDistance = 30f;

    [SerializeField] private int laserDamage = 10;

    [Header("Fire Rain")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float delayBetweenBalls = 0.4f;
    [SerializeField] private float fireRainRange = 7f;

    [SerializeField] private int fireRainDamage = 5;

    [Header("Summon Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform summonPoint;

    private bool hasSummonedEnemy = false;

    [Header("AI")]
    [SerializeField] private float timeBetweenAttacks = 5f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private string attackTriggerName = "attack";

    [Header("Boss HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private Slider hpSlider;

    private int currentHP;

    private bool isDead;
    private bool isAttacking;
    private bool isPhase2;

    private void Start()
    {
        if (mainLaser != null)
            mainLaser.enabled = false;

        if (previewLaser != null)
            previewLaser.enabled = false;

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

    IEnumerator AILoopRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (!isDead)
        {
            if (player == null)
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

            // Phase 2 chỉ triệu hồi 1 lần
            if (isPhase2 && !hasSummonedEnemy)
            {
                yield return StartCoroutine(SummonEnemyRoutine());
            }
            else
            {
                float distance = Vector2.Distance(transform.position, player.position);

                // Player đứng gần -> luôn đánh cận
                if (distance <= meleeRange)
                {
                    yield return StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    // Player ở xa -> ngẫu nhiên Laser hoặc Fire Rain
                    int randomAttack = Random.Range(0, 2);

                    if (randomAttack == 0)
                    {
                        yield return StartCoroutine(LaserAttackRoutine());
                    }
                    else
                    {
                        yield return StartCoroutine(FireRainRoutine());
                    }
                }
            }

            isAttacking = false;

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    //====================================
    // LASER ATTACK
    //====================================
    IEnumerator LaserAttackRoutine()
    {
        Debug.Log("📢 Boss: Laser!");

        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        Vector3 startPos =
            firePoint != null
            ? firePoint.position
            : transform.position;

        Vector3 direction =
            (player.position - startPos).normalized;

        Vector3 targetPos =
            startPos +
            direction * laserMaxDistance;

        previewLaser.enabled = true;

        float timer = 0f;

        while (timer < chargeDuration)
        {
            Vector3 currentStart =
                firePoint != null
                ? firePoint.position
                : transform.position;

            previewLaser.SetPosition(0, currentStart);
            previewLaser.SetPosition(1, targetPos);

            timer += Time.deltaTime;
            yield return null;
        }

        previewLaser.enabled = false;

        mainLaser.enabled = true;

        bool damaged = false;
        timer = 0f;

        while (timer < laserDuration)
        {
            Vector3 currentStart =
                firePoint != null
                ? firePoint.position
                : transform.position;

            mainLaser.SetPosition(0, currentStart);
            mainLaser.SetPosition(1, targetPos);

            if (!damaged)
            {
                RaycastHit2D hit =
    Physics2D.Raycast(
        currentStart,
        direction,
        laserMaxDistance);

                // Thêm đoạn này để debug
                if (hit.collider != null)
                {
                    LoiPlayer playerScript = hit.collider.GetComponent<LoiPlayer>();

                    if (playerScript != null)
                    {
                        playerScript.TakeDamage(laserDamage);

                        Debug.Log("Laser Damage : " + laserDamage);

                        damaged = true;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        mainLaser.enabled = false;
    }

    //====================================
    // FIRE RAIN
    //====================================
    IEnumerator FireRainRoutine()
    {
        if (fireBallPrefab == null)
            yield break;

        if (player == null)
            yield break;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        // Player ngoài tầm thì bỏ qua Fire Rain
        if (distance > fireRainRange)
        {
            Debug.Log("Player ngoài tầm Fire Rain.");
            yield break;
        }

        int ballCount =
            isPhase2 ? 4 : 3;

        Debug.Log($"📢 Boss dùng Fire Rain ({ballCount} quả)");

        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        float targetX =
            player.position.x;

        for (int i = 0; i < ballCount; i++)
        {
            float offset = 0f;

            if (i == 1)
                offset = -1.5f;

            if (i == 2)
                offset = 1.5f;

            if (i == 3)
                offset = -3f;

            Vector3 spawnPos =
                new Vector3(
                    targetX + offset,
                    transform.position.y + spawnHeight,
                    0);

            GameObject fire = Instantiate(
                                fireBallPrefab,
                                      spawnPos,
                                Quaternion.identity);

            FireBall fireBall = fire.GetComponent<FireBall>();

            if (fireBall != null)
            {
                fireBall.SetDamage(fireRainDamage);
            }

            yield return new WaitForSeconds(
                delayBetweenBalls);
        }
    }

    //====================================
    // SUMMON ENEMY
    //====================================
    IEnumerator SummonEnemyRoutine()
    {
        hasSummonedEnemy = true;

        Debug.Log("🔥 Boss triệu hồi Skeleton!");

        if (animator != null)
            animator.SetTrigger(attackTriggerName);

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

    IEnumerator MeleeAttackRoutine()
    {
        Debug.Log("Boss Melee");

        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(meleeHitDelay);

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                meleeRange,
                playerLayer);

        foreach (Collider2D hit in hits)
        {
            LoiPlayer playerScript = hit.GetComponent<LoiPlayer>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(meleeDamage);

                Debug.Log("Melee Damage : " + meleeDamage);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    //====================================
    // TAKE DAMAGE
    //====================================
    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        currentHP =
            Mathf.Clamp(
                currentHP,
                0,
                maxHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        Debug.Log(
            $"Boss HP: {currentHP}/{maxHP}");

        // Phase 2
        if (!isPhase2 &&
            currentHP <= maxHP * 0.3f)
        {
            isPhase2 = true;

            Debug.LogWarning(
                "⚠️ Boss Phase 2!");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    //====================================
    // DIE
    //====================================
    void Die()
    {
        isDead = true;
        GameManager.OnAnyBossDied?.Invoke();

        StopAllCoroutines();

        if (mainLaser != null)
            mainLaser.enabled = false;

        if (previewLaser != null)
            previewLaser.enabled = false;

        if (animator != null)
            animator.SetTrigger("damage");

        Debug.Log("💀 Boss chết!");

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                meleeRange);
        }
    }
}