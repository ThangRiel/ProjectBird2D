using UnityEngine;

public class BirdControl : MonoBehaviour
{
    private Rigidbody2D rb;
    public float jumpForce = 5f;
    public float moveSpeed = 2f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump")){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        // tốc độ di chuyển cố định không phải càng lúc càng nhanh
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        
        
    }
}
