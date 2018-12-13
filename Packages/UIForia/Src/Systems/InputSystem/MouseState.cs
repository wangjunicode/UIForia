using UnityEngine;

public struct MouseState {

    public bool isLeftMouseDown;
    public bool isLeftMouseDownThisFrame;
    public bool isLeftMouseUpThisFrame;

    public bool isRightMouseDown;
    public bool isRightMouseDownThisFrame;
    public bool isRightMouseUpThisFrame;

    public bool isMiddleMouseDown;
    public bool isMiddleMouseDownThisFrame;
    public bool isMiddleMouseUpThisFrame;

    public Vector2 mousePosition;
    public Vector2 mouseDownPosition;
    public Vector2 previousMousePosition;
    
    public Vector2 scrollDelta;

    public bool isDoubleClick;
    public bool isTripleClick;
    public bool isSingleClick;

    public Vector2 MouseDownDelta {
        get {
            if (mouseDownPosition.x < 0 || mouseDownPosition.y < 0) {
                return Vector2.zero;
            }

            return mousePosition - mouseDownPosition;
        }
    }
    
    public Vector2 MouseDelta => previousMousePosition - mousePosition;
    public bool DidMove => MouseDelta.sqrMagnitude > 0;

}