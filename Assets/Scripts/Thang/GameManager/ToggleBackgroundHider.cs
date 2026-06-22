using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleBackgroundHider : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo đối tượng 'Background' (hình ảnh OFF) vào đây.")]
    public GameObject backgroundObject; // Đối tượng Tắt/Normal

    private Toggle toggle;

    private void Awake()
    {
        // 1. Lấy component Toggle trên chính đối tượng này
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        // 2. Lắng nghe sự kiện thay đổi giá trị của Toggle
        toggle.onValueChanged.AddListener(OnToggleStateChanged);

        // 3. Khởi tạo trạng thái ban đầu khi bật Menu
        OnToggleStateChanged(toggle.isOn);
    }

    private void OnDisable()
    {
        // 4. Ngừng lắng nghe sự kiện khi tắt Menu (tránh lỗi)
        toggle.onValueChanged.RemoveListener(OnToggleStateChanged);
    }

    // 5. Hàm xử lý logic ẩn/hiện Background
    private void OnToggleStateChanged(bool isOn)
    {
        if (backgroundObject == null) return;

        // Nếu Toggle ĐANG BẬT (ON) -> Ẩn backgroundObject đi (Tắt)
        // Nếu Toggle ĐANG TẮT (OFF) -> Hiện backgroundObject lên (Bật)
        backgroundObject.SetActive(!isOn);
    }
}