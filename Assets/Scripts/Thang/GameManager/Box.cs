using UnityEngine;

public class Box : MonoBehaviour
{
    private bool isDamed = false;
    private Collider2D Collider;
    private Rigidbody2D rb;
    private Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Collider = GetComponent<Collider2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDamed)
        {
            Collider.enabled = false;
        }
    }
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Bullet"))
        {
            isDamed = true;
            anim.SetBool("isDamed", true);
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}
