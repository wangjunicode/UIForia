using System;
using Rendering;
using Src.Layout;

namespace Src.Systems {

    public interface IStyleSystem : IStyleChangeHandler {

        event PaintChanged onPaintChanged;
        event LayoutChanged onLayoutChanged;
        event RectPropertyChanged onRectChanged;
        event ContentBoxChanged onMarginChanged;
        event ContentBoxChanged onBorderChanged;
        event ContentBoxChanged onPaddingChanged;
        event ConstraintChanged onConstraintChanged;
        event BorderRadiusChanged onBorderRadiusChanged;
        event FontPropertyChanged onFontPropertyChanged;
        event AvailableStatesChanged onAvailableStatesChanged;
        event TextContentChanged onTextContentChanged;
        
        event Action<UIElement> onOverflowPropertyChanged;
        event Action<UIElement, MainAxisAlignment, MainAxisAlignment> onMainAxisAlignmentChanged;
        event Action<UIElement, CrossAxisAlignment, CrossAxisAlignment> onCrossAxisAlignmentChanged;
        event Action<UIElement, LayoutWrap, LayoutWrap> onLayoutWrapChanged;
        event Action<UIElement, LayoutDirection> onLayoutDirectionChanged;
        event Action<UIElement, LayoutType> onLayoutTypeChanged;

    }

}