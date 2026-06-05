using UnityEngine;

public class BackGroundControlH : MonoBehaviour
{
    [SerializeField] private Transform mainCam = null;
    [SerializeField] private Transform midBackGround = null;
    [SerializeField] private Transform sideBackground = null;
    [SerializeField] private MapControllerH mapController = null;
    [SerializeField] private float length = 0f;
    [SerializeField] private float speed = 0f;

    private void Awake()
    {
        if (mapController == null)
        {
            mapController = FindFirstObjectByType<MapControllerH>();
        }
    }

    private void Update()
    {
        if (mainCam == null || midBackGround == null || sideBackground == null)
        {
            return;
        }

        if (mainCam.position.x > midBackGround.position.x)
        {
            UpdateBackgroundPosition(Vector3.right);
        }
        else if (mainCam.position.x < midBackGround.position.x)
        {
            UpdateBackgroundPosition(Vector3.left);
        }
    }

    private void FixedUpdate()
    {
        if (mapController == null || midBackGround == null || sideBackground == null)
        {
            return;
        }

        Vector3 direction = GetScrollDirection(mapController.CurrentVelocity.x);
        midBackGround.position += direction * speed * Time.fixedDeltaTime;
        sideBackground.position += direction * speed * Time.fixedDeltaTime;
    }

    private static Vector3 GetScrollDirection(float mapVelocityX)
    {
        if (mapVelocityX > 0f)
        {
            return Vector3.left;
        }

        if (mapVelocityX < 0f)
        {
            return Vector3.right;
        }

        return Vector3.zero;
    }

    private void UpdateBackgroundPosition(Vector3 direction)
    {
        sideBackground.position = midBackGround.position + direction * length;

        Transform currentMiddle = midBackGround;
        midBackGround = sideBackground;
        sideBackground = currentMiddle;
    }
}
