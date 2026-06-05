using UnityEngine;

public class MapControllerH : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private float moveInput;

    public Vector2 CurrentVelocity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        CurrentVelocity = new Vector2(-moveInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = CurrentVelocity;
    }
}
