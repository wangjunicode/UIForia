using UnityEngine;

public struct MouseHandleShape {

    public Rect rect;
    public float radius;
    public int elementId;
    public bool capture;
    public MouseEventType types;

    public static bool Contains(MouseHandleShape shape, Vector2 point) {
        Vector2 center = shape.rect.center;
        if ((point - center).sqrMagnitude < shape.radius * shape.radius) {
            return false;
        }
        return shape.rect.Contains(point);
    }

}