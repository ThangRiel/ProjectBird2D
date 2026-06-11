using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Transform worldRoot;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    public float attackDuration = 0.4f;

    private float Scale;
    private bool isGrounded;
    private Animator animator;
    private Rigidbody2D rb;
    private Transform playerTransform;
    public bool isAttacking;
    private float moveInput;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab; 
    public Transform firePoint;     

    void Awake()
    {

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
        Scale = playerTransform.localScale.x;
    }

    void Start()
    {

    }
    void Update()
    {

        if (!isAttacking)
        {
            moveInput = Input.GetAxis("Horizontal");
            HandleJump();
        }
        else
        {
            moveInput = 0f;
        }

        HandleAttack();
        UpdateAnimator();
    }
    void FixedUpdate()
    {
        isGrounded = false;
        if (!isAttacking)
        {
            HandleMovement();
        }
    }
    private void HandleAttack()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        // Tạo viên đạn tại vị trí firePoint
        if (bulletPrefab != null && firePoint != null)
        {
            Quaternion bulletRotation = firePoint.rotation;

            if (transform.localScale.x < 0)
            {
                // Nếu đang quay mặt sang trái, xoay đạn 180 độ
                bulletRotation *= Quaternion.Euler(0, 180f, 0);
            }
            Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        }
        // Chờ
        yield return new WaitForSeconds(attackDuration);

        // Hết attackDuration tự động tắt
        isAttacking = false;
        
    }
    public Vector2 HandleMovement()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(Scale, Scale, Scale);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Scale, Scale, Scale);
        }

        if (worldRoot != null)
        {
            worldRoot.position += new Vector3(-moveInput * moveSpeed * Time.deltaTime, 0f, 0f);
        }
        // điều khiển background di chuyển ngược lại để tạo hiệu ứng parallax
        return new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false; // tránh double jump khi đang rời mặt đất
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    void OnCollisionStay2D(Collision2D col)
    {
        // Nếu có tiếp xúc với mặt phẳng bên dưới (normal hướng lên) => đang đứng trên đất
        for (int i = 0; i < col.contactCount; i++)
        {
            var n = col.GetContact(i).normal;
            if (n.y > 0.5f) // chạm từ trên xuống mặt đất
            {
                isGrounded = true;
                return;
            }
        }
    }
    private void UpdateAnimator()
    {
        bool isRunning = Mathf.Abs(moveInput) > 0.1f;
        bool isJumping = !isGrounded;
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isAttacking", isAttacking);
    }
}
