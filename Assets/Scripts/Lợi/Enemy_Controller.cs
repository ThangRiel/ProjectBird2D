using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Gọi hàm này khi Enemy di chuyển
    public void Move(bool moving)
    {
        anim.SetBool("isRunning", moving);
    }

    // Gọi hàm này khi Enemy tấn công
    public void Attack()
    {
        anim.SetTrigger("Attack"); 
    }

    // Gọi hàm này khi Enemy nhận sát thương
    public void TakeDamage()
    {
        anim.SetTrigger("Hit");
    }

    // Gọi hàm này khi Enemy hết máu
    public void Death()
    {
        anim.SetTrigger("Die");
    }
}