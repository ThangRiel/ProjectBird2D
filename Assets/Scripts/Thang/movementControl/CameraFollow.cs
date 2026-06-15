using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 1.5f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Ground Height Settings")]
    [SerializeField] private float groundOffsetY = -0.5f; // Độ cao camera muốn bù thêm khi CHẠM ĐẤT (ví dụ âm để hạ camera xuống nhìn thấy nhiều đất hơn)
    [SerializeField] private float airOffsetY = 1.5f;     // Độ cao camera khi ĐANG BAY (thường cao hơn để ưu tiên nhìn nhân vật)
    [SerializeField] private float maxGroundCheckDist = 5f; // Khoảng cách tối đa để bắt đầu tính toán nhìn đất
    [SerializeField] private float maxGroundOffset = 2f;
    [SerializeField] private float transitionSpeed = 5f;  // Tốc độ chuyển đổi giữa 2 độ cao camera

    [Header("References")]
    // Kéo Script di chuyển của nhân vật (chứa biến isGround) vào đây
    [SerializeField] private BirdControl birdControl;
    [SerializeField] private LayerMask groundLayer;

    private Vector3 velocity;
    private float currentOffsetY;

    private void Start()
    {
        // Ban đầu đặt Y bằng offset mặc định
        currentOffsetY = offset.y;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // 1. Kiểm tra trạng thái của nhân vật để chọn độ cao mục tiêu
        float targetOffsetY = offset.y;
        Vector3 targetPosition = target.position;
        if (birdControl != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(target.position, Vector2.down, maxGroundCheckDist, groundLayer);

            if (hit.collider != null)
            {
                // Khoảng cách thực tế từ nhân vật tới đất
                float distanceToGround = hit.distance;

                float influenceFactor = 1f - (distanceToGround / maxGroundCheckDist);
                influenceFactor = Mathf.Clamp01(influenceFactor); // Giới hạn từ 0 đến 1

                // Trừ bớt trục Y của targetPosition để kéo camera xuống đất một chút
                // Khi bay quá cao (influenceFactor = 0), đoạn trừ này bằng 0 -> Camera quay về ưu tiên nhìn nhân vật
                targetPosition.y -= influenceFactor * maxGroundOffset;
            }
            
            targetOffsetY = birdControl.isGrounded ? groundOffsetY : targetOffsetY;
        }

        // 2. Chuyển đổi mượt mà giữa độ cao cũ và độ cao mới
        currentOffsetY = Mathf.Lerp(currentOffsetY, targetOffsetY, Time.deltaTime * transitionSpeed);

        // 3. Áp dụng độ cao vừa tính vào vị trí camera mục tiêu
        targetPosition = target.position + new Vector3(offset.x, currentOffsetY, offset.z);

        // 4. Di chuyển camera theo nhân vật
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}