using Rendering;

namespace Src.Systems {

    public interface IStyleChangeHandler {

        void SetPaint(int elementId, Paint paint);
        void SetRect(int elementId, LayoutRect rect);
        void SetMargin(int elementId, ContentBoxRect margin);
        void SetBorder(int elementId, ContentBoxRect border);
        void SetPadding(int elementId, ContentBoxRect padding);
        void SetLayout(int elementId, LayoutParameters data);
        void SetBorderRadius(int elementId, BorderRadius radius);
        void SetConstraints(int elementId, LayoutConstraints constraints);
        void SetText(int elementId, TextStyle textStyle);

    }

}