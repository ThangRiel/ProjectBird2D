using System.Collections;
using UnityEngine;

public class RunAndFly : MonoBehaviour
{
    [Header("Moverment")]
    public float moveSpeed;
    public float speedAdjust;
    public float jumpForce;
    public float speedMultiplier = 1f; // ! dùng cho trap ice, làm chậm, khác với SpeedMultiplier được tính bên dưới

    [Header("Giới hạn di chuyển")]
    public Transform anchorTransform;
    public float maxForwardOffset;
    public float maxBackwardOffset;

    [Header("Cơ chế Sinh tử (Out of Bounds)")]
    public float outOfBoundsTimeLimit = 2f; // Thời gian tối đa ở ngoài (1 đến 2 giây)
    private float outOfBoundsTimer = 0f;
    private bool isDead = false;
    public bool isOutOfBounds = true;
    private float outOfBoundsPushDirection = 0f;
    public float outOfBoundsPushForce = 15f; // Lực đẩy khi ở ngoài vùng an toàn
    public float Delay = 1.0f; // Thời gian trễ trước khi ngưng đẩy
    [Header("Fall & Recovery Settings")]
    public float fallThresholdAngle = 10f;
    public float uprightSpeed = 5f;

    public float SpeedMultiplier = 0.5f; // Hệ số dùng để tăng/giảm tốc độ animation theo ý muốn
    [Header("Dust when running")]
    public SimpleObjectPooler dustPooler;
    public float dustCreationRate = 0.2f;
    private float nextDustTime = 0f;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private float Y = -0.3f;
    private bool isFacingWall;

    private bool isGrounded = true;
    private Animator animator;
    private Rigidbody2D rb;
    private float moveInput;
    private GameManager gameManager;
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        dustPooler = FindAnyObjectByType<SimpleObjectPooler>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        if (isDead) return;
        moveInput = Input.GetAxis("Horizontal");

        CheckOutOfBounds();
        CheckWall();
        // Kiểm tra xem người chơi có đang cố tình tự đi ra ngoài không
        if (anchorTransform != null)
        {
            float minX = anchorTransform.position.x - maxBackwardOffset + 0.2f;
            float maxX = anchorTransform.position.x + maxForwardOffset - 0.2f;
            float currentX = transform.position.x;

            // Nếu đang ở mép trái (sau) mà vẫn bấm lùi (moveInput < 0) -> Khóa lại không cho lùi
            if (currentX <= minX && moveInput < 0)
            {
                moveInput = 0;
            }
            // Nếu đang ở mép phải (trước) mà vẫn bấm tiến (moveInput > 0) -> Khóa lại không cho tiến
            if (currentX >= maxX && moveInput > 0)
            {
                moveInput = 0;
            }
        }
        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }

        // check đang rơi hay đang bay lên
        float yVelocity = rb.linearVelocity.y;
        float animSpeed = 0.5f;
        if (yVelocity > -1)
            animSpeed = (1f + yVelocity) * SpeedMultiplier;
        animator.SetFloat("verticalSpeed", animSpeed);
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);

        //*sửa
        float currentMoveSpeed = isFacingWall ? 0f : moveSpeed;
        float targetXVelocity = (currentMoveSpeed + (moveInput * speedAdjust)) * speedMultiplier;

        if (isOutOfBounds)
        {
            targetXVelocity = outOfBoundsPushDirection * outOfBoundsPushForce;
            rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);
        }

        //! Chặn luôn vận tốc tiến về phía trước nếu đang kẹt tường
        if (isFacingWall && targetXVelocity > 0)
        {
            targetXVelocity = 0f;
        }
        rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);

        // Check tạo bụi
        if (isGrounded && horizontalSpeed > 0.5f && Time.time >= nextDustTime && dustPooler != null)
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

    void CheckOutOfBounds()
    {
        if (anchorTransform == null) return;

        float minX = anchorTransform.position.x - maxBackwardOffset;
        float maxX = anchorTransform.position.x + maxForwardOffset;
        float currentX = transform.position.x;

        // Nếu vượt quá giới hạn trước HOẶC sau
        if (currentX < minX || currentX > maxX)
        {
            outOfBoundsTimer += Time.deltaTime;
            isOutOfBounds = true;
            outOfBoundsPushDirection = currentX < minX ? 1f : -1f;
            Debug.LogWarning($"Đang ở ngoài vùng an toàn! Cảnh báo: {outOfBoundsTimeLimit - outOfBoundsTimer:F1}s còn lại!");

            if (outOfBoundsTimer >= outOfBoundsTimeLimit)
            {
                Die();
            }
        }
        else
        {
            // Nếu kịp thời quay lại vùng an toàn thì reset bộ đếm
            outOfBoundsTimer = 0f;
            StartCoroutine(StartAttractDelay());
            outOfBoundsPushDirection = 0f;
        }
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
    public void Die() // ! Chỉnh lại chỗ này thành public để gọi bên HealthManager
    {
        if (isDead) return;
        isDead = true;

        // rb.linearVelocity = Vector2.zero; // Dừng mọi di chuyển
        // Time.timeScale = 0f; //* Dừng thời gian
        Debug.LogError("Hết máu! Mày đã thua!");

        // Thêm logic kích hoạt Anim chết hoặc hiện bảng Game Over ở đây
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    // Khi va chạm với chướng ngại vật (Obstacle)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Va chạm chướng ngại vật! Bị cản/đẩy lại!");
            // Rigidbody2D vật lý tự xử lý việc đẩy lùi nếu chướng ngại vật có Collider2D
        }
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
    void CheckWall()
    {
        Vector2 checkPos = transform.position + Vector3.up * Y; // Nâng lên một chút để tránh va chạm với mặt đất
        //* Bắn tia sang phải (phía trước mặt)
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.right, wallCheckDistance, wallLayer);
        isFacingWall = hit.collider != null;
    }

    private void OnDrawGizmos() //! chỉ có tác dụng hiển thị trong editor, không ảnh hưởng gameplay
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * Y, transform.position + Vector3.up * Y + Vector3.right * wallCheckDistance);
    }

    private IEnumerator StartAttractDelay()
    {
        yield return new WaitForSeconds(Delay);
        isOutOfBounds = false;
    }
}