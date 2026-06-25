using UnityEngine;
using UnityEngine.InputSystem;

namespace PickNOre
{
    public class CursorUI : MonoBehaviour
    {
        [SerializeField] private InputActionReference pointerPositionAction;

        private RectTransform cursorTransform;
        private Canvas parentCanvas;
        private RectTransform canvasRectTransform;
        private Camera canvasCamera;

        void Awake()
        {
            cursorTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
                canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            }
        }

        void OnEnable()
        {
            Cursor.visible = false;
            pointerPositionAction.action.performed += OnPointerPositionChanged;
        }

        void OnDisable()
        {
            Cursor.visible = true;
            pointerPositionAction.action.performed -= OnPointerPositionChanged;
        }
        private void OnPointerPositionChanged(InputAction.CallbackContext ctx)
        {
            if (cursorTransform == null || canvasRectTransform == null) return;

            var mousePosition = ctx.ReadValue<Vector2>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePosition, canvasCamera, out var localPoint))
            {
                cursorTransform.anchoredPosition = localPoint;
            }
        }
    }
}

