using UnityEngine;

public class CameraBackgroundLoopH : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget = null;
    [SerializeField] private Transform[] backgroundPieces = null;
    [SerializeField] private float pieceWidth = 20f;
    [SerializeField] private float parallaxStrength = 0.5f;

    private Vector3 previousCameraPosition;

    private void Start()
    {
        if (cameraTarget == null && Camera.main != null)
        {
            cameraTarget = Camera.main.transform;
        }

        if (cameraTarget != null)
        {
            previousCameraPosition = cameraTarget.position;
        }
    }

    private void LateUpdate()
    {
        if (cameraTarget == null || backgroundPieces == null || backgroundPieces.Length == 0)
        {
            return;
        }

        Vector3 cameraDelta = cameraTarget.position - previousCameraPosition;
        MoveBackground(cameraDelta);
        LoopBackgroundPieces();

        previousCameraPosition = cameraTarget.position;
    }

    private void MoveBackground(Vector3 cameraDelta)
    {
        Vector3 parallaxMove = new(cameraDelta.x * parallaxStrength, cameraDelta.y * parallaxStrength, 0f);

        foreach (Transform piece in backgroundPieces)
        {
            if (piece != null)
            {
                piece.position += parallaxMove;
            }
        }
    }

    private void LoopBackgroundPieces()
    {
        foreach (Transform piece in backgroundPieces)
        {
            if (piece == null)
            {
                continue;
            }

            float distanceFromCamera = cameraTarget.position.x - piece.position.x;

            if (distanceFromCamera > pieceWidth)
            {
                piece.position += Vector3.right * pieceWidth * backgroundPieces.Length;
            }
            else if (distanceFromCamera < -pieceWidth)
            {
                piece.position += Vector3.left * pieceWidth * backgroundPieces.Length;
            }
        }
    }
}
