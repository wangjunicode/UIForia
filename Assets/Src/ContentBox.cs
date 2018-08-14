
public class ContentBox {

    public ContentBoxRect border;
    public ContentBoxRect margin;
    public ContentBoxRect padding;

    public float contentWidth;
    public float contentHeight;

//    public float GetContentWidth() {
//        return Mathf.Min(0, contentWidth - (border.horizontal + margin.horizontal + padding.horizontal));
//    }
//
//    public float GetContentHeight() {
//        return Mathf.Min(0, contentHeight - (border.vertical + margin.vertical + padding.vertical));
//    }

//    public float SetContentWidth(float width) {
//        totalWidth = width - (border.horizontal + margin.horizontal + padding.horizontal);
//        return totalWidth;
//    }
//
//    public float SetContentHeight(float height) {
//        totalHeight = height - (border.vertical + margin.vertical + padding.vertical);
//        return totalHeight;
//    }

}