using System;
using Rendering;
using Src.Layout;
using Src.Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public interface IStyleSystem {

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

        event Action<UIElement, UIMeasurement, UIMeasurement> onMinWidthChanged;
        event Action<UIElement, UIMeasurement, UIMeasurement> onMaxWidthChanged;
        event Action<UIElement, UIMeasurement, UIMeasurement> onPreferredWidthChanged;

        event Action<UIElement, UIMeasurement, UIMeasurement> onMinHeightChanged;
        event Action<UIElement, UIMeasurement, UIMeasurement> onMaxHeightChanged;
        event Action<UIElement, UIMeasurement, UIMeasurement> onPreferredHeightChanged;

        event Action<UIElement, int, int> onGrowthFactorChanged;
        event Action<UIElement, int, int> onShrinkFactorChanged;

        event Action<UIElement, int> onFlexItemOrderOverrideChanged;
        event Action<UIElement, CrossAxisAlignment, CrossAxisAlignment> onFlexItemSelfAlignmentChanged;
        
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

        int SetFontSize(UIElement element, int fontSize);
        Color SetFontColor(UIElement element, Color fontColor);
        
        void SetMainAxisAlignment(UIElement element, MainAxisAlignment alignment, MainAxisAlignment oldAlignment);

        void SetCrossAxisAlignment(UIElement element, CrossAxisAlignment alignment, CrossAxisAlignment oldAlignment);

        void SetLayoutWrap(UIElement element, LayoutWrap layoutWrap, LayoutWrap oldWrap);

        void SetLayoutDirection(UIElement element, LayoutDirection direction);

        void SetLayoutType(UIElement element, LayoutType layoutType);

        void SetMinWidth(UIElement element, UIMeasurement newMinWidth, UIMeasurement oldMinWidth);
        void SetMaxWidth(UIElement element, UIMeasurement newMaxWidth, UIMeasurement oldMaxWidth);
        void SetPreferredWidth(UIElement element, UIMeasurement newPrefWidth, UIMeasurement oldPrefWidth);
        
        void SetMinHeight(UIElement element, UIMeasurement newMinHeight, UIMeasurement oldMinHeight);
        void SetMaxHeight(UIElement element, UIMeasurement newMaxHeight, UIMeasurement oldMaxHeight);
        void SetPreferredHeight(UIElement element, UIMeasurement newPrefHeight, UIMeasurement oldPrefHeight);

        void SetShrinkFactor(UIElement element, int factor, int oldFactor);

        void SetGrowthFactor(UIElement element, int factor, int oldFactor);
        void SetFlexOrderOverride(UIElement element, int order, int oldOrder);

        void SetFlexSelfAlignment(UIElement element, CrossAxisAlignment alignment, CrossAxisAlignment oldAlignment);

        void SetFontAsset(UIElement styleSetElement, AssetPointer<Font> fontAsset);

        void SetFontStyle(UIElement styleSetElement, TextUtil.FontStyle fontStyle);

        void SetTextAnchor(UIElement styleSetElement, TextUtil.TextAnchor textAnchor);

        void SetOverflowX(UIElement styleSetElement, Overflow overflowX);

    }

}