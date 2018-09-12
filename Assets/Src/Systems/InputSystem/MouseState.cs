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
    public bool isDragging;

    public Vector2 MouseDelta => previousMousePosition - mousePosition;
    public bool DidMove => MouseDelta.sqrMagnitude > 0;

}