using UnityEngine;

public class LayeredParallaxBackgroundH : MonoBehaviour
{
    [System.Serializable]
    private class ParallaxLayer
    {
        public Transform[] pieces = null;
        public float xParallaxStrength = 0.3f;
        public float yParallaxStrength = 0f;
        public bool loopHorizontally = true;
        public float loopWidth = 20f;
    }

    [SerializeField] private Transform cameraTarget = null;
    [SerializeField] private ParallaxLayer[] layers = null;

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
        if (cameraTarget == null || layers == null)
        {
            return;
        }

        Vector3 cameraDelta = cameraTarget.position - previousCameraPosition;

        foreach (ParallaxLayer parallaxLayer in layers)
        {
            UpdateLayer(parallaxLayer, cameraDelta);
        }

        previousCameraPosition = cameraTarget.position;
    }

    private void UpdateLayer(ParallaxLayer parallaxLayer, Vector3 cameraDelta)
    {
        if (parallaxLayer == null || parallaxLayer.pieces == null)
        {
            return;
        }

        Vector3 movement = new(
            cameraDelta.x * parallaxLayer.xParallaxStrength,
            cameraDelta.y * parallaxLayer.yParallaxStrength,
            0f);

        foreach (Transform piece in parallaxLayer.pieces)
        {
            if (piece != null)
            {
                piece.position += movement;
            }
        }

        if (parallaxLayer.loopHorizontally)
        {
            LoopLayer(parallaxLayer);
        }
    }

    private void LoopLayer(ParallaxLayer parallaxLayer)
    {
        if (parallaxLayer.pieces.Length == 0 || parallaxLayer.loopWidth <= 0f)
        {
            return;
        }

        float totalWidth = parallaxLayer.loopWidth * parallaxLayer.pieces.Length;

        foreach (Transform piece in parallaxLayer.pieces)
        {
            if (piece == null)
            {
                continue;
            }

            float distanceFromCamera = cameraTarget.position.x - piece.position.x;

            if (distanceFromCamera > parallaxLayer.loopWidth)
            {
                piece.position += Vector3.right * totalWidth;
            }
            else if (distanceFromCamera < -parallaxLayer.loopWidth)
            {
                piece.position += Vector3.left * totalWidth;
            }
        }
    }
}
