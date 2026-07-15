using UnityEngine;

public class ParallaxMove : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor = 0.3f; // 0 = đứng im, 1 = di chuyển giống camera

    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, 0f, 0f);
        lastCamPos = cam.position;
    }
}