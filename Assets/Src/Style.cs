using UnityEngine;

public class Style {
    
    public RectOffset border;
    public RectOffset margin;
    public RectOffset padding;

    public Color backgroundColor;
    public Color borderColor;
    public Color textColor;
    public TextStyle textStyle;
    public Material material;

}

public enum LayoutUnitType {
    Pixels, Percentage, Flex
}

public struct LayoutUnit {
    public float value;
    public LayoutUnitType type;
}

public class LayoutParameters {
    public float width;
    public float height;
    public float minWidth;
    public float maxWidth;
    public float minHeight;
    public float maxHeight;
}

public class TextStyle {
    public Color color;
    public int fontSize;
    public Font font;
    public FontStyle fontStyle;
    public TextAlignment alignment;
}