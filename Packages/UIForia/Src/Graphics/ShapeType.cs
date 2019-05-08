namespace Vertigo {

    public enum ShapeType {

        Unset = 0,
        Rect = 1 << 0,
        RoundedRect = 1 << 1,
        Circle = 1 << 2,
        Ellipse = 1 << 3,
        Rhombus = 1 << 4,
        Triangle = 1 << 5,
        Polygon = 1 << 6,
        // below are NOT used in SDF shaders
        Path = 1 << 7,
        ClosedPath = 1 << 8,
        Sprite = 1 << 9

    }

}