using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossAI2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Melee")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float meleeRange = 1.2f;
    [SerializeField] private int meleeDamage = 3;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float meleeHitDelay = 0.35f;

    [Header("Dash Melee")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float stopDistance = 1.3f;
    private Vector3 startPosition;

    [Header("Laser")]
    public LineRenderer mainLaser;
    public LineRenderer previewLaser;
    public Transform firePoint;

    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private float laserDuration = 0.5f;
    [SerializeField] private float laserMaxDistance = 30f;
    [SerializeField] private int laserDamage = 10;



    [Header("Fire Rain")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float delayBetweenBalls = 0.4f;
    [SerializeField] private int fireRainDamage = 5;

    [Header("Summon")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform summonPoint;

    private bool hasSummonedEnemy = false;

    [Header("AI")]
    [SerializeField] private float detectRange = 15f;
    [SerializeField] private float timeBetweenAttacks = 2.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Boss HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private Slider hpSlider;

    private int currentHP;

    public static bool isDead;
    private bool isAttacking;
    private bool isPhase2;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Tự tìm Player nếu chưa kéo trong Inspector
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        startPosition = transform.position;

        if (mainLaser != null)
            mainLaser.enabled = false;

        if (previewLaser != null)
            previewLaser.enabled = false;

        if (animator == null)
            animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

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
        if (player == null || isDead)
            return;

        if (!isAttacking)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX =
                    player.position.x < transform.position.x;
            }
            else
            {
                Vector3 scale = transform.localScale;

                if (player.position.x > transform.position.x)
                    scale.x = Mathf.Abs(scale.x);
                else
                    scale.x = -Mathf.Abs(scale.x);

                transform.localScale = scale;
            }
        }
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

            float distance =
                Vector2.Distance(
                    attackPoint.position,
                    player.position);

            if (distance > detectRange)
            {
                yield return null;
                continue;
            }

            isAttacking = true;

            // Phase 2 chỉ triệu hồi 1 lần
            if (isPhase2 && !hasSummonedEnemy)
            {
                yield return StartCoroutine(
                    SummonEnemyRoutine());
            }
            else
            {
                //=========================
                // Player đứng gần
                //=========================
                if (distance <= meleeRange)
                {
                    yield return StartCoroutine(
                        MeleeAttackRoutine());
                }

                //=========================
                // Player đứng xa
                //=========================
                else
                {
                    int randomSkill =
                        Random.Range(0, 2);

                    if (randomSkill == 0)
                    {
                        yield return StartCoroutine(
                            LaserAttackRoutine());
                    }
                    else
                    {
                        yield return StartCoroutine(
                            FireRainRoutine());
                    }
                }
            }

            isAttacking = false;

            yield return new WaitForSeconds(
                timeBetweenAttacks);
        }
    }
    //====================================
    // LASER ATTACK
    //====================================
    IEnumerator LaserAttackRoutine()
    {
        Debug.Log("Boss -> Laser");

        if (player == null)
            yield break;

        if (animator != null)
            animator.SetTrigger("Laser");

        Vector3 startPos = firePoint != null
            ? firePoint.position
            : transform.position;

        // Khóa vị trí Player khi bắt đầu charge
        Vector3 targetPos = player.position + Vector3.up * 0.5f;

        Vector3 direction = (targetPos - startPos).normalized;
        Vector3 laserEnd = startPos + direction * laserMaxDistance;

        //========================
        // Preview Laser
        //========================
        if (previewLaser != null)
            previewLaser.enabled = true;

        float timer = 0f;

        while (timer < chargeDuration)
        {
            startPos = firePoint != null
                ? firePoint.position
                : transform.position;

            if (previewLaser != null)
            {
                previewLaser.SetPosition(0, startPos);
                previewLaser.SetPosition(1, laserEnd);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (previewLaser != null)
            previewLaser.enabled = false;

        if (mainLaser != null)
            mainLaser.enabled = true;

        //========================
        // Bắn Laser
        //========================
        bool damaged = false;
        timer = 0f;

        while (timer < laserDuration)
        {
            startPos = firePoint != null
                ? firePoint.position
                : transform.position;

            if (mainLaser != null)
            {
                mainLaser.SetPosition(0, startPos);
                mainLaser.SetPosition(1, laserEnd);
            }

            if (!damaged)
            {
                Debug.DrawRay(
                    startPos,
                    direction * laserMaxDistance,
                    Color.red,
                    1f);

                RaycastHit2D[] hits = Physics2D.RaycastAll(
                    startPos,
                    direction,
                    laserMaxDistance);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider == null)
                        continue;

                    Debug.Log("Raycast hit : " + hit.collider.name);

                    // Bỏ qua chính Boss
                    if (hit.collider.gameObject == gameObject)
                        continue;

                    LoiPlayer playerScript =
                        hit.collider.GetComponent<LoiPlayer>();

                    if (playerScript != null)
                    {
                        Debug.Log(">>> Player bị Laser trúng!");

                        playerScript.TakeDamage(laserDamage);

                        Debug.Log("Laser Damage : " + laserDamage);

                        damaged = true;
                        break;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (mainLaser != null)
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

        Debug.Log("Boss -> Fire Rain");

        if (animator != null)
            animator.SetTrigger("Shoot");

        // Chờ animation gồng
        yield return new WaitForSeconds(0.5f);

        int ballCount =
            isPhase2 ? 12 : 5;

        float targetX =
            player.position.x;

        for (int i = 0; i < ballCount; i++)
        {
            float offset = 0f;
            offset = Random.Range(-2f, 2f);

            Vector3 spawnPos =
                new Vector3(
                    targetX + offset,
                    transform.position.y + spawnHeight,
                    0f);

            GameObject fire = Instantiate(
                              fireBallPrefab,
                                    spawnPos,
                         Quaternion.identity);

            FireBall fireScript = fire.GetComponent<FireBall>();

            if (fireScript != null)
            {
                fireScript.SetDamage(fireRainDamage);
            }

            yield return new WaitForSeconds(
                delayBetweenBalls);
        }
    }
    //====================================
    // MELEE ATTACK
    //====================================
    IEnumerator MeleeAttackRoutine()
    {
        Debug.Log("Boss Dash Melee");

        // Lưu vị trí ban đầu
        Vector3 startPosition = transform.position;

        // Tính hướng tới Player
        Vector3 dir = (player.position - transform.position).normalized;

        // Điểm dừng trước mặt Player
        Vector3 targetPosition = player.position - dir * stopDistance;

        // Giữ nguyên trục Y để Boss chỉ lướt ngang
        targetPosition.y = transform.position.y;

        // Tắt collider khi lướt để không đẩy Player
        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider != null)
            bossCollider.enabled = false;

        //========================
        // Dash tới
        //========================
        while (Vector2.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                dashSpeed * Time.deltaTime);

            yield return null;
        }

        // Bật collider lại
        if (bossCollider != null)
            bossCollider.enabled = true;

        //========================
        // Animation đánh
        //========================
        animator.SetTrigger("Melee");

        yield return new WaitForSeconds(meleeHitDelay);

        //========================
        // Gây sát thương
        //========================
        Collider2D[] hits = Physics2D.OverlapCircleAll(
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

        yield return new WaitForSeconds(0.35f);

        //========================
        // Dash về vị trí cũ
        //========================
        if (bossCollider != null)
            bossCollider.enabled = false;

        while (Vector2.Distance(transform.position, startPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                returnSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = startPosition;

        if (bossCollider != null)
            bossCollider.enabled = true;
    }
    //====================================
    // SUMMON ENEMY
    //====================================
    IEnumerator SummonEnemyRoutine()
    {
        hasSummonedEnemy = true;

        Debug.Log("Boss -> Summon");

        if (enemyPrefab != null)
        {
            if (animator != null)
                animator.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.5f);

            Vector3 spawnPos =
                summonPoint != null
                ? summonPoint.position
                : transform.position + Vector3.left * 2f;

            GameObject enemy =
                Instantiate(
                    enemyPrefab,
                    spawnPos,
                    Quaternion.identity);

            enemy.name = "Summoned Skeleton";
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

        currentHP = Mathf.Clamp(
            currentHP,
            0,
            maxHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        Debug.Log($"Boss HP : {currentHP}/{maxHP}");

        //==============================
        // PHASE 2
        //==============================
        if (!isPhase2 &&
            currentHP <= maxHP * 0.3f)
        {
            isPhase2 = true;

            Debug.Log("⚠ Boss Phase 2");
        }

        //==============================
        // DIE
        //==============================
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
        //! Phát loa thông báo: "Có boss vừa chết!"
        GameManager.OnAnyBossDied?.Invoke();
        StopAllCoroutines();

        if (mainLaser != null)
            mainLaser.enabled = false;

        if (previewLaser != null)
            previewLaser.enabled = false;

        if (animator != null)
            animator.SetTrigger("Death");

        Debug.Log("Boss Dead");

        Destroy(gameObject, 1.8f);
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StopScoreTick(true);
        }
    }


    //====================================
    // GIZMOS
    //====================================
    private void OnDrawGizmosSelected()
    {
        // Detect Range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange);

        // Melee Range
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                meleeRange);
        }

        // Fire Point
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(
                firePoint.position,
                0.15f);
        }
    }
}