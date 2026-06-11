using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator anim;
    private Enemy enemyScript;

    void Start()
    {
        anim = GetComponent<Animator>();
        enemyScript = GetComponent<Enemy>();
    }

    void Update()
    {
        if (anim != null && enemyScript != null)
        {
            // Vì trong script gốc của bạn không có biến kiểm tra đứng im, 
            // nên tạm thời mặc định là true (luôn chạy tuần tra)
            anim.SetBool("isRunning", true); 
        }
    }
}