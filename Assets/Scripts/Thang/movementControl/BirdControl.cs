// using Mono.Cecil.Cil;
using UnityEngine;

public class BirdControl : MonoBehaviour
{
    private Rigidbody2D rb;
    [Header("Moverment")]
    public float jumpForce = 5f;
    public float moveSpeed = 2f;
    private Animator animator;
    public bool isGrounded = true;

    [Header("Fall & Recovery Settings")]
    public float fallThresholdAngle = 10f;
    public float uprightSpeed = 5f;

    public float SpeedMultiplier = 0.5f; // Hệ số dùng để tăng/giảm tốc độ animation theo ý muốn
    [Header("Dust when running")]
    public SimpleObjectPooler dustPooler;
    public float dustCreationRate = 0.2f;
    public float test;
    private float nextDustTime = 0f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dustPooler = FindAnyObjectByType<SimpleObjectPooler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
        // tốc độ di chuyển cố định không phải càng lúc càng nhanh
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        // check đang rơi hay đang bay lên
        float yVelocity = rb.linearVelocity.y;
        float animSpeed = 0.5f;
        if (yVelocity > -1)
            animSpeed = (1f + yVelocity) * SpeedMultiplier;
        animator.SetFloat("verticalSpeed", animSpeed);
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);

        // Check điều kiện: Đang ở trên mặt đất AND Đang chạy AND Đã đến lúc tạo bụi tiếp theo
        if (isGrounded && horizontalSpeed > 0.5f && Time.time >= nextDustTime)
        {
            // 1. Lấy một hiệu ứng bụi từ pool
            GameObject dustEffect = dustPooler.GetPooledDustEffect();

            // 2. Đặt vị trí bụi về vị trí chân nhân vật.
            // Cần điều chỉnh Vector2.down * height để phù hợp với chân nhân vật của bạn.
            Vector3 footPosition = new Vector3(transform.position.x, transform.position.y - 0.66f, 0);
            dustEffect.transform.position = footPosition;

            // 3. Kích hoạt hiệu ứng bụi
            DustEffect dustScript = dustEffect.GetComponent<DustEffect>();
            if (dustScript != null)
            {
                dustScript.Activate();
            }

            // 4. Cập nhật thời điểm tạo bụi tiếp theo
            nextDustTime = Time.time + dustCreationRate;
        }
        updateAnimation();
        CheckAndRecoverFromFall();
    }
    private void updateAnimation()
    {
        bool isFlying = !isGrounded;
        animator.SetBool("isFlying", isFlying);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var n = collision.GetContact(i).normal;
            if (n.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = false;
    }

    void CheckAndRecoverFromFall()
    {
        // Lấy góc Z hiện tại của nhân vật (chuyển về khoảng -180 đến 180 để dễ tính toán)
        float currentZAngle = transform.eulerAngles.z;
        if (currentZAngle > 180) currentZAngle -= 360;

        // Nếu góc nghiêng tuyệt đối lớn hơn ngưỡng cho phép (đang bị ngã)
        if (Mathf.Abs(currentZAngle) > fallThresholdAngle)
        {
            // Nếu đang ở trên mặt đất thì cho tự đứng dậy
            if (isGrounded)
            {
                // Tạo mục tiêu góc quay thẳng đứng (Z = 0)
                Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

                // Xoay mượt mà về góc thẳng đứng
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * uprightSpeed);
            }
        }
    }
}
