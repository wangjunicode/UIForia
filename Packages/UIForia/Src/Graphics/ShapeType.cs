namespace Vertigo {

    public enum ShapeType {

        Unset = 0,
        Rect = 1 << 0,
        RoundedRect = 1 << 1,
        Circle = 1 << 2,
        Ellipse = 1 << 3,
        Path = 1 << 4,
        ClosedPath = 1 << 5,
        Triangle = 1 << 6,
        Polygon = 1 << 7,
        Rhombus = 1 << 8,
        Sprite = 1 << 9

    }

}