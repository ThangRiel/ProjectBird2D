using UnityEngine;

public class CameraMoveIndependent : MonoBehaviour
{
    public float speed = 5f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * speed);
    }
}
