
namespace UIForia.Rendering {
    
    public partial struct UIStyleSetStateProxy {
        
        public UIForia.Rendering.Overflow OverflowX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state); }
        }
        
        public UIForia.Rendering.Overflow OverflowY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state); }
        }
        
        public UnityEngine.Color BorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state); }
        }
        
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state); }
        }
        
        public UnityEngine.Color BackgroundColorSecondary {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundColorSecondary, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundColorSecondary, value), state); }
        }
        
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Texture2D BackgroundImage1 {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage1, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage1, 0, 0, value), state); }
        }
        
        public UnityEngine.Texture2D BackgroundImage2 {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage2, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage2, 0, 0, value), state); }
        }
        
        public Shapes2D.GradientType BackgroundGradientType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundGradientType, state).AsGradientType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientType, (int)value), state); }
        }
        
        public Shapes2D.GradientAxis BackgroundGradientAxis {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundGradientAxis, state).AsGradientAxis; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientAxis, (int)value), state); }
        }
        
        public float BackgroundGradientStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundGradientStart, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientStart, value), state); }
        }
        
        public float BackgroundFillRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillRotation, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillRotation, value), state); }
        }
        
        public UIForia.Rendering.BackgroundFillType BackgroundFillType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillType, state).AsBackgroundFillType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillType, (int)value), state); }
        }
        
        public UIForia.Rendering.BackgroundShapeType BackgroundShapeType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundShapeType, state).AsBackgroundShapeType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundShapeType, (int)value), state); }
        }
        
        public float BackgroundFillOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillOffsetX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetX, value), state); }
        }
        
        public float BackgroundFillOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillOffsetY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetY, value), state); }
        }
        
        public float BackgroundFillScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillScaleX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleX, value), state); }
        }
        
        public float BackgroundFillScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFillScaleY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleY, value), state); }
        }
        
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state); }
        }
        
        public UnityEngine.Texture2D Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Cursor, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Cursor, 0, 0, value), state); }
        }
        
        public UIForia.Rendering.Visibility Visibility {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Visibility, state).AsVisibility; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state); }
        }
        
        public int FlexItemOrder {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemOrder, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemOrder, value), state); }
        }
        
        public int FlexItemGrow {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value), state); }
        }
        
        public int FlexItemShrink {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value), state); }
        }
        
        public UIForia.Layout.CrossAxisAlignment FlexItemSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemSelfAlignment, state).AsCrossAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int)value), state); }
        }
        
        public UIForia.Rendering.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state); }
        }
        
        public UIForia.Rendering.LayoutWrap FlexLayoutWrap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state); }
        }
        
        public UIForia.Layout.MainAxisAlignment FlexLayoutMainAxisAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutMainAxisAlignment, state).AsMainAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value), state); }
        }
        
        public UIForia.Layout.CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutCrossAxisAlignment, state).AsCrossAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value), state); }
        }
        
        public int GridItemColStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemColStart, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemColStart, value), state); }
        }
        
        public int GridItemColSpan {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemColSpan, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemColSpan, value), state); }
        }
        
        public int GridItemRowStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemRowStart, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemRowStart, value), state); }
        }
        
        public int GridItemRowSpan {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemRowSpan, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemRowSpan, value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridItemColSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemColSelfAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemColSelfAlignment, (int)value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridItemRowSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemRowSelfAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, (int)value), state); }
        }
        
        public UIForia.Rendering.LayoutDirection GridLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state); }
        }
        
        public UIForia.Layout.GridLayoutDensity GridLayoutDensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutColAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTrackSize; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutRowAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAutoSize, state).AsGridTrackSize; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state); }
        }
        
        public float GridLayoutColGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value), state); }
        }
        
        public float GridLayoutRowGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state); }
        }
        
        public UIForia.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state); }
        }
        
        public UIForia.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state); }
        }
        
        public UIForia.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state); }
        }
        
        public UIForia.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state); }
        }
        
        public UIForia.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state); }
        }
        
        public UIForia.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state); }
        }
        
        public UIForia.UIMeasurement MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state); }
        }
        
        public UIForia.UIMeasurement MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state); }
        }
        
        public UIForia.UIMeasurement MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state); }
        }
        
        public UIForia.UIMeasurement MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state); }
        }
        
        public UIForia.UIFixedLength BorderLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state); }
        }
        
        public UnityEngine.Color TextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state); }
        }
        
        public TMPro.TMP_FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, 0, 0, value), state); }
        }
        
        public int TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state); }
        }
        
        public UIForia.Text.FontStyle TextFontStyle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state); }
        }
        
        public UIForia.Text.TextAlignment TextAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state); }
        }
        
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state); }
        }
        
        public UIForia.UIFixedLength AnchorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value), state); }
        }
        
        public UIForia.UIFixedLength AnchorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value), state); }
        }
        
        public UIForia.UIFixedLength AnchorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value), state); }
        }
        
        public UIForia.UIFixedLength AnchorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value), state); }
        }
        
        public UIForia.Rendering.AnchorTarget AnchorTarget {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorTarget, state).AsAnchorTarget; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value), state); }
        }
        
        public UIForia.UIFixedLength TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state); }
        }
        
        public UIForia.UIFixedLength TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state); }
        }
        
        public UIForia.UIFixedLength TransformPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state); }
        }
        
        public UIForia.UIFixedLength TransformPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state); }
        }
        
        public float TransformScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value), state); }
        }
        
        public float TransformScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value), state); }
        }
        
        public float TransformRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformRotation, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value), state); }
        }
        
        public UIForia.Rendering.TransformBehavior TransformBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsTransformBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value), state); }
        }
        
        public UIForia.Rendering.TransformBehavior TransformBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformBehaviorY, state).AsTransformBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value), state); }
        }
        
        public UIForia.Rendering.LayoutType LayoutType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutBehavior LayoutBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutBehavior, state).AsLayoutBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state); }
        }
        
        public int ZIndex {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ZIndex, value), state); }
        }
        
        public int RenderLayerOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value), state); }
        }
        
        public UIForia.Rendering.RenderLayer RenderLayer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state); }
        }
        
        public UIForia.Rendering.VerticalScrollbarAttachment ScrollbarVerticalAttachment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalAttachment, state).AsVerticalScrollbarAttachment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalAttachment, (int)value), state); }
        }
        
        public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarVerticalButtonPlacement {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalButtonPlacement, state).AsScrollbarButtonPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalButtonPlacement, (int)value), state); }
        }
        
        public float ScrollbarVerticalTrackSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackSize, value), state); }
        }
        
        public float ScrollbarVerticalTrackBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius, value), state); }
        }
        
        public float ScrollbarVerticalTrackBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalTrackBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarVerticalTrackImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalTrackColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackColor, value), state); }
        }
        
        public float ScrollbarVerticalHandleSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleSize, value), state); }
        }
        
        public float ScrollbarVerticalHandleBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius, value), state); }
        }
        
        public float ScrollbarVerticalHandleBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalHandleBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarVerticalHandleImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalHandleColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleColor, value), state); }
        }
        
        public float ScrollbarVerticalIncrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementSize, value), state); }
        }
        
        public float ScrollbarVerticalIncrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, value), state); }
        }
        
        public float ScrollbarVerticalIncrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalIncrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarVerticalIncrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalIncrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementColor, value), state); }
        }
        
        public float ScrollbarVerticalDecrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementSize, value), state); }
        }
        
        public float ScrollbarVerticalDecrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, value), state); }
        }
        
        public float ScrollbarVerticalDecrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalDecrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarVerticalDecrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarVerticalDecrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementColor, value), state); }
        }
        
        public UIForia.Rendering.HorizontalScrollbarAttachment ScrollbarHorizontalAttachment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalAttachment, state).AsHorizontalScrollbarAttachment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalAttachment, (int)value), state); }
        }
        
        public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarHorizontalButtonPlacement {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalButtonPlacement, state).AsScrollbarButtonPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement, (int)value), state); }
        }
        
        public float ScrollbarHorizontalTrackSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackSize, value), state); }
        }
        
        public float ScrollbarHorizontalTrackBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, value), state); }
        }
        
        public float ScrollbarHorizontalTrackBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalTrackBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarHorizontalTrackImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalTrackColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackColor, value), state); }
        }
        
        public float ScrollbarHorizontalHandleSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleSize, value), state); }
        }
        
        public float ScrollbarHorizontalHandleBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, value), state); }
        }
        
        public float ScrollbarHorizontalHandleBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalHandleBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarHorizontalHandleImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalHandleColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleColor, value), state); }
        }
        
        public float ScrollbarHorizontalIncrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementSize, value), state); }
        }
        
        public float ScrollbarHorizontalIncrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, value), state); }
        }
        
        public float ScrollbarHorizontalIncrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalIncrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarHorizontalIncrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalIncrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementColor, value), state); }
        }
        
        public float ScrollbarHorizontalDecrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementSize, value), state); }
        }
        
        public float ScrollbarHorizontalDecrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, value), state); }
        }
        
        public float ScrollbarHorizontalDecrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalDecrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, value), state); }
        }
        
        public UnityEngine.Texture2D ScrollbarHorizontalDecrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementImage, 0, 0, value), state); }
        }
        
        public UnityEngine.Color ScrollbarHorizontalDecrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementColor, value), state); }
        }
        
    }

    public partial struct StyleProperty {

        public bool IsUnset {
            get { 
                switch(propertyId) {
                                        case StylePropertyId.OverflowX: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.OverflowY: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.BorderColor: return valuePart1 == 0;
                    case StylePropertyId.BackgroundColor: return valuePart1 == 0;
                    case StylePropertyId.BackgroundColorSecondary: return valuePart1 == 0;
                    case StylePropertyId.BackgroundImage: return objectField == null;
                    case StylePropertyId.BackgroundImage1: return objectField == null;
                    case StylePropertyId.BackgroundImage2: return objectField == null;
                    case StylePropertyId.BackgroundGradientType: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.BackgroundGradientAxis: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.BackgroundGradientStart: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.BackgroundFillRotation: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.BackgroundFillType: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.BackgroundShapeType: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.BackgroundFillOffsetX: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.BackgroundFillOffsetY: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.BackgroundFillScaleX: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.BackgroundFillScaleY: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.Opacity: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.Cursor: return objectField == null;
                    case StylePropertyId.Visibility: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.FlexItemOrder: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.FlexItemGrow: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.FlexItemShrink: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.FlexItemSelfAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.FlexLayoutDirection: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.FlexLayoutWrap: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.FlexLayoutMainAxisAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.FlexLayoutCrossAxisAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridItemColStart: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.GridItemColSpan: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.GridItemRowStart: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.GridItemRowSpan: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.GridItemColSelfAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridItemRowSelfAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridLayoutDirection: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridLayoutDensity: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridLayoutColTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutRowTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutColAutoSize: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.GridLayoutRowAutoSize: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.GridLayoutColGap: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.GridLayoutRowGap: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.GridLayoutColAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.GridLayoutRowAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.MinWidth: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MaxWidth: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PreferredWidth: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MinHeight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MaxHeight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PreferredHeight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MarginTop: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MarginRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MarginBottom: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.MarginLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderTop: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderBottom: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderRadiusTopLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderRadiusTopRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderRadiusBottomRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.BorderRadiusBottomLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PaddingTop: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PaddingRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PaddingBottom: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.PaddingLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.TextColor: return valuePart1 == 0;
                    case StylePropertyId.TextFontAsset: return objectField == null;
                    case StylePropertyId.TextFontSize: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.TextFontStyle: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.TextAlignment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.TextTransform: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.AnchorTop: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.AnchorRight: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.AnchorBottom: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.AnchorLeft: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.AnchorTarget: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.TransformPositionX: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.TransformPositionY: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.TransformPivotX: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.TransformPivotY: return !FloatUtil.IsDefined(floatValue) || valuePart1 == 0;
                    case StylePropertyId.TransformScaleX: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.TransformScaleY: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.TransformRotation: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.TransformBehaviorX: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.TransformBehaviorY: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.LayoutType: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.LayoutBehavior: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ZIndex: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.RenderLayerOffset: return !IntUtil.IsDefined(valuePart0);
                    case StylePropertyId.RenderLayer: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ScrollbarVerticalAttachment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ScrollbarVerticalButtonPlacement: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ScrollbarVerticalTrackSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalTrackBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalTrackBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalTrackBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalTrackImage: return objectField == null;
                    case StylePropertyId.ScrollbarVerticalTrackColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalHandleSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalHandleBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalHandleBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalHandleBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalHandleImage: return objectField == null;
                    case StylePropertyId.ScrollbarVerticalHandleColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalIncrementSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalIncrementBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalIncrementBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalIncrementBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalIncrementImage: return objectField == null;
                    case StylePropertyId.ScrollbarVerticalIncrementColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalDecrementSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalDecrementBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalDecrementBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarVerticalDecrementBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarVerticalDecrementImage: return objectField == null;
                    case StylePropertyId.ScrollbarVerticalDecrementColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalAttachment: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ScrollbarHorizontalButtonPlacement: return valuePart0 == 0 || IntUtil.UnsetValue == valuePart0;
                    case StylePropertyId.ScrollbarHorizontalTrackSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalTrackBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalTrackBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalTrackBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalTrackImage: return objectField == null;
                    case StylePropertyId.ScrollbarHorizontalTrackColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalHandleSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalHandleBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalHandleBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalHandleBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalHandleImage: return objectField == null;
                    case StylePropertyId.ScrollbarHorizontalHandleColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalIncrementSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalIncrementBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalIncrementBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalIncrementBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalIncrementImage: return objectField == null;
                    case StylePropertyId.ScrollbarHorizontalIncrementColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalDecrementSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalDecrementBorderRadius: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalDecrementBorderSize: return !FloatUtil.IsDefined(floatValue);
                    case StylePropertyId.ScrollbarHorizontalDecrementBorderColor: return valuePart1 == 0;
                    case StylePropertyId.ScrollbarHorizontalDecrementImage: return objectField == null;
                    case StylePropertyId.ScrollbarHorizontalDecrementColor: return valuePart1 == 0;

                }
                return true;
            }
        }

    }

    public partial class UIStyle {
    
        
        public UIForia.Rendering.Overflow OverflowX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Overflow)FindEnumProperty(StylePropertyId.OverflowX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value)); }
        }
            
        public UIForia.Rendering.Overflow OverflowY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Overflow)FindEnumProperty(StylePropertyId.OverflowY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value)); }
        }
            
        public UnityEngine.Color BorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColor, value)); }
        }
            
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value)); }
        }
            
        public UnityEngine.Color BackgroundColorSecondary {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundColorSecondary); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundColorSecondary, value)); }
        }
            
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, 0, 0, value)); }
        }
            
        public UnityEngine.Texture2D BackgroundImage1 {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage1).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage1, 0, 0, value)); }
        }
            
        public UnityEngine.Texture2D BackgroundImage2 {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage2).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage2, 0, 0, value)); }
        }
            
        public Shapes2D.GradientType BackgroundGradientType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (Shapes2D.GradientType)FindEnumProperty(StylePropertyId.BackgroundGradientType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientType, (int)value)); }
        }
            
        public Shapes2D.GradientAxis BackgroundGradientAxis {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (Shapes2D.GradientAxis)FindEnumProperty(StylePropertyId.BackgroundGradientAxis); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientAxis, (int)value)); }
        }
            
        public float BackgroundGradientStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundGradientStart); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientStart, value)); }
        }
            
        public float BackgroundFillRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundFillRotation); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillRotation, value)); }
        }
            
        public UIForia.Rendering.BackgroundFillType BackgroundFillType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.BackgroundFillType)FindEnumProperty(StylePropertyId.BackgroundFillType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillType, (int)value)); }
        }
            
        public UIForia.Rendering.BackgroundShapeType BackgroundShapeType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.BackgroundShapeType)FindEnumProperty(StylePropertyId.BackgroundShapeType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundShapeType, (int)value)); }
        }
            
        public float BackgroundFillOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetX, value)); }
        }
            
        public float BackgroundFillOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetY, value)); }
        }
            
        public float BackgroundFillScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleX, value)); }
        }
            
        public float BackgroundFillScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleY, value)); }
        }
            
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.Opacity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Opacity, value)); }
        }
            
        public UnityEngine.Texture2D Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Cursor).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Cursor, 0, 0, value)); }
        }
            
        public UIForia.Rendering.Visibility Visibility {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Visibility)FindEnumProperty(StylePropertyId.Visibility); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value)); }
        }
            
        public int FlexItemOrder {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemOrder); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemOrder, value)); }
        }
            
        public int FlexItemGrow {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemGrow); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value)); }
        }
            
        public int FlexItemShrink {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemShrink); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value)); }
        }
            
        public UIForia.Layout.CrossAxisAlignment FlexItemSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.CrossAxisAlignment)FindEnumProperty(StylePropertyId.FlexItemSelfAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int)value)); }
        }
            
        public UIForia.Rendering.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.LayoutDirection)FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value)); }
        }
            
        public UIForia.Rendering.LayoutWrap FlexLayoutWrap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.LayoutWrap)FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value)); }
        }
            
        public UIForia.Layout.MainAxisAlignment FlexLayoutMainAxisAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.MainAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value)); }
        }
            
        public UIForia.Layout.CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.CrossAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value)); }
        }
            
        public int GridItemColStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.GridItemColStart); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemColStart, value)); }
        }
            
        public int GridItemColSpan {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.GridItemColSpan); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemColSpan, value)); }
        }
            
        public int GridItemRowStart {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.GridItemRowStart); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemRowStart, value)); }
        }
            
        public int GridItemRowSpan {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.GridItemRowSpan); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemRowSpan, value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridItemColSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridItemColSelfAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemColSelfAlignment, (int)value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridItemRowSelfAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridItemRowSelfAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, (int)value)); }
        }
            
        public UIForia.Rendering.LayoutDirection GridLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.LayoutDirection)FindEnumProperty(StylePropertyId.GridLayoutDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value)); }
        }
            
        public UIForia.Layout.GridLayoutDensity GridLayoutDensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridLayoutDensity)FindEnumProperty(StylePropertyId.GridLayoutDensity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutColTemplate).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutRowTemplate).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutColAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutRowAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value)); }
        }
            
        public float GridLayoutColGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutColGap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value)); }
        }
            
        public float GridLayoutRowGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutRowGap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutColAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutRowAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value)); }
        }
            
        public UIForia.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinWidth, value)); }
        }
            
        public UIForia.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value)); }
        }
            
        public UIForia.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value)); }
        }
            
        public UIForia.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinHeight, value)); }
        }
            
        public UIForia.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value)); }
        }
            
        public UIForia.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value)); }
        }
            
        public UIForia.UIMeasurement MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginTop, value)); }
        }
            
        public UIForia.UIMeasurement MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginRight, value)); }
        }
            
        public UIForia.UIMeasurement MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value)); }
        }
            
        public UIForia.UIMeasurement MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderTop, value)); }
        }
            
        public UIForia.UIFixedLength BorderRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value)); }
        }
            
        public UIForia.UIFixedLength BorderLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value)); }
        }
            
        public UIForia.UIFixedLength PaddingTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value)); }
        }
            
        public UIForia.UIFixedLength PaddingRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value)); }
        }
            
        public UIForia.UIFixedLength PaddingBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value)); }
        }
            
        public UIForia.UIFixedLength PaddingLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value)); }
        }
            
        public UnityEngine.Color TextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextColor, value)); }
        }
            
        public TMPro.TMP_FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.TextFontAsset).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, 0, 0, value)); }
        }
            
        public int TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.TextFontSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value)); }
        }
            
        public UIForia.Text.FontStyle TextFontStyle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.FontStyle)FindEnumProperty(StylePropertyId.TextFontStyle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value)); }
        }
            
        public UIForia.Text.TextAlignment TextAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.TextAlignment)FindEnumProperty(StylePropertyId.TextAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value)); }
        }
            
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.TextTransform)FindEnumProperty(StylePropertyId.TextTransform); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value)); }
        }
            
        public UIForia.UIFixedLength AnchorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value)); }
        }
            
        public UIForia.UIFixedLength AnchorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value)); }
        }
            
        public UIForia.UIFixedLength AnchorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value)); }
        }
            
        public UIForia.UIFixedLength AnchorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value)); }
        }
            
        public UIForia.Rendering.AnchorTarget AnchorTarget {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.AnchorTarget)FindEnumProperty(StylePropertyId.AnchorTarget); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value)); }
        }
            
        public UIForia.UIFixedLength TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPositionX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value)); }
        }
            
        public UIForia.UIFixedLength TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPositionY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value)); }
        }
            
        public UIForia.UIFixedLength TransformPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value)); }
        }
            
        public UIForia.UIFixedLength TransformPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value)); }
        }
            
        public float TransformScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value)); }
        }
            
        public float TransformScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value)); }
        }
            
        public float TransformRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformRotation); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value)); }
        }
            
        public UIForia.Rendering.TransformBehavior TransformBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value)); }
        }
            
        public UIForia.Rendering.TransformBehavior TransformBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value)); }
        }
            
        public UIForia.Rendering.LayoutType LayoutType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.LayoutType)FindEnumProperty(StylePropertyId.LayoutType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value)); }
        }
            
        public UIForia.Layout.LayoutBehavior LayoutBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutBehavior)FindEnumProperty(StylePropertyId.LayoutBehavior); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value)); }
        }
            
        public int ZIndex {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.ZIndex); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ZIndex, value)); }
        }
            
        public int RenderLayerOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.RenderLayerOffset); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value)); }
        }
            
        public UIForia.Rendering.RenderLayer RenderLayer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.RenderLayer)FindEnumProperty(StylePropertyId.RenderLayer); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value)); }
        }
            
        public UIForia.Rendering.VerticalScrollbarAttachment ScrollbarVerticalAttachment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.VerticalScrollbarAttachment)FindEnumProperty(StylePropertyId.ScrollbarVerticalAttachment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalAttachment, (int)value)); }
        }
            
        public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarVerticalButtonPlacement {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.ScrollbarButtonPlacement)FindEnumProperty(StylePropertyId.ScrollbarVerticalButtonPlacement); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalButtonPlacement, (int)value)); }
        }
            
        public float ScrollbarVerticalTrackSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackSize, value)); }
        }
            
        public float ScrollbarVerticalTrackBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius, value)); }
        }
            
        public float ScrollbarVerticalTrackBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalTrackBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarVerticalTrackImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarVerticalTrackImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalTrackColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalTrackColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackColor, value)); }
        }
            
        public float ScrollbarVerticalHandleSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleSize, value)); }
        }
            
        public float ScrollbarVerticalHandleBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius, value)); }
        }
            
        public float ScrollbarVerticalHandleBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalHandleBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarVerticalHandleImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarVerticalHandleImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalHandleColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalHandleColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleColor, value)); }
        }
            
        public float ScrollbarVerticalIncrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementSize, value)); }
        }
            
        public float ScrollbarVerticalIncrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, value)); }
        }
            
        public float ScrollbarVerticalIncrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalIncrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarVerticalIncrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarVerticalIncrementImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalIncrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalIncrementColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementColor, value)); }
        }
            
        public float ScrollbarVerticalDecrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementSize, value)); }
        }
            
        public float ScrollbarVerticalDecrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, value)); }
        }
            
        public float ScrollbarVerticalDecrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalDecrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarVerticalDecrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarVerticalDecrementImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarVerticalDecrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarVerticalDecrementColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementColor, value)); }
        }
            
        public UIForia.Rendering.HorizontalScrollbarAttachment ScrollbarHorizontalAttachment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.HorizontalScrollbarAttachment)FindEnumProperty(StylePropertyId.ScrollbarHorizontalAttachment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalAttachment, (int)value)); }
        }
            
        public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarHorizontalButtonPlacement {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.ScrollbarButtonPlacement)FindEnumProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement, (int)value)); }
        }
            
        public float ScrollbarHorizontalTrackSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackSize, value)); }
        }
            
        public float ScrollbarHorizontalTrackBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, value)); }
        }
            
        public float ScrollbarHorizontalTrackBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalTrackBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarHorizontalTrackImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarHorizontalTrackImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalTrackColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalTrackColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackColor, value)); }
        }
            
        public float ScrollbarHorizontalHandleSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleSize, value)); }
        }
            
        public float ScrollbarHorizontalHandleBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, value)); }
        }
            
        public float ScrollbarHorizontalHandleBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalHandleBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarHorizontalHandleImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarHorizontalHandleImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalHandleColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalHandleColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleColor, value)); }
        }
            
        public float ScrollbarHorizontalIncrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementSize, value)); }
        }
            
        public float ScrollbarHorizontalIncrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, value)); }
        }
            
        public float ScrollbarHorizontalIncrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalIncrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarHorizontalIncrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarHorizontalIncrementImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalIncrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalIncrementColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementColor, value)); }
        }
            
        public float ScrollbarHorizontalDecrementSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementSize, value)); }
        }
            
        public float ScrollbarHorizontalDecrementBorderRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, value)); }
        }
            
        public float ScrollbarHorizontalDecrementBorderSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalDecrementBorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, value)); }
        }
            
        public UnityEngine.Texture2D ScrollbarHorizontalDecrementImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.ScrollbarHorizontalDecrementImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementImage, 0, 0, value)); }
        }
            
        public UnityEngine.Color ScrollbarHorizontalDecrementColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalDecrementColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementColor, value)); }
        }
            
        
    }

    public partial class UIStyleSet {
    
        

            public UIForia.Rendering.Overflow OverflowX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.OverflowX, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowX;
                }
            }

            public UIForia.Rendering.Overflow OverflowY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.OverflowY, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowY;
                }
            }

            public UnityEngine.Color BorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColor;
                }
            }

            public UnityEngine.Color BackgroundColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundColor;
                }
            }

            public UnityEngine.Color BackgroundColorSecondary { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundColorSecondary, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundColorSecondary;
                }
            }

            public UnityEngine.Texture2D BackgroundImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage;
                }
            }

            public UnityEngine.Texture2D BackgroundImage1 { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundImage1, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage1;
                }
            }

            public UnityEngine.Texture2D BackgroundImage2 { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundImage2, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage2;
                }
            }

            public Shapes2D.GradientType BackgroundGradientType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundGradientType, out property)) return property.AsGradientType;
                    return DefaultStyleValues_Generated.BackgroundGradientType;
                }
            }

            public Shapes2D.GradientAxis BackgroundGradientAxis { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundGradientAxis, out property)) return property.AsGradientAxis;
                    return DefaultStyleValues_Generated.BackgroundGradientAxis;
                }
            }

            public float BackgroundGradientStart { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundGradientStart, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundGradientStart;
                }
            }

            public float BackgroundFillRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundFillRotation;
                }
            }

            public UIForia.Rendering.BackgroundFillType BackgroundFillType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillType, out property)) return property.AsBackgroundFillType;
                    return DefaultStyleValues_Generated.BackgroundFillType;
                }
            }

            public UIForia.Rendering.BackgroundShapeType BackgroundShapeType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundShapeType, out property)) return property.AsBackgroundShapeType;
                    return DefaultStyleValues_Generated.BackgroundShapeType;
                }
            }

            public float BackgroundFillOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillOffsetX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundFillOffsetX;
                }
            }

            public float BackgroundFillOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillOffsetY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundFillOffsetY;
                }
            }

            public float BackgroundFillScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundFillScaleX;
                }
            }

            public float BackgroundFillScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BackgroundFillScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundFillScaleY;
                }
            }

            public float Opacity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.Opacity, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.Opacity;
                }
            }

            public UnityEngine.Texture2D Cursor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.Cursor, out property)) return property.AsTexture2D;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Cursor), out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.Cursor;
                }
            }

            public UIForia.Rendering.Visibility Visibility { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.Visibility, out property)) return property.AsVisibility;
                    return DefaultStyleValues_Generated.Visibility;
                }
            }

            public int FlexItemOrder { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexItemOrder, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemOrder;
                }
            }

            public int FlexItemGrow { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexItemGrow, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemGrow;
                }
            }

            public int FlexItemShrink { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexItemShrink, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemShrink;
                }
            }

            public UIForia.Layout.CrossAxisAlignment FlexItemSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexItemSelfAlignment, out property)) return property.AsCrossAxisAlignment;
                    return DefaultStyleValues_Generated.FlexItemSelfAlignment;
                }
            }

            public UIForia.Rendering.LayoutDirection FlexLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.FlexLayoutDirection;
                }
            }

            public UIForia.Rendering.LayoutWrap FlexLayoutWrap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexLayoutWrap, out property)) return property.AsLayoutWrap;
                    return DefaultStyleValues_Generated.FlexLayoutWrap;
                }
            }

            public UIForia.Layout.MainAxisAlignment FlexLayoutMainAxisAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexLayoutMainAxisAlignment, out property)) return property.AsMainAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutMainAxisAlignment;
                }
            }

            public UIForia.Layout.CrossAxisAlignment FlexLayoutCrossAxisAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.FlexLayoutCrossAxisAlignment, out property)) return property.AsCrossAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutCrossAxisAlignment;
                }
            }

            public int GridItemColStart { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemColStart, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemColStart;
                }
            }

            public int GridItemColSpan { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemColSpan, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemColSpan;
                }
            }

            public int GridItemRowStart { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemRowStart, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemRowStart;
                }
            }

            public int GridItemRowSpan { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemRowSpan, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemRowSpan;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridItemColSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemColSelfAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridItemColSelfAlignment;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridItemRowSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridItemRowSelfAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridItemRowSelfAlignment;
                }
            }

            public UIForia.Rendering.LayoutDirection GridLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.GridLayoutDirection;
                }
            }

            public UIForia.Layout.GridLayoutDensity GridLayoutDensity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutDensity, out property)) return property.AsGridLayoutDensity;
                    return DefaultStyleValues_Generated.GridLayoutDensity;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutColTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutColTemplate;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutRowTemplate;
                }
            }

            public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutColAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutColAutoSize;
                }
            }

            public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutRowAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutRowAutoSize;
                }
            }

            public float GridLayoutColGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutColGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutColGap;
                }
            }

            public float GridLayoutRowGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutRowGap;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutColAlignment;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutRowAlignment;
                }
            }

            public UIForia.UIMeasurement MinWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MinWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinWidth;
                }
            }

            public UIForia.UIMeasurement MaxWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MaxWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxWidth;
                }
            }

            public UIForia.UIMeasurement PreferredWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PreferredWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredWidth;
                }
            }

            public UIForia.UIMeasurement MinHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MinHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinHeight;
                }
            }

            public UIForia.UIMeasurement MaxHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MaxHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxHeight;
                }
            }

            public UIForia.UIMeasurement PreferredHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PreferredHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredHeight;
                }
            }

            public UIForia.UIMeasurement MarginTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MarginTop, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginTop;
                }
            }

            public UIForia.UIMeasurement MarginRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MarginRight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginRight;
                }
            }

            public UIForia.UIMeasurement MarginBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MarginBottom, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginBottom;
                }
            }

            public UIForia.UIMeasurement MarginLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.MarginLeft, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginLeft;
                }
            }

            public UIForia.UIFixedLength BorderTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderTop;
                }
            }

            public UIForia.UIFixedLength BorderRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRight;
                }
            }

            public UIForia.UIFixedLength BorderBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderBottom;
                }
            }

            public UIForia.UIFixedLength BorderLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomLeft;
                }
            }

            public UIForia.UIFixedLength PaddingTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PaddingTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingTop;
                }
            }

            public UIForia.UIFixedLength PaddingRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PaddingRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingRight;
                }
            }

            public UIForia.UIFixedLength PaddingBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PaddingBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingBottom;
                }
            }

            public UIForia.UIFixedLength PaddingLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.PaddingLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingLeft;
                }
            }

            public UnityEngine.Color TextColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextColor, out property)) return property.AsColor;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextColor;
                }
            }

            public TMPro.TMP_FontAsset TextFontAsset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextFontAsset, out property)) return property.AsFont;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontAsset), out property)) return property.AsFont;
                    return DefaultStyleValues_Generated.TextFontAsset;
                }
            }

            public int TextFontSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextFontSize, out property)) return property.AsInt;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontSize), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.TextFontSize;
                }
            }

            public UIForia.Text.FontStyle TextFontStyle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextFontStyle, out property)) return property.AsFontStyle;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontStyle), out property)) return property.AsFontStyle;
                    return DefaultStyleValues_Generated.TextFontStyle;
                }
            }

            public UIForia.Text.TextAlignment TextAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextAlignment, out property)) return property.AsTextAlignment;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextAlignment), out property)) return property.AsTextAlignment;
                    return DefaultStyleValues_Generated.TextAlignment;
                }
            }

            public UIForia.Text.TextTransform TextTransform { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TextTransform, out property)) return property.AsTextTransform;
                    if (m_PropertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextTransform), out property)) return property.AsTextTransform;
                    return DefaultStyleValues_Generated.TextTransform;
                }
            }

            public UIForia.UIFixedLength AnchorTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.AnchorTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorTop;
                }
            }

            public UIForia.UIFixedLength AnchorRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.AnchorRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorRight;
                }
            }

            public UIForia.UIFixedLength AnchorBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.AnchorBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorBottom;
                }
            }

            public UIForia.UIFixedLength AnchorLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.AnchorLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorLeft;
                }
            }

            public UIForia.Rendering.AnchorTarget AnchorTarget { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.AnchorTarget, out property)) return property.AsAnchorTarget;
                    return DefaultStyleValues_Generated.AnchorTarget;
                }
            }

            public UIForia.UIFixedLength TransformPositionX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformPositionX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPositionX;
                }
            }

            public UIForia.UIFixedLength TransformPositionY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformPositionY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPositionY;
                }
            }

            public UIForia.UIFixedLength TransformPivotX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformPivotX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotX;
                }
            }

            public UIForia.UIFixedLength TransformPivotY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformPivotY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotY;
                }
            }

            public float TransformScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleX;
                }
            }

            public float TransformScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleY;
                }
            }

            public float TransformRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformRotation;
                }
            }

            public UIForia.Rendering.TransformBehavior TransformBehaviorX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorX, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorX;
                }
            }

            public UIForia.Rendering.TransformBehavior TransformBehaviorY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorY, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorY;
                }
            }

            public UIForia.Rendering.LayoutType LayoutType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.LayoutType, out property)) return property.AsLayoutType;
                    return DefaultStyleValues_Generated.LayoutType;
                }
            }

            public UIForia.Layout.LayoutBehavior LayoutBehavior { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.LayoutBehavior, out property)) return property.AsLayoutBehavior;
                    return DefaultStyleValues_Generated.LayoutBehavior;
                }
            }

            public int ZIndex { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ZIndex, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.ZIndex;
                }
            }

            public int RenderLayerOffset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.RenderLayerOffset, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.RenderLayerOffset;
                }
            }

            public UIForia.Rendering.RenderLayer RenderLayer { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.RenderLayer, out property)) return property.AsRenderLayer;
                    return DefaultStyleValues_Generated.RenderLayer;
                }
            }

            public UIForia.Rendering.VerticalScrollbarAttachment ScrollbarVerticalAttachment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalAttachment, out property)) return property.AsVerticalScrollbarAttachment;
                    return DefaultStyleValues_Generated.ScrollbarVerticalAttachment;
                }
            }

            public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarVerticalButtonPlacement { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalButtonPlacement, out property)) return property.AsScrollbarButtonPlacement;
                    return DefaultStyleValues_Generated.ScrollbarVerticalButtonPlacement;
                }
            }

            public float ScrollbarVerticalTrackSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackSize;
                }
            }

            public float ScrollbarVerticalTrackBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackBorderRadius;
                }
            }

            public float ScrollbarVerticalTrackBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarVerticalTrackBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarVerticalTrackImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackImage;
                }
            }

            public UnityEngine.Color ScrollbarVerticalTrackColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalTrackColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalTrackColor;
                }
            }

            public float ScrollbarVerticalHandleSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleSize;
                }
            }

            public float ScrollbarVerticalHandleBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleBorderRadius;
                }
            }

            public float ScrollbarVerticalHandleBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarVerticalHandleBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarVerticalHandleImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleImage;
                }
            }

            public UnityEngine.Color ScrollbarVerticalHandleColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalHandleColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalHandleColor;
                }
            }

            public float ScrollbarVerticalIncrementSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementSize;
                }
            }

            public float ScrollbarVerticalIncrementBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementBorderRadius;
                }
            }

            public float ScrollbarVerticalIncrementBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarVerticalIncrementBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarVerticalIncrementImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementImage;
                }
            }

            public UnityEngine.Color ScrollbarVerticalIncrementColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalIncrementColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalIncrementColor;
                }
            }

            public float ScrollbarVerticalDecrementSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementSize;
                }
            }

            public float ScrollbarVerticalDecrementBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementBorderRadius;
                }
            }

            public float ScrollbarVerticalDecrementBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarVerticalDecrementBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarVerticalDecrementImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementImage;
                }
            }

            public UnityEngine.Color ScrollbarVerticalDecrementColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarVerticalDecrementColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarVerticalDecrementColor;
                }
            }

            public UIForia.Rendering.HorizontalScrollbarAttachment ScrollbarHorizontalAttachment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalAttachment, out property)) return property.AsHorizontalScrollbarAttachment;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalAttachment;
                }
            }

            public UIForia.Rendering.ScrollbarButtonPlacement ScrollbarHorizontalButtonPlacement { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalButtonPlacement, out property)) return property.AsScrollbarButtonPlacement;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalButtonPlacement;
                }
            }

            public float ScrollbarHorizontalTrackSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackSize;
                }
            }

            public float ScrollbarHorizontalTrackBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackBorderRadius;
                }
            }

            public float ScrollbarHorizontalTrackBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalTrackBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarHorizontalTrackImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackImage;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalTrackColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalTrackColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalTrackColor;
                }
            }

            public float ScrollbarHorizontalHandleSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleSize;
                }
            }

            public float ScrollbarHorizontalHandleBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleBorderRadius;
                }
            }

            public float ScrollbarHorizontalHandleBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalHandleBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarHorizontalHandleImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleImage;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalHandleColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalHandleColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalHandleColor;
                }
            }

            public float ScrollbarHorizontalIncrementSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementSize;
                }
            }

            public float ScrollbarHorizontalIncrementBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementBorderRadius;
                }
            }

            public float ScrollbarHorizontalIncrementBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalIncrementBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarHorizontalIncrementImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementImage;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalIncrementColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalIncrementColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalIncrementColor;
                }
            }

            public float ScrollbarHorizontalDecrementSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementSize;
                }
            }

            public float ScrollbarHorizontalDecrementBorderRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementBorderRadius;
                }
            }

            public float ScrollbarHorizontalDecrementBorderSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementBorderSize, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementBorderSize;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalDecrementBorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementBorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementBorderColor;
                }
            }

            public UnityEngine.Texture2D ScrollbarHorizontalDecrementImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementImage;
                }
            }

            public UnityEngine.Color ScrollbarHorizontalDecrementColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (m_PropertyMap.TryGetValue((int) StylePropertyId.ScrollbarHorizontalDecrementColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarHorizontalDecrementColor;
                }
            }

        
        public void SetOverflowX(UIForia.Rendering.Overflow value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state);
        }

        public UIForia.Rendering.Overflow GetOverflowX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow;
        }
        
        public void SetOverflowY(UIForia.Rendering.Overflow value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state);
        }

        public UIForia.Rendering.Overflow GetOverflowY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow;
        }
        
        public void SetBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state);
        }

        public UnityEngine.Color GetBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor;
        }
        
        public void SetBackgroundColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state);
        }

        public UnityEngine.Color GetBackgroundColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor;
        }
        
        public void SetBackgroundColorSecondary(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundColorSecondary, value), state);
        }

        public UnityEngine.Color GetBackgroundColorSecondary(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundColorSecondary, state).AsColor;
        }
        
        public void SetBackgroundImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetBackgroundImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D;
        }
        
        public void SetBackgroundImage1(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage1, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetBackgroundImage1(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImage1, state).AsTexture2D;
        }
        
        public void SetBackgroundImage2(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage2, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetBackgroundImage2(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImage2, state).AsTexture2D;
        }
        
        public void SetBackgroundGradientType(Shapes2D.GradientType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientType, (int)value), state);
        }

        public Shapes2D.GradientType GetBackgroundGradientType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundGradientType, state).AsGradientType;
        }
        
        public void SetBackgroundGradientAxis(Shapes2D.GradientAxis value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientAxis, (int)value), state);
        }

        public Shapes2D.GradientAxis GetBackgroundGradientAxis(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundGradientAxis, state).AsGradientAxis;
        }
        
        public void SetBackgroundGradientStart(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundGradientStart, value), state);
        }

        public float GetBackgroundGradientStart(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundGradientStart, state).AsFloat;
        }
        
        public void SetBackgroundFillRotation(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillRotation, value), state);
        }

        public float GetBackgroundFillRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillRotation, state).AsFloat;
        }
        
        public void SetBackgroundFillType(UIForia.Rendering.BackgroundFillType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillType, (int)value), state);
        }

        public UIForia.Rendering.BackgroundFillType GetBackgroundFillType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillType, state).AsBackgroundFillType;
        }
        
        public void SetBackgroundShapeType(UIForia.Rendering.BackgroundShapeType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundShapeType, (int)value), state);
        }

        public UIForia.Rendering.BackgroundShapeType GetBackgroundShapeType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundShapeType, state).AsBackgroundShapeType;
        }
        
        public void SetBackgroundFillOffsetX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetX, value), state);
        }

        public float GetBackgroundFillOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillOffsetX, state).AsFloat;
        }
        
        public void SetBackgroundFillOffsetY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillOffsetY, value), state);
        }

        public float GetBackgroundFillOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillOffsetY, state).AsFloat;
        }
        
        public void SetBackgroundFillScaleX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleX, value), state);
        }

        public float GetBackgroundFillScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillScaleX, state).AsFloat;
        }
        
        public void SetBackgroundFillScaleY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFillScaleY, value), state);
        }

        public float GetBackgroundFillScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFillScaleY, state).AsFloat;
        }
        
        public void SetOpacity(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state);
        }

        public float GetOpacity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat;
        }
        
        public void SetCursor(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Cursor, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetCursor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Cursor, state).AsTexture2D;
        }
        
        public void SetVisibility(UIForia.Rendering.Visibility value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state);
        }

        public UIForia.Rendering.Visibility GetVisibility(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Visibility, state).AsVisibility;
        }
        
        public void SetFlexItemOrder(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemOrder, value), state);
        }

        public int GetFlexItemOrder(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemOrder, state).AsInt;
        }
        
        public void SetFlexItemGrow(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value), state);
        }

        public int GetFlexItemGrow(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt;
        }
        
        public void SetFlexItemShrink(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value), state);
        }

        public int GetFlexItemShrink(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt;
        }
        
        public void SetFlexItemSelfAlignment(UIForia.Layout.CrossAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int)value), state);
        }

        public UIForia.Layout.CrossAxisAlignment GetFlexItemSelfAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemSelfAlignment, state).AsCrossAxisAlignment;
        }
        
        public void SetFlexLayoutDirection(UIForia.Rendering.LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state);
        }

        public UIForia.Rendering.LayoutDirection GetFlexLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetFlexLayoutWrap(UIForia.Rendering.LayoutWrap value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state);
        }

        public UIForia.Rendering.LayoutWrap GetFlexLayoutWrap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap;
        }
        
        public void SetFlexLayoutMainAxisAlignment(UIForia.Layout.MainAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value), state);
        }

        public UIForia.Layout.MainAxisAlignment GetFlexLayoutMainAxisAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutMainAxisAlignment, state).AsMainAxisAlignment;
        }
        
        public void SetFlexLayoutCrossAxisAlignment(UIForia.Layout.CrossAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value), state);
        }

        public UIForia.Layout.CrossAxisAlignment GetFlexLayoutCrossAxisAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutCrossAxisAlignment, state).AsCrossAxisAlignment;
        }
        
        public void SetGridItemColStart(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemColStart, value), state);
        }

        public int GetGridItemColStart(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemColStart, state).AsInt;
        }
        
        public void SetGridItemColSpan(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemColSpan, value), state);
        }

        public int GetGridItemColSpan(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemColSpan, state).AsInt;
        }
        
        public void SetGridItemRowStart(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemRowStart, value), state);
        }

        public int GetGridItemRowStart(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemRowStart, state).AsInt;
        }
        
        public void SetGridItemRowSpan(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemRowSpan, value), state);
        }

        public int GetGridItemRowSpan(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemRowSpan, state).AsInt;
        }
        
        public void SetGridItemColSelfAlignment(UIForia.Layout.GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemColSelfAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridItemColSelfAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemColSelfAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetGridItemRowSelfAlignment(UIForia.Layout.GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridItemRowSelfAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemRowSelfAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetGridLayoutDirection(UIForia.Rendering.LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state);
        }

        public UIForia.Rendering.LayoutDirection GetGridLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetGridLayoutDensity(UIForia.Layout.GridLayoutDensity value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state);
        }

        public UIForia.Layout.GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity;
        }
        
        public void SetGridLayoutColTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutColTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutRowTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutRowTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutColAutoSize(UIForia.Layout.LayoutTypes.GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridTrackSize GetGridLayoutColAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTrackSize;
        }
        
        public void SetGridLayoutRowAutoSize(UIForia.Layout.LayoutTypes.GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridTrackSize GetGridLayoutRowAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowAutoSize, state).AsGridTrackSize;
        }
        
        public void SetGridLayoutColGap(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value), state);
        }

        public float GetGridLayoutColGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat;
        }
        
        public void SetGridLayoutRowGap(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value), state);
        }

        public float GetGridLayoutRowGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat;
        }
        
        public void SetGridLayoutColAlignment(UIForia.Layout.GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridLayoutColAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetGridLayoutRowAlignment(UIForia.Layout.GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetMinWidth(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state);
        }

        public UIForia.UIMeasurement GetMinWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement;
        }
        
        public void SetMaxWidth(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state);
        }

        public UIForia.UIMeasurement GetMaxWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement;
        }
        
        public void SetPreferredWidth(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state);
        }

        public UIForia.UIMeasurement GetPreferredWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement;
        }
        
        public void SetMinHeight(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state);
        }

        public UIForia.UIMeasurement GetMinHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement;
        }
        
        public void SetMaxHeight(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state);
        }

        public UIForia.UIMeasurement GetMaxHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement;
        }
        
        public void SetPreferredHeight(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state);
        }

        public UIForia.UIMeasurement GetPreferredHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement;
        }
        
        public void SetMarginTop(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state);
        }

        public UIForia.UIMeasurement GetMarginTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement;
        }
        
        public void SetMarginRight(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state);
        }

        public UIForia.UIMeasurement GetMarginRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement;
        }
        
        public void SetMarginBottom(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state);
        }

        public UIForia.UIMeasurement GetMarginBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement;
        }
        
        public void SetMarginLeft(UIForia.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state);
        }

        public UIForia.UIMeasurement GetMarginLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement;
        }
        
        public void SetBorderTop(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state);
        }

        public UIForia.UIFixedLength GetBorderTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength;
        }
        
        public void SetBorderRight(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength;
        }
        
        public void SetBorderBottom(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state);
        }

        public UIForia.UIFixedLength GetBorderBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength;
        }
        
        public void SetBorderLeft(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopLeft(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusTopLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopRight(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusTopRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomRight(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusBottomRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomLeft(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusBottomLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength;
        }
        
        public void SetPaddingTop(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state);
        }

        public UIForia.UIFixedLength GetPaddingTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength;
        }
        
        public void SetPaddingRight(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state);
        }

        public UIForia.UIFixedLength GetPaddingRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength;
        }
        
        public void SetPaddingBottom(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state);
        }

        public UIForia.UIFixedLength GetPaddingBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength;
        }
        
        public void SetPaddingLeft(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state);
        }

        public UIForia.UIFixedLength GetPaddingLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength;
        }
        
        public void SetTextColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state);
        }

        public UnityEngine.Color GetTextColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor;
        }
        
        public void SetTextFontAsset(TMPro.TMP_FontAsset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, 0, 0, value), state);
        }

        public TMPro.TMP_FontAsset GetTextFontAsset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont;
        }
        
        public void SetTextFontSize(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state);
        }

        public int GetTextFontSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsInt;
        }
        
        public void SetTextFontStyle(UIForia.Text.FontStyle value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state);
        }

        public UIForia.Text.FontStyle GetTextFontStyle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle;
        }
        
        public void SetTextAlignment(UIForia.Text.TextAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state);
        }

        public UIForia.Text.TextAlignment GetTextAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment;
        }
        
        public void SetTextTransform(UIForia.Text.TextTransform value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state);
        }

        public UIForia.Text.TextTransform GetTextTransform(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform;
        }
        
        public void SetAnchorTop(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value), state);
        }

        public UIForia.UIFixedLength GetAnchorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTop, state).AsUIFixedLength;
        }
        
        public void SetAnchorRight(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value), state);
        }

        public UIForia.UIFixedLength GetAnchorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorRight, state).AsUIFixedLength;
        }
        
        public void SetAnchorBottom(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value), state);
        }

        public UIForia.UIFixedLength GetAnchorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorBottom, state).AsUIFixedLength;
        }
        
        public void SetAnchorLeft(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value), state);
        }

        public UIForia.UIFixedLength GetAnchorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorLeft, state).AsUIFixedLength;
        }
        
        public void SetAnchorTarget(UIForia.Rendering.AnchorTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value), state);
        }

        public UIForia.Rendering.AnchorTarget GetAnchorTarget(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTarget, state).AsAnchorTarget;
        }
        
        public void SetTransformPositionX(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state);
        }

        public UIForia.UIFixedLength GetTransformPositionX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsUIFixedLength;
        }
        
        public void SetTransformPositionY(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state);
        }

        public UIForia.UIFixedLength GetTransformPositionY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsUIFixedLength;
        }
        
        public void SetTransformPivotX(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state);
        }

        public UIForia.UIFixedLength GetTransformPivotX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength;
        }
        
        public void SetTransformPivotY(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state);
        }

        public UIForia.UIFixedLength GetTransformPivotY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotY, state).AsUIFixedLength;
        }
        
        public void SetTransformScaleX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value), state);
        }

        public float GetTransformScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat;
        }
        
        public void SetTransformScaleY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value), state);
        }

        public float GetTransformScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat;
        }
        
        public void SetTransformRotation(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value), state);
        }

        public float GetTransformRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformRotation, state).AsFloat;
        }
        
        public void SetTransformBehaviorX(UIForia.Rendering.TransformBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value), state);
        }

        public UIForia.Rendering.TransformBehavior GetTransformBehaviorX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsTransformBehavior;
        }
        
        public void SetTransformBehaviorY(UIForia.Rendering.TransformBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value), state);
        }

        public UIForia.Rendering.TransformBehavior GetTransformBehaviorY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformBehaviorY, state).AsTransformBehavior;
        }
        
        public void SetLayoutType(UIForia.Rendering.LayoutType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state);
        }

        public UIForia.Rendering.LayoutType GetLayoutType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType;
        }
        
        public void SetLayoutBehavior(UIForia.Layout.LayoutBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state);
        }

        public UIForia.Layout.LayoutBehavior GetLayoutBehavior(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutBehavior, state).AsLayoutBehavior;
        }
        
        public void SetZIndex(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ZIndex, value), state);
        }

        public int GetZIndex(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt;
        }
        
        public void SetRenderLayerOffset(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value), state);
        }

        public int GetRenderLayerOffset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt;
        }
        
        public void SetRenderLayer(UIForia.Rendering.RenderLayer value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state);
        }

        public UIForia.Rendering.RenderLayer GetRenderLayer(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer;
        }
        
        public void SetScrollbarVerticalAttachment(UIForia.Rendering.VerticalScrollbarAttachment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalAttachment, (int)value), state);
        }

        public UIForia.Rendering.VerticalScrollbarAttachment GetScrollbarVerticalAttachment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalAttachment, state).AsVerticalScrollbarAttachment;
        }
        
        public void SetScrollbarVerticalButtonPlacement(UIForia.Rendering.ScrollbarButtonPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalButtonPlacement, (int)value), state);
        }

        public UIForia.Rendering.ScrollbarButtonPlacement GetScrollbarVerticalButtonPlacement(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalButtonPlacement, state).AsScrollbarButtonPlacement;
        }
        
        public void SetScrollbarVerticalTrackSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackSize, value), state);
        }

        public float GetScrollbarVerticalTrackSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalTrackBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius, value), state);
        }

        public float GetScrollbarVerticalTrackBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarVerticalTrackBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize, value), state);
        }

        public float GetScrollbarVerticalTrackBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalTrackBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalTrackBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackBorderColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalTrackImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarVerticalTrackImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackImage, state).AsTexture2D;
        }
        
        public void SetScrollbarVerticalTrackColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalTrackColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalTrackColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalTrackColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalHandleSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleSize, value), state);
        }

        public float GetScrollbarVerticalHandleSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalHandleBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius, value), state);
        }

        public float GetScrollbarVerticalHandleBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarVerticalHandleBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize, value), state);
        }

        public float GetScrollbarVerticalHandleBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalHandleBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalHandleBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleBorderColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalHandleImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarVerticalHandleImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleImage, state).AsTexture2D;
        }
        
        public void SetScrollbarVerticalHandleColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalHandleColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalHandleColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalHandleColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalIncrementSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementSize, value), state);
        }

        public float GetScrollbarVerticalIncrementSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalIncrementBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, value), state);
        }

        public float GetScrollbarVerticalIncrementBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarVerticalIncrementBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize, value), state);
        }

        public float GetScrollbarVerticalIncrementBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalIncrementBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalIncrementBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementBorderColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalIncrementImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarVerticalIncrementImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementImage, state).AsTexture2D;
        }
        
        public void SetScrollbarVerticalIncrementColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalIncrementColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalIncrementColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalDecrementSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementSize, value), state);
        }

        public float GetScrollbarVerticalDecrementSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalDecrementBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, value), state);
        }

        public float GetScrollbarVerticalDecrementBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarVerticalDecrementBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize, value), state);
        }

        public float GetScrollbarVerticalDecrementBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarVerticalDecrementBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalDecrementBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementBorderColor, state).AsColor;
        }
        
        public void SetScrollbarVerticalDecrementImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarVerticalDecrementImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementImage, state).AsTexture2D;
        }
        
        public void SetScrollbarVerticalDecrementColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementColor, value), state);
        }

        public UnityEngine.Color GetScrollbarVerticalDecrementColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarVerticalDecrementColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalAttachment(UIForia.Rendering.HorizontalScrollbarAttachment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalAttachment, (int)value), state);
        }

        public UIForia.Rendering.HorizontalScrollbarAttachment GetScrollbarHorizontalAttachment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalAttachment, state).AsHorizontalScrollbarAttachment;
        }
        
        public void SetScrollbarHorizontalButtonPlacement(UIForia.Rendering.ScrollbarButtonPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement, (int)value), state);
        }

        public UIForia.Rendering.ScrollbarButtonPlacement GetScrollbarHorizontalButtonPlacement(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalButtonPlacement, state).AsScrollbarButtonPlacement;
        }
        
        public void SetScrollbarHorizontalTrackSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackSize, value), state);
        }

        public float GetScrollbarHorizontalTrackSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalTrackBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, value), state);
        }

        public float GetScrollbarHorizontalTrackBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalTrackBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize, value), state);
        }

        public float GetScrollbarHorizontalTrackBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalTrackBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalTrackBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackBorderColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalTrackImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarHorizontalTrackImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackImage, state).AsTexture2D;
        }
        
        public void SetScrollbarHorizontalTrackColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalTrackColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalTrackColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalHandleSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleSize, value), state);
        }

        public float GetScrollbarHorizontalHandleSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalHandleBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, value), state);
        }

        public float GetScrollbarHorizontalHandleBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalHandleBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize, value), state);
        }

        public float GetScrollbarHorizontalHandleBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalHandleBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalHandleBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleBorderColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalHandleImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarHorizontalHandleImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleImage, state).AsTexture2D;
        }
        
        public void SetScrollbarHorizontalHandleColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalHandleColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalHandleColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalIncrementSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementSize, value), state);
        }

        public float GetScrollbarHorizontalIncrementSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalIncrementBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, value), state);
        }

        public float GetScrollbarHorizontalIncrementBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalIncrementBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, value), state);
        }

        public float GetScrollbarHorizontalIncrementBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalIncrementBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalIncrementBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalIncrementImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarHorizontalIncrementImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementImage, state).AsTexture2D;
        }
        
        public void SetScrollbarHorizontalIncrementColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalIncrementColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalIncrementColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalDecrementSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementSize, value), state);
        }

        public float GetScrollbarHorizontalDecrementSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalDecrementBorderRadius(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, value), state);
        }

        public float GetScrollbarHorizontalDecrementBorderRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalDecrementBorderSize(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, value), state);
        }

        public float GetScrollbarHorizontalDecrementBorderSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, state).AsFloat;
        }
        
        public void SetScrollbarHorizontalDecrementBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalDecrementBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, state).AsColor;
        }
        
        public void SetScrollbarHorizontalDecrementImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementImage, 0, 0, value), state);
        }

        public UnityEngine.Texture2D GetScrollbarHorizontalDecrementImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementImage, state).AsTexture2D;
        }
        
        public void SetScrollbarHorizontalDecrementColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementColor, value), state);
        }

        public UnityEngine.Color GetScrollbarHorizontalDecrementColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarHorizontalDecrementColor, state).AsColor;
        }
        

        public StyleProperty GetComputedStyleProperty(StylePropertyId propertyId) {
        			switch(propertyId) {
				case StylePropertyId.OverflowX:
					 return new StyleProperty(StylePropertyId.OverflowX, (int)OverflowX);
				case StylePropertyId.OverflowY:
					 return new StyleProperty(StylePropertyId.OverflowY, (int)OverflowY);
				case StylePropertyId.BorderColor:
					 return new StyleProperty(StylePropertyId.BorderColor, BorderColor);
				case StylePropertyId.BackgroundColor:
					 return new StyleProperty(StylePropertyId.BackgroundColor, BackgroundColor);
				case StylePropertyId.BackgroundColorSecondary:
					 return new StyleProperty(StylePropertyId.BackgroundColorSecondary, BackgroundColorSecondary);
				case StylePropertyId.BackgroundImage:
					 return new StyleProperty(StylePropertyId.BackgroundImage, 0, 0, BackgroundImage);
				case StylePropertyId.BackgroundImage1:
					 return new StyleProperty(StylePropertyId.BackgroundImage1, 0, 0, BackgroundImage1);
				case StylePropertyId.BackgroundImage2:
					 return new StyleProperty(StylePropertyId.BackgroundImage2, 0, 0, BackgroundImage2);
				case StylePropertyId.BackgroundGradientType:
					 return new StyleProperty(StylePropertyId.BackgroundGradientType, (int)BackgroundGradientType);
				case StylePropertyId.BackgroundGradientAxis:
					 return new StyleProperty(StylePropertyId.BackgroundGradientAxis, (int)BackgroundGradientAxis);
				case StylePropertyId.BackgroundGradientStart:
					 return new StyleProperty(StylePropertyId.BackgroundGradientStart, BackgroundGradientStart);
				case StylePropertyId.BackgroundFillRotation:
					 return new StyleProperty(StylePropertyId.BackgroundFillRotation, BackgroundFillRotation);
				case StylePropertyId.BackgroundFillType:
					 return new StyleProperty(StylePropertyId.BackgroundFillType, (int)BackgroundFillType);
				case StylePropertyId.BackgroundShapeType:
					 return new StyleProperty(StylePropertyId.BackgroundShapeType, (int)BackgroundShapeType);
				case StylePropertyId.BackgroundFillOffsetX:
					 return new StyleProperty(StylePropertyId.BackgroundFillOffsetX, BackgroundFillOffsetX);
				case StylePropertyId.BackgroundFillOffsetY:
					 return new StyleProperty(StylePropertyId.BackgroundFillOffsetY, BackgroundFillOffsetY);
				case StylePropertyId.BackgroundFillScaleX:
					 return new StyleProperty(StylePropertyId.BackgroundFillScaleX, BackgroundFillScaleX);
				case StylePropertyId.BackgroundFillScaleY:
					 return new StyleProperty(StylePropertyId.BackgroundFillScaleY, BackgroundFillScaleY);
				case StylePropertyId.Opacity:
					 return new StyleProperty(StylePropertyId.Opacity, Opacity);
				case StylePropertyId.Cursor:
					 return new StyleProperty(StylePropertyId.Cursor, 0, 0, Cursor);
				case StylePropertyId.Visibility:
					 return new StyleProperty(StylePropertyId.Visibility, (int)Visibility);
				case StylePropertyId.FlexItemOrder:
					 return new StyleProperty(StylePropertyId.FlexItemOrder, FlexItemOrder);
				case StylePropertyId.FlexItemGrow:
					 return new StyleProperty(StylePropertyId.FlexItemGrow, FlexItemGrow);
				case StylePropertyId.FlexItemShrink:
					 return new StyleProperty(StylePropertyId.FlexItemShrink, FlexItemShrink);
				case StylePropertyId.FlexItemSelfAlignment:
					 return new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int)FlexItemSelfAlignment);
				case StylePropertyId.FlexLayoutDirection:
					 return new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)FlexLayoutDirection);
				case StylePropertyId.FlexLayoutWrap:
					 return new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)FlexLayoutWrap);
				case StylePropertyId.FlexLayoutMainAxisAlignment:
					 return new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)FlexLayoutMainAxisAlignment);
				case StylePropertyId.FlexLayoutCrossAxisAlignment:
					 return new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)FlexLayoutCrossAxisAlignment);
				case StylePropertyId.GridItemColStart:
					 return new StyleProperty(StylePropertyId.GridItemColStart, GridItemColStart);
				case StylePropertyId.GridItemColSpan:
					 return new StyleProperty(StylePropertyId.GridItemColSpan, GridItemColSpan);
				case StylePropertyId.GridItemRowStart:
					 return new StyleProperty(StylePropertyId.GridItemRowStart, GridItemRowStart);
				case StylePropertyId.GridItemRowSpan:
					 return new StyleProperty(StylePropertyId.GridItemRowSpan, GridItemRowSpan);
				case StylePropertyId.GridItemColSelfAlignment:
					 return new StyleProperty(StylePropertyId.GridItemColSelfAlignment, (int)GridItemColSelfAlignment);
				case StylePropertyId.GridItemRowSelfAlignment:
					 return new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, (int)GridItemRowSelfAlignment);
				case StylePropertyId.GridLayoutDirection:
					 return new StyleProperty(StylePropertyId.GridLayoutDirection, (int)GridLayoutDirection);
				case StylePropertyId.GridLayoutDensity:
					 return new StyleProperty(StylePropertyId.GridLayoutDensity, (int)GridLayoutDensity);
				case StylePropertyId.GridLayoutColTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, GridLayoutColTemplate);
				case StylePropertyId.GridLayoutRowTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, GridLayoutRowTemplate);
				case StylePropertyId.GridLayoutColAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutColAutoSize, GridLayoutColAutoSize);
				case StylePropertyId.GridLayoutRowAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, GridLayoutRowAutoSize);
				case StylePropertyId.GridLayoutColGap:
					 return new StyleProperty(StylePropertyId.GridLayoutColGap, GridLayoutColGap);
				case StylePropertyId.GridLayoutRowGap:
					 return new StyleProperty(StylePropertyId.GridLayoutRowGap, GridLayoutRowGap);
				case StylePropertyId.GridLayoutColAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)GridLayoutColAlignment);
				case StylePropertyId.GridLayoutRowAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)GridLayoutRowAlignment);
				case StylePropertyId.MinWidth:
					 return new StyleProperty(StylePropertyId.MinWidth, MinWidth);
				case StylePropertyId.MaxWidth:
					 return new StyleProperty(StylePropertyId.MaxWidth, MaxWidth);
				case StylePropertyId.PreferredWidth:
					 return new StyleProperty(StylePropertyId.PreferredWidth, PreferredWidth);
				case StylePropertyId.MinHeight:
					 return new StyleProperty(StylePropertyId.MinHeight, MinHeight);
				case StylePropertyId.MaxHeight:
					 return new StyleProperty(StylePropertyId.MaxHeight, MaxHeight);
				case StylePropertyId.PreferredHeight:
					 return new StyleProperty(StylePropertyId.PreferredHeight, PreferredHeight);
				case StylePropertyId.MarginTop:
					 return new StyleProperty(StylePropertyId.MarginTop, MarginTop);
				case StylePropertyId.MarginRight:
					 return new StyleProperty(StylePropertyId.MarginRight, MarginRight);
				case StylePropertyId.MarginBottom:
					 return new StyleProperty(StylePropertyId.MarginBottom, MarginBottom);
				case StylePropertyId.MarginLeft:
					 return new StyleProperty(StylePropertyId.MarginLeft, MarginLeft);
				case StylePropertyId.BorderTop:
					 return new StyleProperty(StylePropertyId.BorderTop, BorderTop);
				case StylePropertyId.BorderRight:
					 return new StyleProperty(StylePropertyId.BorderRight, BorderRight);
				case StylePropertyId.BorderBottom:
					 return new StyleProperty(StylePropertyId.BorderBottom, BorderBottom);
				case StylePropertyId.BorderLeft:
					 return new StyleProperty(StylePropertyId.BorderLeft, BorderLeft);
				case StylePropertyId.BorderRadiusTopLeft:
					 return new StyleProperty(StylePropertyId.BorderRadiusTopLeft, BorderRadiusTopLeft);
				case StylePropertyId.BorderRadiusTopRight:
					 return new StyleProperty(StylePropertyId.BorderRadiusTopRight, BorderRadiusTopRight);
				case StylePropertyId.BorderRadiusBottomRight:
					 return new StyleProperty(StylePropertyId.BorderRadiusBottomRight, BorderRadiusBottomRight);
				case StylePropertyId.BorderRadiusBottomLeft:
					 return new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, BorderRadiusBottomLeft);
				case StylePropertyId.PaddingTop:
					 return new StyleProperty(StylePropertyId.PaddingTop, PaddingTop);
				case StylePropertyId.PaddingRight:
					 return new StyleProperty(StylePropertyId.PaddingRight, PaddingRight);
				case StylePropertyId.PaddingBottom:
					 return new StyleProperty(StylePropertyId.PaddingBottom, PaddingBottom);
				case StylePropertyId.PaddingLeft:
					 return new StyleProperty(StylePropertyId.PaddingLeft, PaddingLeft);
				case StylePropertyId.TextColor:
					 return new StyleProperty(StylePropertyId.TextColor, TextColor);
				case StylePropertyId.TextFontAsset:
					 return new StyleProperty(StylePropertyId.TextFontAsset, 0, 0, TextFontAsset);
				case StylePropertyId.TextFontSize:
					 return new StyleProperty(StylePropertyId.TextFontSize, TextFontSize);
				case StylePropertyId.TextFontStyle:
					 return new StyleProperty(StylePropertyId.TextFontStyle, (int)TextFontStyle);
				case StylePropertyId.TextAlignment:
					 return new StyleProperty(StylePropertyId.TextAlignment, (int)TextAlignment);
				case StylePropertyId.TextTransform:
					 return new StyleProperty(StylePropertyId.TextTransform, (int)TextTransform);
				case StylePropertyId.AnchorTop:
					 return new StyleProperty(StylePropertyId.AnchorTop, AnchorTop);
				case StylePropertyId.AnchorRight:
					 return new StyleProperty(StylePropertyId.AnchorRight, AnchorRight);
				case StylePropertyId.AnchorBottom:
					 return new StyleProperty(StylePropertyId.AnchorBottom, AnchorBottom);
				case StylePropertyId.AnchorLeft:
					 return new StyleProperty(StylePropertyId.AnchorLeft, AnchorLeft);
				case StylePropertyId.AnchorTarget:
					 return new StyleProperty(StylePropertyId.AnchorTarget, (int)AnchorTarget);
				case StylePropertyId.TransformPositionX:
					 return new StyleProperty(StylePropertyId.TransformPositionX, TransformPositionX);
				case StylePropertyId.TransformPositionY:
					 return new StyleProperty(StylePropertyId.TransformPositionY, TransformPositionY);
				case StylePropertyId.TransformPivotX:
					 return new StyleProperty(StylePropertyId.TransformPivotX, TransformPivotX);
				case StylePropertyId.TransformPivotY:
					 return new StyleProperty(StylePropertyId.TransformPivotY, TransformPivotY);
				case StylePropertyId.TransformScaleX:
					 return new StyleProperty(StylePropertyId.TransformScaleX, TransformScaleX);
				case StylePropertyId.TransformScaleY:
					 return new StyleProperty(StylePropertyId.TransformScaleY, TransformScaleY);
				case StylePropertyId.TransformRotation:
					 return new StyleProperty(StylePropertyId.TransformRotation, TransformRotation);
				case StylePropertyId.TransformBehaviorX:
					 return new StyleProperty(StylePropertyId.TransformBehaviorX, (int)TransformBehaviorX);
				case StylePropertyId.TransformBehaviorY:
					 return new StyleProperty(StylePropertyId.TransformBehaviorY, (int)TransformBehaviorY);
				case StylePropertyId.LayoutType:
					 return new StyleProperty(StylePropertyId.LayoutType, (int)LayoutType);
				case StylePropertyId.LayoutBehavior:
					 return new StyleProperty(StylePropertyId.LayoutBehavior, (int)LayoutBehavior);
				case StylePropertyId.ZIndex:
					 return new StyleProperty(StylePropertyId.ZIndex, ZIndex);
				case StylePropertyId.RenderLayerOffset:
					 return new StyleProperty(StylePropertyId.RenderLayerOffset, RenderLayerOffset);
				case StylePropertyId.RenderLayer:
					 return new StyleProperty(StylePropertyId.RenderLayer, (int)RenderLayer);
				case StylePropertyId.ScrollbarVerticalAttachment:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalAttachment, (int)ScrollbarVerticalAttachment);
				case StylePropertyId.ScrollbarVerticalButtonPlacement:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalButtonPlacement, (int)ScrollbarVerticalButtonPlacement);
				case StylePropertyId.ScrollbarVerticalTrackSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackSize, ScrollbarVerticalTrackSize);
				case StylePropertyId.ScrollbarVerticalTrackBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius, ScrollbarVerticalTrackBorderRadius);
				case StylePropertyId.ScrollbarVerticalTrackBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize, ScrollbarVerticalTrackBorderSize);
				case StylePropertyId.ScrollbarVerticalTrackBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor, ScrollbarVerticalTrackBorderColor);
				case StylePropertyId.ScrollbarVerticalTrackImage:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackImage, 0, 0, ScrollbarVerticalTrackImage);
				case StylePropertyId.ScrollbarVerticalTrackColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalTrackColor, ScrollbarVerticalTrackColor);
				case StylePropertyId.ScrollbarVerticalHandleSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleSize, ScrollbarVerticalHandleSize);
				case StylePropertyId.ScrollbarVerticalHandleBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius, ScrollbarVerticalHandleBorderRadius);
				case StylePropertyId.ScrollbarVerticalHandleBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize, ScrollbarVerticalHandleBorderSize);
				case StylePropertyId.ScrollbarVerticalHandleBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor, ScrollbarVerticalHandleBorderColor);
				case StylePropertyId.ScrollbarVerticalHandleImage:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleImage, 0, 0, ScrollbarVerticalHandleImage);
				case StylePropertyId.ScrollbarVerticalHandleColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalHandleColor, ScrollbarVerticalHandleColor);
				case StylePropertyId.ScrollbarVerticalIncrementSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementSize, ScrollbarVerticalIncrementSize);
				case StylePropertyId.ScrollbarVerticalIncrementBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, ScrollbarVerticalIncrementBorderRadius);
				case StylePropertyId.ScrollbarVerticalIncrementBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize, ScrollbarVerticalIncrementBorderSize);
				case StylePropertyId.ScrollbarVerticalIncrementBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor, ScrollbarVerticalIncrementBorderColor);
				case StylePropertyId.ScrollbarVerticalIncrementImage:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementImage, 0, 0, ScrollbarVerticalIncrementImage);
				case StylePropertyId.ScrollbarVerticalIncrementColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalIncrementColor, ScrollbarVerticalIncrementColor);
				case StylePropertyId.ScrollbarVerticalDecrementSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementSize, ScrollbarVerticalDecrementSize);
				case StylePropertyId.ScrollbarVerticalDecrementBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, ScrollbarVerticalDecrementBorderRadius);
				case StylePropertyId.ScrollbarVerticalDecrementBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize, ScrollbarVerticalDecrementBorderSize);
				case StylePropertyId.ScrollbarVerticalDecrementBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor, ScrollbarVerticalDecrementBorderColor);
				case StylePropertyId.ScrollbarVerticalDecrementImage:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementImage, 0, 0, ScrollbarVerticalDecrementImage);
				case StylePropertyId.ScrollbarVerticalDecrementColor:
					 return new StyleProperty(StylePropertyId.ScrollbarVerticalDecrementColor, ScrollbarVerticalDecrementColor);
				case StylePropertyId.ScrollbarHorizontalAttachment:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalAttachment, (int)ScrollbarHorizontalAttachment);
				case StylePropertyId.ScrollbarHorizontalButtonPlacement:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement, (int)ScrollbarHorizontalButtonPlacement);
				case StylePropertyId.ScrollbarHorizontalTrackSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackSize, ScrollbarHorizontalTrackSize);
				case StylePropertyId.ScrollbarHorizontalTrackBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, ScrollbarHorizontalTrackBorderRadius);
				case StylePropertyId.ScrollbarHorizontalTrackBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize, ScrollbarHorizontalTrackBorderSize);
				case StylePropertyId.ScrollbarHorizontalTrackBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor, ScrollbarHorizontalTrackBorderColor);
				case StylePropertyId.ScrollbarHorizontalTrackImage:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackImage, 0, 0, ScrollbarHorizontalTrackImage);
				case StylePropertyId.ScrollbarHorizontalTrackColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalTrackColor, ScrollbarHorizontalTrackColor);
				case StylePropertyId.ScrollbarHorizontalHandleSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleSize, ScrollbarHorizontalHandleSize);
				case StylePropertyId.ScrollbarHorizontalHandleBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, ScrollbarHorizontalHandleBorderRadius);
				case StylePropertyId.ScrollbarHorizontalHandleBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize, ScrollbarHorizontalHandleBorderSize);
				case StylePropertyId.ScrollbarHorizontalHandleBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor, ScrollbarHorizontalHandleBorderColor);
				case StylePropertyId.ScrollbarHorizontalHandleImage:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleImage, 0, 0, ScrollbarHorizontalHandleImage);
				case StylePropertyId.ScrollbarHorizontalHandleColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalHandleColor, ScrollbarHorizontalHandleColor);
				case StylePropertyId.ScrollbarHorizontalIncrementSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementSize, ScrollbarHorizontalIncrementSize);
				case StylePropertyId.ScrollbarHorizontalIncrementBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, ScrollbarHorizontalIncrementBorderRadius);
				case StylePropertyId.ScrollbarHorizontalIncrementBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, ScrollbarHorizontalIncrementBorderSize);
				case StylePropertyId.ScrollbarHorizontalIncrementBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, ScrollbarHorizontalIncrementBorderColor);
				case StylePropertyId.ScrollbarHorizontalIncrementImage:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementImage, 0, 0, ScrollbarHorizontalIncrementImage);
				case StylePropertyId.ScrollbarHorizontalIncrementColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalIncrementColor, ScrollbarHorizontalIncrementColor);
				case StylePropertyId.ScrollbarHorizontalDecrementSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementSize, ScrollbarHorizontalDecrementSize);
				case StylePropertyId.ScrollbarHorizontalDecrementBorderRadius:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, ScrollbarHorizontalDecrementBorderRadius);
				case StylePropertyId.ScrollbarHorizontalDecrementBorderSize:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, ScrollbarHorizontalDecrementBorderSize);
				case StylePropertyId.ScrollbarHorizontalDecrementBorderColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, ScrollbarHorizontalDecrementBorderColor);
				case StylePropertyId.ScrollbarHorizontalDecrementImage:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementImage, 0, 0, ScrollbarHorizontalDecrementImage);
				case StylePropertyId.ScrollbarHorizontalDecrementColor:
					 return new StyleProperty(StylePropertyId.ScrollbarHorizontalDecrementColor, ScrollbarHorizontalDecrementColor);
				default: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
				}  
        }

    }

    public static partial class StyleUtil {
        
      public static bool CanAnimate(StylePropertyId propertyId) {
                switch (propertyId) {
    
                    case StylePropertyId.BorderColor: return true;
                    case StylePropertyId.BackgroundColor: return true;
                    case StylePropertyId.BackgroundColorSecondary: return true;
                    case StylePropertyId.BackgroundGradientStart: return true;
                    case StylePropertyId.BackgroundFillRotation: return true;
                    case StylePropertyId.BackgroundFillOffsetX: return true;
                    case StylePropertyId.BackgroundFillOffsetY: return true;
                    case StylePropertyId.BackgroundFillScaleX: return true;
                    case StylePropertyId.BackgroundFillScaleY: return true;
                    case StylePropertyId.Opacity: return true;
                    case StylePropertyId.FlexItemOrder: return true;
                    case StylePropertyId.FlexItemGrow: return true;
                    case StylePropertyId.FlexItemShrink: return true;
                    case StylePropertyId.GridLayoutColGap: return true;
                    case StylePropertyId.GridLayoutRowGap: return true;
                    case StylePropertyId.MinWidth: return true;
                    case StylePropertyId.MaxWidth: return true;
                    case StylePropertyId.PreferredWidth: return true;
                    case StylePropertyId.MinHeight: return true;
                    case StylePropertyId.MaxHeight: return true;
                    case StylePropertyId.PreferredHeight: return true;
                    case StylePropertyId.MarginTop: return true;
                    case StylePropertyId.MarginRight: return true;
                    case StylePropertyId.MarginBottom: return true;
                    case StylePropertyId.MarginLeft: return true;
                    case StylePropertyId.BorderTop: return true;
                    case StylePropertyId.BorderRight: return true;
                    case StylePropertyId.BorderBottom: return true;
                    case StylePropertyId.BorderLeft: return true;
                    case StylePropertyId.BorderRadiusTopLeft: return true;
                    case StylePropertyId.BorderRadiusTopRight: return true;
                    case StylePropertyId.BorderRadiusBottomRight: return true;
                    case StylePropertyId.BorderRadiusBottomLeft: return true;
                    case StylePropertyId.PaddingTop: return true;
                    case StylePropertyId.PaddingRight: return true;
                    case StylePropertyId.PaddingBottom: return true;
                    case StylePropertyId.PaddingLeft: return true;
                    case StylePropertyId.TextColor: return true;
                    case StylePropertyId.TextFontSize: return true;
                    case StylePropertyId.AnchorTop: return true;
                    case StylePropertyId.AnchorRight: return true;
                    case StylePropertyId.AnchorBottom: return true;
                    case StylePropertyId.AnchorLeft: return true;
                    case StylePropertyId.TransformPositionX: return true;
                    case StylePropertyId.TransformPositionY: return true;
                    case StylePropertyId.TransformPivotX: return true;
                    case StylePropertyId.TransformPivotY: return true;
                    case StylePropertyId.TransformScaleX: return true;
                    case StylePropertyId.TransformScaleY: return true;
                    case StylePropertyId.TransformRotation: return true;
                    case StylePropertyId.ZIndex: return true;
                    case StylePropertyId.RenderLayerOffset: return true;
                    case StylePropertyId.RenderLayer: return true;

                }
    
                return false;
            }

        public static bool IsInherited(StylePropertyId propertyId) {
            switch (propertyId) {

                    case StylePropertyId.Cursor: return true;
                    case StylePropertyId.TextColor: return true;
                    case StylePropertyId.TextFontAsset: return true;
                    case StylePropertyId.TextFontSize: return true;
                    case StylePropertyId.TextFontStyle: return true;
                    case StylePropertyId.TextAlignment: return true;
                    case StylePropertyId.TextTransform: return true;

            }

            return false;
        }

    }

}