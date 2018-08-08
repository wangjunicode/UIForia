using UnityEngine;

public class ContentBox {
    
    public RectOffset border;
    public RectOffset margin;
    public RectOffset padding;

    public float totalWidth;
    public float totalHeight;
    
    public float GetContentWidth() {
        return Mathf.Min(0, totalWidth - (border.horizontal + margin.horizontal + padding.horizontal));
    }
    
    public float GetContentHeight() {
        return Mathf.Min(0, totalHeight - (border.vertical + margin.vertical + padding.vertical));
    }

    public float SetContentWidth(float width) {
        return 0;
    }

    public float SetContentHeight() {
        return 0;
    }
}