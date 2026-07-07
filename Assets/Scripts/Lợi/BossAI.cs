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

    [Header("Fire Rain")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float delayBetweenBalls = 0.4f;
    [SerializeField] private float fireRainRange = 10f;

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
        yield return new WaitForSeconds(3f);

        int attackIndex = 0;

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

            if (isPhase2 && !hasSummonedEnemy)
            {
                yield return StartCoroutine(SummonEnemyRoutine());
            }
            else
            {
                if (attackIndex % 2 == 0)
                {
                    yield return StartCoroutine(LaserAttackRoutine());
                }
                else
                {
                    float distance =
                        Vector2.Distance(
                            transform.position,
                            player.position);

                    if (distance <= fireRainRange)
                    {
                        yield return StartCoroutine(FireRainRoutine());
                    }
                    else
                    {
                        // Nếu Player đứng quá xa thì Boss đổi sang Laser
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

                if (hit.collider != null &&
                    hit.collider.CompareTag("Player"))
                {
                    hit.collider.SendMessage(
                        "TakeDamage",
                        2,
                        SendMessageOptions.DontRequireReceiver);

                    damaged = true;
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

            Instantiate(
                fireBallPrefab,
                spawnPos,
                Quaternion.identity);

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

    //====================================
    // TAKE DAMAGE
    //====================================
    public void TakeDamage(int damage)
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

        StopAllCoroutines();

        if (mainLaser != null)
            mainLaser.enabled = false;

        if (previewLaser != null)
            previewLaser.enabled = false;

        if (animator != null)
            animator.SetTrigger("die");

        Debug.Log("💀 Boss chết!");

        Destroy(gameObject, 1.5f);
    }
}