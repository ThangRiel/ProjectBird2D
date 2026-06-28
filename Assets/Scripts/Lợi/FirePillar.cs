using UnityEngine;

public class FirePillar : MonoBehaviour
{
    public float destroyTime = 2f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player trúng lửa");
        }
    }
}