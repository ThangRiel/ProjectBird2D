using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackGroundControl : MonoBehaviour
{

    public Transform mainCam;
    public Transform midBackGround;
    public Transform sideBackground;
    public float length;
    public float speed;
    private MapController mapController;
    private Vector3 right = Vector3.right;
    private Vector3 left = Vector3.left;

    // Update is called once per frame

    private void Awake()
    {
        mapController = FindObjectOfType<MapController>();
    }
    void Update()
    {
        if (mainCam.position.x > midBackGround.position.x)
        {
            updateBackGroundPosition(right);
        }
        else if (mainCam.position.x < midBackGround.position.x)
        {
            updateBackGroundPosition(left);
        }
    }

    void FixedUpdate()
    {
        Vector3 direction;
        Vector2 movement;

        movement = mapController.HandleMovement();
        if (movement.x > 0)
        {
            direction = left;
        }
        else if (movement.x < 0)
        {
            direction = right;
        }
        else
        {
            direction = Vector3.zero;
        }
        midBackGround.position += direction * speed * Time.fixedDeltaTime;
        sideBackground.position += direction * speed * Time.fixedDeltaTime;
    }

    void updateBackGroundPosition(Vector3 direction)
    {
        sideBackground.position = midBackGround.position + direction*length;
        Transform temp = midBackGround;
        midBackGround = sideBackground;
        sideBackground = temp;
    }
}
