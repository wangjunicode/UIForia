using Rendering;
using Src.Layout;

namespace Src.Systems {

    public interface IStyleChangeHandler {

        void SetPaint(UIElement element, Paint paint);
        void SetDimensions(UIElement element, Dimensions rect);
        void SetMargin(UIElement element, ContentBoxRect margin);
        void SetBorder(UIElement element, ContentBoxRect border);
        void SetPadding(UIElement element, ContentBoxRect padding);
        void SetLayout(UIElement element, LayoutParameters data);
        void SetBorderRadius(UIElement element, BorderRadius radius);
        void SetConstraints(UIElement element, LayoutConstraints constraints);
        void SetTextStyle(UIElement element, TextStyle textStyle);
        void SetAvailableStates(UIElement element, StyleState availableStates);
        void SetTransform(UIElement element, UITransform transform);

        void SetMainAxisAlignment(UIElement element, MainAxisAlignment alignment, MainAxisAlignment oldAlignment);

        void SetCrossAxisAlignment(UIElement element, CrossAxisAlignment alignment, CrossAxisAlignment oldAlignment);

        void SetLayoutWrap(UIElement element, LayoutWrap layoutWrap, LayoutWrap oldWrap);

        void SetLayoutDirection(UIElement element, LayoutDirection direction);

        void SetLayoutType(UIElement element, LayoutType layoutType);

    }

}