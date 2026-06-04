using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;


    private bool isGrounded;
    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }
    void Update()
    {
        
        HandleMovement();

    }
    public Vector2 HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 vector = new Vector2(-moveInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = vector;
        return vector;
    }
}
