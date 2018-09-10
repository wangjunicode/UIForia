using Src;
using UnityEngine;

public struct MeasurementVector2 {

    public UIMeasurement x;
    public UIMeasurement y;

    public MeasurementVector2(UIMeasurement x, UIMeasurement y) {
        this.x = x;
        this.y = y;
    }

}

public class UITransform {

    public MeasurementVector2 position;
    public Vector2 scale;
    public Vector2 pivot;
    public float rotation;

}