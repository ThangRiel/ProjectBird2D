using UnityEngine;

public class PlayerWorldShiftController_NoLayer : MonoBehaviour
{
    [Header("Refs")]
    public Transform worldRoot;
    public Rigidbody2D rb;

    [Header("Move (World shifts)")]
    public float worldSpeed = 6f;

    [Header("Jump")]
    public float jumpForce = 12f;

    // grounded state (không dùng layer)
    private bool grounded;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    
    void Update()
    {
        // 1) Di chuyển world theo input trái/phải
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        if (worldRoot != null)
        {
            worldRoot.position += new Vector3(-x * worldSpeed * Time.deltaTime, 0f, 0f);
        }

        // 2) Nhảy (Space) - chỉ cho nhảy khi grounded
        if (Input.GetButtonDown("Jump") && grounded)
        {
            grounded = true; // tránh double jump khi đang rời mặt đất
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // mỗi frame vật lý reset lại, nếu có collision bên dưới thì sẽ set lại true
        grounded = false;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        // Nếu có tiếp xúc với mặt phẳng bên dưới (normal hướng lên) => đang đứng trên đất
        for (int i = 0; i < col.contactCount; i++)
        {
            var n = col.GetContact(i).normal;
            if (n.y > 0.5f) // chạm từ trên xuống mặt đất
            {
                grounded = true;
                return;
            }
        }
    }
}