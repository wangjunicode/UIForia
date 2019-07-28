
using UIForia.Util;

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
        
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state); }
        }
        
        public UnityEngine.Color BackgroundTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state); }
        }
        
        public UnityEngine.Color BorderColorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state); }
        }
        
        public UnityEngine.Color BorderColorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state); }
        }
        
        public UnityEngine.Color BorderColorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state); }
        }
        
        public UnityEngine.Color BorderColorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageTileX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageTileY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state); }
        }
        
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state); }
        }
        
        public string Painter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Painter, state).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Painter, value), state); }
        }
        
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state); }
        }
        
        public UIForia.Rendering.CursorStyle Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state); }
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
        
        public UIForia.Layout.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutWrap FlexLayoutWrap {
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
        
        public UIForia.Layout.LayoutDirection GridLayoutDirection {
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
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutMainAxisAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutMainAxisAutoSize, state).AsGridTrackSize; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutMainAxisAutoSize, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutCrossAxisAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutCrossAxisAutoSize, state).AsGridTrackSize; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutCrossAxisAutoSize, value), state); }
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
        
        public float RadialLayoutStartAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutStartAngle, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value), state); }
        }
        
        public float RadialLayoutEndAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutEndAngle, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value), state); }
        }
        
        public UIForia.UIFixedLength RadialLayoutRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state); }
        }
        
        public UIForia.Layout.AlignmentTarget AlignmentTargetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentTarget AlignmentTargetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentBehavior AlignmentBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBehaviorX, state).AsAlignmentBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentBehavior AlignmentBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBehaviorY, state).AsAlignmentBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value), state); }
        }
        
        public float AlignmentPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentPivotX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotX, value), state); }
        }
        
        public float AlignmentPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentPivotY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotY, value), state); }
        }
        
        public float AlignmentOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state); }
        }
        
        public float AlignmentOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state); }
        }
        
        public UIForia.Layout.Fit FitX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitX, state).AsFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value), state); }
        }
        
        public UIForia.Layout.Fit FitY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitY, state).AsFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state); }
        }
        
        public UnityEngine.Color BorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state); }
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
        
        public UIForia.FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state); }
        }
        
        public UIForia.UIFixedLength TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength; }
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
        
        public float TextOutlineWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state); }
        }
        
        public UnityEngine.Color TextOutlineColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state); }
        }
        
        public float TextOutlineSoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state); }
        }
        
        public UnityEngine.Color TextGlowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state); }
        }
        
        public float TextGlowOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOffset, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value), state); }
        }
        
        public float TextGlowInner {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowInner, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value), state); }
        }
        
        public float TextGlowOuter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOuter, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value), state); }
        }
        
        public float TextGlowPower {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowPower, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value), state); }
        }
        
        public UnityEngine.Color TextUnderlayColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state); }
        }
        
        public float TextUnderlayX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value), state); }
        }
        
        public float TextUnderlayY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value), state); }
        }
        
        public float TextUnderlayDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayDilate, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value), state); }
        }
        
        public float TextUnderlaySoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlaySoftness, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value), state); }
        }
        
        public float TextFaceDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFaceDilate, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value), state); }
        }
        
        public UIForia.Rendering.UnderlayType TextUnderlayType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state); }
        }
        
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state); }
        }
        
        public UIForia.Text.WhitespaceMode TextWhitespaceMode {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state); }
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
        
        public UIForia.TransformOffset TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsTransformOffset; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state); }
        }
        
        public UIForia.TransformOffset TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsTransformOffset; }
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
        
        public UIForia.Layout.LayoutType LayoutType {
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
        
        public int Layer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Layer, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Layer, value), state); }
        }
        
        public string Scrollbar {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Scrollbar, state).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Scrollbar, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement ScrollbarSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarSize, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value), state); }
        }
        
        public UnityEngine.Color ScrollbarColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value), state); }
        }
        
        public UIForia.Rendering.UnderlayType ShadowType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowType, state).AsUnderlayType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value), state); }
        }
        
        public float ShadowOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value), state); }
        }
        
        public float ShadowOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value), state); }
        }
        
        public float ShadowSoftnessX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSoftnessX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessX, value), state); }
        }
        
        public float ShadowSoftnessY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSoftnessY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessY, value), state); }
        }
        
        public float ShadowIntensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowIntensity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value), state); }
        }
        
    }

    public partial struct StyleProperty {

        public bool IsUnset {
            get { 
                switch(propertyId) {
                                        case StylePropertyId.OverflowX: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.OverflowY: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.BackgroundColor: return int1 == 0;
                    case StylePropertyId.BackgroundTint: return int1 == 0;
                    case StylePropertyId.BorderColorTop: return int1 == 0;
                    case StylePropertyId.BorderColorRight: return int1 == 0;
                    case StylePropertyId.BorderColorBottom: return int1 == 0;
                    case StylePropertyId.BorderColorLeft: return int1 == 0;
                    case StylePropertyId.BackgroundImageOffsetX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageOffsetY: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageScaleX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageScaleY: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageTileX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageTileY: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImageRotation: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BackgroundImage: return objectField == null;
                    case StylePropertyId.Painter: return objectField == null;
                    case StylePropertyId.Opacity: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.Cursor: return objectField == null;
                    case StylePropertyId.Visibility: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexItemOrder: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.FlexItemGrow: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.FlexItemShrink: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.FlexItemSelfAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutDirection: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutWrap: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutMainAxisAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutCrossAxisAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridItemColStart: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.GridItemColSpan: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.GridItemRowStart: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.GridItemRowSpan: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.GridItemColSelfAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridItemRowSelfAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutDirection: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutDensity: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutColTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutRowTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutMainAxisAutoSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.GridLayoutCrossAxisAutoSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.GridLayoutColGap: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.GridLayoutRowGap: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.GridLayoutColAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutRowAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.RadialLayoutStartAngle: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.RadialLayoutEndAngle: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.RadialLayoutRadius: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AlignmentTargetX: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.AlignmentTargetY: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.AlignmentBehaviorX: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.AlignmentBehaviorY: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.AlignmentPivotX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.AlignmentPivotY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.AlignmentOffsetX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.AlignmentOffsetY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.FitX: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FitY: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.MinWidth: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MaxWidth: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PreferredWidth: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MinHeight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MaxHeight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PreferredHeight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MarginTop: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MarginRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MarginBottom: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.MarginLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderColor: return int1 == 0;
                    case StylePropertyId.BorderTop: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderBottom: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderRadiusTopLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderRadiusTopRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderRadiusBottomRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.BorderRadiusBottomLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PaddingTop: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PaddingRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PaddingBottom: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.PaddingLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TextColor: return int1 == 0;
                    case StylePropertyId.TextFontAsset: return objectField == null;
                    case StylePropertyId.TextFontSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TextFontStyle: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TextAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TextOutlineWidth: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextOutlineColor: return int1 == 0;
                    case StylePropertyId.TextOutlineSoftness: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextGlowColor: return int1 == 0;
                    case StylePropertyId.TextGlowOffset: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextGlowInner: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextGlowOuter: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextGlowPower: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextUnderlayColor: return int1 == 0;
                    case StylePropertyId.TextUnderlayX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextUnderlayY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextUnderlayDilate: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextUnderlaySoftness: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextFaceDilate: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TextUnderlayType: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TextTransform: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TextWhitespaceMode: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.AnchorTop: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AnchorRight: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AnchorBottom: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AnchorLeft: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AnchorTarget: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TransformPositionX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TransformPositionY: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TransformPivotX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TransformPivotY: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.TransformScaleX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TransformScaleY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TransformRotation: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.TransformBehaviorX: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.TransformBehaviorY: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.LayoutType: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.LayoutBehavior: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.ZIndex: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.RenderLayerOffset: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.RenderLayer: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.Layer: return !IntUtil.IsDefined(int0);
                    case StylePropertyId.Scrollbar: return objectField == null;
                    case StylePropertyId.ScrollbarSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.ScrollbarColor: return int1 == 0;
                    case StylePropertyId.ShadowType: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.ShadowOffsetX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.ShadowOffsetY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.ShadowSoftnessX: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.ShadowSoftnessY: return !FloatUtil.IsDefined(float1);
                    case StylePropertyId.ShadowIntensity: return !FloatUtil.IsDefined(float1);

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
            
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value)); }
        }
            
        public UnityEngine.Color BackgroundTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundTint); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value)); }
        }
            
        public UnityEngine.Color BorderColorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value)); }
        }
            
        public UnityEngine.Color BorderColorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value)); }
        }
            
        public UnityEngine.Color BorderColorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value)); }
        }
            
        public UnityEngine.Color BorderColorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageScaleX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageScaleY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageTileX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageTileX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageTileY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageTileY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageRotation); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value)); }
        }
            
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value)); }
        }
            
        public string Painter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Painter).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Painter, value)); }
        }
            
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.Opacity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Opacity, value)); }
        }
            
        public UIForia.Rendering.CursorStyle Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Cursor).AsCursorStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Cursor, value)); }
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
            
        public UIForia.Layout.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutDirection)FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value)); }
        }
            
        public UIForia.Layout.LayoutWrap FlexLayoutWrap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutWrap)FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
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
            
        public UIForia.Layout.LayoutDirection GridLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutDirection)FindEnumProperty(StylePropertyId.GridLayoutDirection); }
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
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutRowTemplate).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutMainAxisAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutMainAxisAutoSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutMainAxisAutoSize, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutCrossAxisAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutCrossAxisAutoSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutCrossAxisAutoSize, value)); }
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
            
        public float RadialLayoutStartAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutStartAngle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value)); }
        }
            
        public float RadialLayoutEndAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutEndAngle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value)); }
        }
            
        public UIForia.UIFixedLength RadialLayoutRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.RadialLayoutRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value)); }
        }
            
        public UIForia.Layout.AlignmentTarget AlignmentTargetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentTarget AlignmentTargetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentBehavior AlignmentBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentBehavior)FindEnumProperty(StylePropertyId.AlignmentBehaviorX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentBehavior AlignmentBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentBehavior)FindEnumProperty(StylePropertyId.AlignmentBehaviorY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value)); }
        }
            
        public float AlignmentPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentPivotX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotX, value)); }
        }
            
        public float AlignmentPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentPivotY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotY, value)); }
        }
            
        public float AlignmentOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value)); }
        }
            
        public float AlignmentOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value)); }
        }
            
        public UIForia.Layout.Fit FitX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.Fit)FindEnumProperty(StylePropertyId.FitX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value)); }
        }
            
        public UIForia.Layout.Fit FitY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.Fit)FindEnumProperty(StylePropertyId.FitY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinHeight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginTop, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginRight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value)); }
        }
            
        public UnityEngine.Color BorderColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColor, value)); }
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
            
        public UIForia.FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.TextFontAsset).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value)); }
        }
            
        public UIForia.UIFixedLength TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TextFontSize); }
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
            
        public float TextOutlineWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value)); }
        }
            
        public UnityEngine.Color TextOutlineColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextOutlineColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value)); }
        }
            
        public float TextOutlineSoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineSoftness); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value)); }
        }
            
        public UnityEngine.Color TextGlowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextGlowColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value)); }
        }
            
        public float TextGlowOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOffset); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value)); }
        }
            
        public float TextGlowInner {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowInner); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value)); }
        }
            
        public float TextGlowOuter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOuter); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value)); }
        }
            
        public float TextGlowPower {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowPower); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value)); }
        }
            
        public UnityEngine.Color TextUnderlayColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextUnderlayColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value)); }
        }
            
        public float TextUnderlayX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value)); }
        }
            
        public float TextUnderlayY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value)); }
        }
            
        public float TextUnderlayDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayDilate); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value)); }
        }
            
        public float TextUnderlaySoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlaySoftness); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value)); }
        }
            
        public float TextFaceDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextFaceDilate); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value)); }
        }
            
        public UIForia.Rendering.UnderlayType TextUnderlayType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.UnderlayType)FindEnumProperty(StylePropertyId.TextUnderlayType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value)); }
        }
            
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.TextTransform)FindEnumProperty(StylePropertyId.TextTransform); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value)); }
        }
            
        public UIForia.Text.WhitespaceMode TextWhitespaceMode {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.WhitespaceMode)FindEnumProperty(StylePropertyId.TextWhitespaceMode); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value)); }
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
            
        public UIForia.TransformOffset TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindTransformOffsetProperty(StylePropertyId.TransformPositionX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value)); }
        }
            
        public UIForia.TransformOffset TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindTransformOffsetProperty(StylePropertyId.TransformPositionY); }
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
            
        public UIForia.Layout.LayoutType LayoutType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutType)FindEnumProperty(StylePropertyId.LayoutType); }
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
            
        public int Layer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.Layer); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Layer, value)); }
        }
            
        public string Scrollbar {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Scrollbar).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Scrollbar, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement ScrollbarSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.ScrollbarSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value)); }
        }
            
        public UnityEngine.Color ScrollbarColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value)); }
        }
            
        public UIForia.Rendering.UnderlayType ShadowType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.UnderlayType)FindEnumProperty(StylePropertyId.ShadowType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value)); }
        }
            
        public float ShadowOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value)); }
        }
            
        public float ShadowOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value)); }
        }
            
        public float ShadowSoftnessX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSoftnessX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessX, value)); }
        }
            
        public float ShadowSoftnessY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSoftnessY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessY, value)); }
        }
            
        public float ShadowIntensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowIntensity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value)); }
        }
            
        
    }

    public partial class UIStyleSet {
    
        

            public UIForia.Rendering.Overflow OverflowX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowX, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowX;
                }
            }

            public UIForia.Rendering.Overflow OverflowY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowY, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowY;
                }
            }

            public UnityEngine.Color BackgroundColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundColor;
                }
            }

            public UnityEngine.Color BackgroundTint { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundTint, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundTint;
                }
            }

            public UnityEngine.Color BorderColorTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorTop, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorTop;
                }
            }

            public UnityEngine.Color BorderColorRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorRight, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorRight;
                }
            }

            public UnityEngine.Color BorderColorBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorBottom, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorBottom;
                }
            }

            public UnityEngine.Color BorderColorLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorLeft, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorLeft;
                }
            }

            public UIForia.UIFixedLength BackgroundImageOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetX;
                }
            }

            public UIForia.UIFixedLength BackgroundImageOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetY;
                }
            }

            public UIForia.UIFixedLength BackgroundImageScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageScaleX;
                }
            }

            public UIForia.UIFixedLength BackgroundImageScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageScaleY;
                }
            }

            public UIForia.UIFixedLength BackgroundImageTileX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageTileX;
                }
            }

            public UIForia.UIFixedLength BackgroundImageTileY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageTileY;
                }
            }

            public UIForia.UIFixedLength BackgroundImageRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageRotation, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageRotation;
                }
            }

            public UnityEngine.Texture2D BackgroundImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage;
                }
            }

            public string Painter { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Painter, out property)) return property.AsString;
                    return DefaultStyleValues_Generated.Painter;
                }
            }

            public float Opacity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Opacity, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Opacity), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.Opacity;
                }
            }

            public UIForia.Rendering.CursorStyle Cursor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Cursor, out property)) return property.AsCursorStyle;
                    return DefaultStyleValues_Generated.Cursor;
                }
            }

            public UIForia.Rendering.Visibility Visibility { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Visibility, out property)) return property.AsVisibility;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Visibility), out property)) return property.AsVisibility;
                    return DefaultStyleValues_Generated.Visibility;
                }
            }

            public int FlexItemOrder { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemOrder, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemOrder;
                }
            }

            public int FlexItemGrow { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemGrow, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemGrow;
                }
            }

            public int FlexItemShrink { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemShrink, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemShrink;
                }
            }

            public UIForia.Layout.CrossAxisAlignment FlexItemSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemSelfAlignment, out property)) return property.AsCrossAxisAlignment;
                    return DefaultStyleValues_Generated.FlexItemSelfAlignment;
                }
            }

            public UIForia.Layout.LayoutDirection FlexLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.FlexLayoutDirection;
                }
            }

            public UIForia.Layout.LayoutWrap FlexLayoutWrap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutWrap, out property)) return property.AsLayoutWrap;
                    return DefaultStyleValues_Generated.FlexLayoutWrap;
                }
            }

            public UIForia.Layout.MainAxisAlignment FlexLayoutMainAxisAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutMainAxisAlignment, out property)) return property.AsMainAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutMainAxisAlignment;
                }
            }

            public UIForia.Layout.CrossAxisAlignment FlexLayoutCrossAxisAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutCrossAxisAlignment, out property)) return property.AsCrossAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutCrossAxisAlignment;
                }
            }

            public int GridItemColStart { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemColStart, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemColStart;
                }
            }

            public int GridItemColSpan { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemColSpan, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemColSpan;
                }
            }

            public int GridItemRowStart { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemRowStart, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemRowStart;
                }
            }

            public int GridItemRowSpan { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemRowSpan, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.GridItemRowSpan;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridItemColSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemColSelfAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridItemColSelfAlignment;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridItemRowSelfAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemRowSelfAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridItemRowSelfAlignment;
                }
            }

            public UIForia.Layout.LayoutDirection GridLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.GridLayoutDirection;
                }
            }

            public UIForia.Layout.GridLayoutDensity GridLayoutDensity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDensity, out property)) return property.AsGridLayoutDensity;
                    return DefaultStyleValues_Generated.GridLayoutDensity;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutColTemplate;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutRowTemplate;
                }
            }

            public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutMainAxisAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutMainAxisAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutMainAxisAutoSize;
                }
            }

            public UIForia.Layout.LayoutTypes.GridTrackSize GridLayoutCrossAxisAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutCrossAxisAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutCrossAxisAutoSize;
                }
            }

            public float GridLayoutColGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutColGap;
                }
            }

            public float GridLayoutRowGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutRowGap;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutColAlignment;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutRowAlignment;
                }
            }

            public float RadialLayoutStartAngle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutStartAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutStartAngle;
                }
            }

            public float RadialLayoutEndAngle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutEndAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutEndAngle;
                }
            }

            public UIForia.UIFixedLength RadialLayoutRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutRadius, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.RadialLayoutRadius;
                }
            }

            public UIForia.Layout.AlignmentTarget AlignmentTargetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetX, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetX;
                }
            }

            public UIForia.Layout.AlignmentTarget AlignmentTargetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetY, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetY;
                }
            }

            public UIForia.Layout.AlignmentBehavior AlignmentBehaviorX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBehaviorX, out property)) return property.AsAlignmentBehavior;
                    return DefaultStyleValues_Generated.AlignmentBehaviorX;
                }
            }

            public UIForia.Layout.AlignmentBehavior AlignmentBehaviorY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBehaviorY, out property)) return property.AsAlignmentBehavior;
                    return DefaultStyleValues_Generated.AlignmentBehaviorY;
                }
            }

            public float AlignmentPivotX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentPivotX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentPivotX;
                }
            }

            public float AlignmentPivotY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentPivotY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentPivotY;
                }
            }

            public float AlignmentOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentOffsetX;
                }
            }

            public float AlignmentOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentOffsetY;
                }
            }

            public UIForia.Layout.Fit FitX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitX, out property)) return property.AsFit;
                    return DefaultStyleValues_Generated.FitX;
                }
            }

            public UIForia.Layout.Fit FitY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitY, out property)) return property.AsFit;
                    return DefaultStyleValues_Generated.FitY;
                }
            }

            public UIForia.Rendering.UIMeasurement MinWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement MaxWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement PreferredWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement MinHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinHeight;
                }
            }

            public UIForia.Rendering.UIMeasurement MaxHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxHeight;
                }
            }

            public UIForia.Rendering.UIMeasurement PreferredHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredHeight;
                }
            }

            public UIForia.Rendering.UIMeasurement MarginTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginTop, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginTop;
                }
            }

            public UIForia.Rendering.UIMeasurement MarginRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginRight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginRight;
                }
            }

            public UIForia.Rendering.UIMeasurement MarginBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginBottom, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginBottom;
                }
            }

            public UIForia.Rendering.UIMeasurement MarginLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginLeft, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginLeft;
                }
            }

            public UnityEngine.Color BorderColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColor;
                }
            }

            public UIForia.UIFixedLength BorderTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderTop;
                }
            }

            public UIForia.UIFixedLength BorderRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRight;
                }
            }

            public UIForia.UIFixedLength BorderBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderBottom;
                }
            }

            public UIForia.UIFixedLength BorderLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomLeft;
                }
            }

            public UIForia.UIFixedLength PaddingTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingTop;
                }
            }

            public UIForia.UIFixedLength PaddingRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingRight;
                }
            }

            public UIForia.UIFixedLength PaddingBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingBottom;
                }
            }

            public UIForia.UIFixedLength PaddingLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingLeft;
                }
            }

            public UnityEngine.Color TextColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextColor;
                }
            }

            public UIForia.FontAsset TextFontAsset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontAsset, out property)) return property.AsFont;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontAsset), out property)) return property.AsFont;
                    return DefaultStyleValues_Generated.TextFontAsset;
                }
            }

            public UIForia.UIFixedLength TextFontSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontSize, out property)) return property.AsUIFixedLength;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontSize), out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TextFontSize;
                }
            }

            public UIForia.Text.FontStyle TextFontStyle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontStyle, out property)) return property.AsFontStyle;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontStyle), out property)) return property.AsFontStyle;
                    return DefaultStyleValues_Generated.TextFontStyle;
                }
            }

            public UIForia.Text.TextAlignment TextAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextAlignment, out property)) return property.AsTextAlignment;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextAlignment), out property)) return property.AsTextAlignment;
                    return DefaultStyleValues_Generated.TextAlignment;
                }
            }

            public float TextOutlineWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineWidth, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineWidth), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineWidth;
                }
            }

            public UnityEngine.Color TextOutlineColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextOutlineColor;
                }
            }

            public float TextOutlineSoftness { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineSoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineSoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineSoftness;
                }
            }

            public UnityEngine.Color TextGlowColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextGlowColor;
                }
            }

            public float TextGlowOffset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOffset, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOffset), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOffset;
                }
            }

            public float TextGlowInner { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowInner, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowInner), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowInner;
                }
            }

            public float TextGlowOuter { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOuter, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOuter), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOuter;
                }
            }

            public float TextGlowPower { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowPower, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowPower), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowPower;
                }
            }

            public UnityEngine.Color TextUnderlayColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextUnderlayColor;
                }
            }

            public float TextUnderlayX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayX, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayX), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayX;
                }
            }

            public float TextUnderlayY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayY, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayY), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayY;
                }
            }

            public float TextUnderlayDilate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayDilate;
                }
            }

            public float TextUnderlaySoftness { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlaySoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlaySoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlaySoftness;
                }
            }

            public float TextFaceDilate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFaceDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFaceDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextFaceDilate;
                }
            }

            public UIForia.Rendering.UnderlayType TextUnderlayType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayType, out property)) return property.AsUnderlayType;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayType), out property)) return property.AsUnderlayType;
                    return DefaultStyleValues_Generated.TextUnderlayType;
                }
            }

            public UIForia.Text.TextTransform TextTransform { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextTransform, out property)) return property.AsTextTransform;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextTransform), out property)) return property.AsTextTransform;
                    return DefaultStyleValues_Generated.TextTransform;
                }
            }

            public UIForia.Text.WhitespaceMode TextWhitespaceMode { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextWhitespaceMode, out property)) return property.AsWhitespaceMode;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextWhitespaceMode), out property)) return property.AsWhitespaceMode;
                    return DefaultStyleValues_Generated.TextWhitespaceMode;
                }
            }

            public UIForia.UIFixedLength AnchorTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorTop;
                }
            }

            public UIForia.UIFixedLength AnchorRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorRight;
                }
            }

            public UIForia.UIFixedLength AnchorBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorBottom;
                }
            }

            public UIForia.UIFixedLength AnchorLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorLeft;
                }
            }

            public UIForia.Rendering.AnchorTarget AnchorTarget { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorTarget, out property)) return property.AsAnchorTarget;
                    return DefaultStyleValues_Generated.AnchorTarget;
                }
            }

            public UIForia.TransformOffset TransformPositionX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionX, out property)) return property.AsTransformOffset;
                    return DefaultStyleValues_Generated.TransformPositionX;
                }
            }

            public UIForia.TransformOffset TransformPositionY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionY, out property)) return property.AsTransformOffset;
                    return DefaultStyleValues_Generated.TransformPositionY;
                }
            }

            public UIForia.UIFixedLength TransformPivotX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotX;
                }
            }

            public UIForia.UIFixedLength TransformPivotY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotY;
                }
            }

            public float TransformScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleX;
                }
            }

            public float TransformScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleY;
                }
            }

            public float TransformRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformRotation;
                }
            }

            public UIForia.Rendering.TransformBehavior TransformBehaviorX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorX, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorX;
                }
            }

            public UIForia.Rendering.TransformBehavior TransformBehaviorY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorY, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorY;
                }
            }

            public UIForia.Layout.LayoutType LayoutType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutType, out property)) return property.AsLayoutType;
                    return DefaultStyleValues_Generated.LayoutType;
                }
            }

            public UIForia.Layout.LayoutBehavior LayoutBehavior { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutBehavior, out property)) return property.AsLayoutBehavior;
                    return DefaultStyleValues_Generated.LayoutBehavior;
                }
            }

            public int ZIndex { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ZIndex, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.ZIndex), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.ZIndex;
                }
            }

            public int RenderLayerOffset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayerOffset, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.RenderLayerOffset;
                }
            }

            public UIForia.Rendering.RenderLayer RenderLayer { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayer, out property)) return property.AsRenderLayer;
                    return DefaultStyleValues_Generated.RenderLayer;
                }
            }

            public int Layer { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Layer, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Layer), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.Layer;
                }
            }

            public string Scrollbar { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Scrollbar, out property)) return property.AsString;
                    return DefaultStyleValues_Generated.Scrollbar;
                }
            }

            public UIForia.Rendering.UIMeasurement ScrollbarSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollbarSize, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.ScrollbarSize;
                }
            }

            public UnityEngine.Color ScrollbarColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollbarColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarColor;
                }
            }

            public UIForia.Rendering.UnderlayType ShadowType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowType, out property)) return property.AsUnderlayType;
                    return DefaultStyleValues_Generated.ShadowType;
                }
            }

            public float ShadowOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowOffsetX;
                }
            }

            public float ShadowOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowOffsetY;
                }
            }

            public float ShadowSoftnessX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSoftnessX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSoftnessX;
                }
            }

            public float ShadowSoftnessY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSoftnessY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSoftnessY;
                }
            }

            public float ShadowIntensity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowIntensity, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowIntensity;
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
        
        public void SetBackgroundColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state);
        }

        public UnityEngine.Color GetBackgroundColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor;
        }
        
        public void SetBackgroundTint(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state);
        }

        public UnityEngine.Color GetBackgroundTint(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor;
        }
        
        public void SetBorderColorTop(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state);
        }

        public UnityEngine.Color GetBorderColorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor;
        }
        
        public void SetBorderColorRight(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state);
        }

        public UnityEngine.Color GetBorderColorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor;
        }
        
        public void SetBorderColorBottom(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state);
        }

        public UnityEngine.Color GetBorderColorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor;
        }
        
        public void SetBorderColorLeft(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state);
        }

        public UnityEngine.Color GetBorderColorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor;
        }
        
        public void SetBackgroundImageOffsetX(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageOffsetY(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageScaleX(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageScaleY(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageTileX(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageTileX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageTileY(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageTileY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageRotation(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state);
        }

        public UnityEngine.Texture2D GetBackgroundImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D;
        }
        
        public void SetPainter(string value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Painter, value), state);
        }

        public string GetPainter(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Painter, state).AsString;
        }
        
        public void SetOpacity(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state);
        }

        public float GetOpacity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat;
        }
        
        public void SetCursor(UIForia.Rendering.CursorStyle value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state);
        }

        public UIForia.Rendering.CursorStyle GetCursor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle;
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
        
        public void SetFlexLayoutDirection(UIForia.Layout.LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state);
        }

        public UIForia.Layout.LayoutDirection GetFlexLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetFlexLayoutWrap(UIForia.Layout.LayoutWrap value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state);
        }

        public UIForia.Layout.LayoutWrap GetFlexLayoutWrap(StyleState state) {
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
        
        public void SetGridLayoutDirection(UIForia.Layout.LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state);
        }

        public UIForia.Layout.LayoutDirection GetGridLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetGridLayoutDensity(UIForia.Layout.GridLayoutDensity value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state);
        }

        public UIForia.Layout.GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity;
        }
        
        public void SetGridLayoutColTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutColTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutRowTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutRowTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutMainAxisAutoSize(UIForia.Layout.LayoutTypes.GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutMainAxisAutoSize, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridTrackSize GetGridLayoutMainAxisAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutMainAxisAutoSize, state).AsGridTrackSize;
        }
        
        public void SetGridLayoutCrossAxisAutoSize(UIForia.Layout.LayoutTypes.GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutCrossAxisAutoSize, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridTrackSize GetGridLayoutCrossAxisAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutCrossAxisAutoSize, state).AsGridTrackSize;
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
        
        public void SetRadialLayoutStartAngle(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value), state);
        }

        public float GetRadialLayoutStartAngle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutStartAngle, state).AsFloat;
        }
        
        public void SetRadialLayoutEndAngle(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value), state);
        }

        public float GetRadialLayoutEndAngle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutEndAngle, state).AsFloat;
        }
        
        public void SetRadialLayoutRadius(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state);
        }

        public UIForia.UIFixedLength GetRadialLayoutRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength;
        }
        
        public void SetAlignmentTargetX(UIForia.Layout.AlignmentTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state);
        }

        public UIForia.Layout.AlignmentTarget GetAlignmentTargetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentTargetY(UIForia.Layout.AlignmentTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state);
        }

        public UIForia.Layout.AlignmentTarget GetAlignmentTargetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentBehaviorX(UIForia.Layout.AlignmentBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value), state);
        }

        public UIForia.Layout.AlignmentBehavior GetAlignmentBehaviorX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentBehaviorX, state).AsAlignmentBehavior;
        }
        
        public void SetAlignmentBehaviorY(UIForia.Layout.AlignmentBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value), state);
        }

        public UIForia.Layout.AlignmentBehavior GetAlignmentBehaviorY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentBehaviorY, state).AsAlignmentBehavior;
        }
        
        public void SetAlignmentPivotX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotX, value), state);
        }

        public float GetAlignmentPivotX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentPivotX, state).AsFloat;
        }
        
        public void SetAlignmentPivotY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotY, value), state);
        }

        public float GetAlignmentPivotY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentPivotY, state).AsFloat;
        }
        
        public void SetAlignmentOffsetX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state);
        }

        public float GetAlignmentOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsFloat;
        }
        
        public void SetAlignmentOffsetY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state);
        }

        public float GetAlignmentOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsFloat;
        }
        
        public void SetFitX(UIForia.Layout.Fit value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value), state);
        }

        public UIForia.Layout.Fit GetFitX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitX, state).AsFit;
        }
        
        public void SetFitY(UIForia.Layout.Fit value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value), state);
        }

        public UIForia.Layout.Fit GetFitY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitY, state).AsFit;
        }
        
        public void SetMinWidth(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMinWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement;
        }
        
        public void SetMaxWidth(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMaxWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement;
        }
        
        public void SetPreferredWidth(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetPreferredWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement;
        }
        
        public void SetMinHeight(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMinHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement;
        }
        
        public void SetMaxHeight(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMaxHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement;
        }
        
        public void SetPreferredHeight(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetPreferredHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement;
        }
        
        public void SetMarginTop(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMarginTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement;
        }
        
        public void SetMarginRight(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMarginRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement;
        }
        
        public void SetMarginBottom(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMarginBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement;
        }
        
        public void SetMarginLeft(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMarginLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement;
        }
        
        public void SetBorderColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state);
        }

        public UnityEngine.Color GetBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor;
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
        
        public void SetTextFontAsset(UIForia.FontAsset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state);
        }

        public UIForia.FontAsset GetTextFontAsset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont;
        }
        
        public void SetTextFontSize(UIForia.UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state);
        }

        public UIForia.UIFixedLength GetTextFontSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength;
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
        
        public void SetTextOutlineWidth(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state);
        }

        public float GetTextOutlineWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat;
        }
        
        public void SetTextOutlineColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state);
        }

        public UnityEngine.Color GetTextOutlineColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor;
        }
        
        public void SetTextOutlineSoftness(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state);
        }

        public float GetTextOutlineSoftness(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat;
        }
        
        public void SetTextGlowColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state);
        }

        public UnityEngine.Color GetTextGlowColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowColor, state).AsColor;
        }
        
        public void SetTextGlowOffset(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value), state);
        }

        public float GetTextGlowOffset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowOffset, state).AsFloat;
        }
        
        public void SetTextGlowInner(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value), state);
        }

        public float GetTextGlowInner(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowInner, state).AsFloat;
        }
        
        public void SetTextGlowOuter(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value), state);
        }

        public float GetTextGlowOuter(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowOuter, state).AsFloat;
        }
        
        public void SetTextGlowPower(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value), state);
        }

        public float GetTextGlowPower(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowPower, state).AsFloat;
        }
        
        public void SetTextUnderlayColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state);
        }

        public UnityEngine.Color GetTextUnderlayColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayColor, state).AsColor;
        }
        
        public void SetTextUnderlayX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value), state);
        }

        public float GetTextUnderlayX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayX, state).AsFloat;
        }
        
        public void SetTextUnderlayY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value), state);
        }

        public float GetTextUnderlayY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayY, state).AsFloat;
        }
        
        public void SetTextUnderlayDilate(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value), state);
        }

        public float GetTextUnderlayDilate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayDilate, state).AsFloat;
        }
        
        public void SetTextUnderlaySoftness(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value), state);
        }

        public float GetTextUnderlaySoftness(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlaySoftness, state).AsFloat;
        }
        
        public void SetTextFaceDilate(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value), state);
        }

        public float GetTextFaceDilate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFaceDilate, state).AsFloat;
        }
        
        public void SetTextUnderlayType(UIForia.Rendering.UnderlayType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state);
        }

        public UIForia.Rendering.UnderlayType GetTextUnderlayType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType;
        }
        
        public void SetTextTransform(UIForia.Text.TextTransform value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state);
        }

        public UIForia.Text.TextTransform GetTextTransform(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform;
        }
        
        public void SetTextWhitespaceMode(UIForia.Text.WhitespaceMode value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state);
        }

        public UIForia.Text.WhitespaceMode GetTextWhitespaceMode(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode;
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
        
        public void SetTransformPositionX(UIForia.TransformOffset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state);
        }

        public UIForia.TransformOffset GetTransformPositionX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsTransformOffset;
        }
        
        public void SetTransformPositionY(UIForia.TransformOffset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state);
        }

        public UIForia.TransformOffset GetTransformPositionY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsTransformOffset;
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
        
        public void SetLayoutType(UIForia.Layout.LayoutType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state);
        }

        public UIForia.Layout.LayoutType GetLayoutType(StyleState state) {
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
        
        public void SetLayer(int value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Layer, value), state);
        }

        public int GetLayer(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Layer, state).AsInt;
        }
        
        public void SetScrollbar(string value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Scrollbar, value), state);
        }

        public string GetScrollbar(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Scrollbar, state).AsString;
        }
        
        public void SetScrollbarSize(UIForia.Rendering.UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetScrollbarSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarSize, state).AsUIMeasurement;
        }
        
        public void SetScrollbarColor(UnityEngine.Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value), state);
        }

        public UnityEngine.Color GetScrollbarColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarColor, state).AsColor;
        }
        
        public void SetShadowType(UIForia.Rendering.UnderlayType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value), state);
        }

        public UIForia.Rendering.UnderlayType GetShadowType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowType, state).AsUnderlayType;
        }
        
        public void SetShadowOffsetX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value), state);
        }

        public float GetShadowOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowOffsetX, state).AsFloat;
        }
        
        public void SetShadowOffsetY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value), state);
        }

        public float GetShadowOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowOffsetY, state).AsFloat;
        }
        
        public void SetShadowSoftnessX(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessX, value), state);
        }

        public float GetShadowSoftnessX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowSoftnessX, state).AsFloat;
        }
        
        public void SetShadowSoftnessY(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessY, value), state);
        }

        public float GetShadowSoftnessY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowSoftnessY, state).AsFloat;
        }
        
        public void SetShadowIntensity(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value), state);
        }

        public float GetShadowIntensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowIntensity, state).AsFloat;
        }
        

        public StyleProperty GetComputedStyleProperty(StylePropertyId propertyId) {
        			switch(propertyId) {
				case StylePropertyId.OverflowX:
					 return new StyleProperty(StylePropertyId.OverflowX, (int)OverflowX);
				case StylePropertyId.OverflowY:
					 return new StyleProperty(StylePropertyId.OverflowY, (int)OverflowY);
				case StylePropertyId.BackgroundColor:
					 return new StyleProperty(StylePropertyId.BackgroundColor, BackgroundColor);
				case StylePropertyId.BackgroundTint:
					 return new StyleProperty(StylePropertyId.BackgroundTint, BackgroundTint);
				case StylePropertyId.BorderColorTop:
					 return new StyleProperty(StylePropertyId.BorderColorTop, BorderColorTop);
				case StylePropertyId.BorderColorRight:
					 return new StyleProperty(StylePropertyId.BorderColorRight, BorderColorRight);
				case StylePropertyId.BorderColorBottom:
					 return new StyleProperty(StylePropertyId.BorderColorBottom, BorderColorBottom);
				case StylePropertyId.BorderColorLeft:
					 return new StyleProperty(StylePropertyId.BorderColorLeft, BorderColorLeft);
				case StylePropertyId.BackgroundImageOffsetX:
					 return new StyleProperty(StylePropertyId.BackgroundImageOffsetX, BackgroundImageOffsetX);
				case StylePropertyId.BackgroundImageOffsetY:
					 return new StyleProperty(StylePropertyId.BackgroundImageOffsetY, BackgroundImageOffsetY);
				case StylePropertyId.BackgroundImageScaleX:
					 return new StyleProperty(StylePropertyId.BackgroundImageScaleX, BackgroundImageScaleX);
				case StylePropertyId.BackgroundImageScaleY:
					 return new StyleProperty(StylePropertyId.BackgroundImageScaleY, BackgroundImageScaleY);
				case StylePropertyId.BackgroundImageTileX:
					 return new StyleProperty(StylePropertyId.BackgroundImageTileX, BackgroundImageTileX);
				case StylePropertyId.BackgroundImageTileY:
					 return new StyleProperty(StylePropertyId.BackgroundImageTileY, BackgroundImageTileY);
				case StylePropertyId.BackgroundImageRotation:
					 return new StyleProperty(StylePropertyId.BackgroundImageRotation, BackgroundImageRotation);
				case StylePropertyId.BackgroundImage:
					 return new StyleProperty(StylePropertyId.BackgroundImage, BackgroundImage);
				case StylePropertyId.Painter:
					 return new StyleProperty(StylePropertyId.Painter, Painter);
				case StylePropertyId.Opacity:
					 return new StyleProperty(StylePropertyId.Opacity, Opacity);
				case StylePropertyId.Cursor:
					 return new StyleProperty(StylePropertyId.Cursor, Cursor);
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
					 return new StyleProperty(StylePropertyId.GridLayoutColTemplate, GridLayoutColTemplate);
				case StylePropertyId.GridLayoutRowTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, GridLayoutRowTemplate);
				case StylePropertyId.GridLayoutMainAxisAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutMainAxisAutoSize, GridLayoutMainAxisAutoSize);
				case StylePropertyId.GridLayoutCrossAxisAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutCrossAxisAutoSize, GridLayoutCrossAxisAutoSize);
				case StylePropertyId.GridLayoutColGap:
					 return new StyleProperty(StylePropertyId.GridLayoutColGap, GridLayoutColGap);
				case StylePropertyId.GridLayoutRowGap:
					 return new StyleProperty(StylePropertyId.GridLayoutRowGap, GridLayoutRowGap);
				case StylePropertyId.GridLayoutColAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)GridLayoutColAlignment);
				case StylePropertyId.GridLayoutRowAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)GridLayoutRowAlignment);
				case StylePropertyId.RadialLayoutStartAngle:
					 return new StyleProperty(StylePropertyId.RadialLayoutStartAngle, RadialLayoutStartAngle);
				case StylePropertyId.RadialLayoutEndAngle:
					 return new StyleProperty(StylePropertyId.RadialLayoutEndAngle, RadialLayoutEndAngle);
				case StylePropertyId.RadialLayoutRadius:
					 return new StyleProperty(StylePropertyId.RadialLayoutRadius, RadialLayoutRadius);
				case StylePropertyId.AlignmentTargetX:
					 return new StyleProperty(StylePropertyId.AlignmentTargetX, (int)AlignmentTargetX);
				case StylePropertyId.AlignmentTargetY:
					 return new StyleProperty(StylePropertyId.AlignmentTargetY, (int)AlignmentTargetY);
				case StylePropertyId.AlignmentBehaviorX:
					 return new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)AlignmentBehaviorX);
				case StylePropertyId.AlignmentBehaviorY:
					 return new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)AlignmentBehaviorY);
				case StylePropertyId.AlignmentPivotX:
					 return new StyleProperty(StylePropertyId.AlignmentPivotX, AlignmentPivotX);
				case StylePropertyId.AlignmentPivotY:
					 return new StyleProperty(StylePropertyId.AlignmentPivotY, AlignmentPivotY);
				case StylePropertyId.AlignmentOffsetX:
					 return new StyleProperty(StylePropertyId.AlignmentOffsetX, AlignmentOffsetX);
				case StylePropertyId.AlignmentOffsetY:
					 return new StyleProperty(StylePropertyId.AlignmentOffsetY, AlignmentOffsetY);
				case StylePropertyId.FitX:
					 return new StyleProperty(StylePropertyId.FitX, (int)FitX);
				case StylePropertyId.FitY:
					 return new StyleProperty(StylePropertyId.FitY, (int)FitY);
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
				case StylePropertyId.BorderColor:
					 return new StyleProperty(StylePropertyId.BorderColor, BorderColor);
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
					 return new StyleProperty(StylePropertyId.TextFontAsset, TextFontAsset);
				case StylePropertyId.TextFontSize:
					 return new StyleProperty(StylePropertyId.TextFontSize, TextFontSize);
				case StylePropertyId.TextFontStyle:
					 return new StyleProperty(StylePropertyId.TextFontStyle, (int)TextFontStyle);
				case StylePropertyId.TextAlignment:
					 return new StyleProperty(StylePropertyId.TextAlignment, (int)TextAlignment);
				case StylePropertyId.TextOutlineWidth:
					 return new StyleProperty(StylePropertyId.TextOutlineWidth, TextOutlineWidth);
				case StylePropertyId.TextOutlineColor:
					 return new StyleProperty(StylePropertyId.TextOutlineColor, TextOutlineColor);
				case StylePropertyId.TextOutlineSoftness:
					 return new StyleProperty(StylePropertyId.TextOutlineSoftness, TextOutlineSoftness);
				case StylePropertyId.TextGlowColor:
					 return new StyleProperty(StylePropertyId.TextGlowColor, TextGlowColor);
				case StylePropertyId.TextGlowOffset:
					 return new StyleProperty(StylePropertyId.TextGlowOffset, TextGlowOffset);
				case StylePropertyId.TextGlowInner:
					 return new StyleProperty(StylePropertyId.TextGlowInner, TextGlowInner);
				case StylePropertyId.TextGlowOuter:
					 return new StyleProperty(StylePropertyId.TextGlowOuter, TextGlowOuter);
				case StylePropertyId.TextGlowPower:
					 return new StyleProperty(StylePropertyId.TextGlowPower, TextGlowPower);
				case StylePropertyId.TextUnderlayColor:
					 return new StyleProperty(StylePropertyId.TextUnderlayColor, TextUnderlayColor);
				case StylePropertyId.TextUnderlayX:
					 return new StyleProperty(StylePropertyId.TextUnderlayX, TextUnderlayX);
				case StylePropertyId.TextUnderlayY:
					 return new StyleProperty(StylePropertyId.TextUnderlayY, TextUnderlayY);
				case StylePropertyId.TextUnderlayDilate:
					 return new StyleProperty(StylePropertyId.TextUnderlayDilate, TextUnderlayDilate);
				case StylePropertyId.TextUnderlaySoftness:
					 return new StyleProperty(StylePropertyId.TextUnderlaySoftness, TextUnderlaySoftness);
				case StylePropertyId.TextFaceDilate:
					 return new StyleProperty(StylePropertyId.TextFaceDilate, TextFaceDilate);
				case StylePropertyId.TextUnderlayType:
					 return new StyleProperty(StylePropertyId.TextUnderlayType, (int)TextUnderlayType);
				case StylePropertyId.TextTransform:
					 return new StyleProperty(StylePropertyId.TextTransform, (int)TextTransform);
				case StylePropertyId.TextWhitespaceMode:
					 return new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)TextWhitespaceMode);
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
				case StylePropertyId.Layer:
					 return new StyleProperty(StylePropertyId.Layer, Layer);
				case StylePropertyId.Scrollbar:
					 return new StyleProperty(StylePropertyId.Scrollbar, Scrollbar);
				case StylePropertyId.ScrollbarSize:
					 return new StyleProperty(StylePropertyId.ScrollbarSize, ScrollbarSize);
				case StylePropertyId.ScrollbarColor:
					 return new StyleProperty(StylePropertyId.ScrollbarColor, ScrollbarColor);
				case StylePropertyId.ShadowType:
					 return new StyleProperty(StylePropertyId.ShadowType, (int)ShadowType);
				case StylePropertyId.ShadowOffsetX:
					 return new StyleProperty(StylePropertyId.ShadowOffsetX, ShadowOffsetX);
				case StylePropertyId.ShadowOffsetY:
					 return new StyleProperty(StylePropertyId.ShadowOffsetY, ShadowOffsetY);
				case StylePropertyId.ShadowSoftnessX:
					 return new StyleProperty(StylePropertyId.ShadowSoftnessX, ShadowSoftnessX);
				case StylePropertyId.ShadowSoftnessY:
					 return new StyleProperty(StylePropertyId.ShadowSoftnessY, ShadowSoftnessY);
				case StylePropertyId.ShadowIntensity:
					 return new StyleProperty(StylePropertyId.ShadowIntensity, ShadowIntensity);
				default: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
				}  
        }

    }

    public static partial class StyleUtil {
        
      public static bool CanAnimate(StylePropertyId propertyId) {
                switch (propertyId) {
    
                    case StylePropertyId.BackgroundColor: return true;
                    case StylePropertyId.BackgroundTint: return true;
                    case StylePropertyId.BorderColorTop: return true;
                    case StylePropertyId.BorderColorRight: return true;
                    case StylePropertyId.BorderColorBottom: return true;
                    case StylePropertyId.BorderColorLeft: return true;
                    case StylePropertyId.BackgroundImageOffsetX: return true;
                    case StylePropertyId.BackgroundImageOffsetY: return true;
                    case StylePropertyId.BackgroundImageScaleX: return true;
                    case StylePropertyId.BackgroundImageScaleY: return true;
                    case StylePropertyId.BackgroundImageTileX: return true;
                    case StylePropertyId.BackgroundImageTileY: return true;
                    case StylePropertyId.BackgroundImageRotation: return true;
                    case StylePropertyId.Opacity: return true;
                    case StylePropertyId.FlexItemOrder: return true;
                    case StylePropertyId.FlexItemGrow: return true;
                    case StylePropertyId.FlexItemShrink: return true;
                    case StylePropertyId.GridLayoutColGap: return true;
                    case StylePropertyId.GridLayoutRowGap: return true;
                    case StylePropertyId.RadialLayoutStartAngle: return true;
                    case StylePropertyId.RadialLayoutEndAngle: return true;
                    case StylePropertyId.RadialLayoutRadius: return true;
                    case StylePropertyId.AlignmentPivotX: return true;
                    case StylePropertyId.AlignmentPivotY: return true;
                    case StylePropertyId.AlignmentOffsetX: return true;
                    case StylePropertyId.AlignmentOffsetY: return true;
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
                    case StylePropertyId.BorderColor: return true;
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
                    case StylePropertyId.TextOutlineWidth: return true;
                    case StylePropertyId.TextOutlineColor: return true;
                    case StylePropertyId.TextOutlineSoftness: return true;
                    case StylePropertyId.TextGlowColor: return true;
                    case StylePropertyId.TextGlowOffset: return true;
                    case StylePropertyId.TextGlowInner: return true;
                    case StylePropertyId.TextGlowOuter: return true;
                    case StylePropertyId.TextGlowPower: return true;
                    case StylePropertyId.TextUnderlayColor: return true;
                    case StylePropertyId.TextUnderlayX: return true;
                    case StylePropertyId.TextUnderlayY: return true;
                    case StylePropertyId.TextUnderlayDilate: return true;
                    case StylePropertyId.TextUnderlaySoftness: return true;
                    case StylePropertyId.TextFaceDilate: return true;
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
                    case StylePropertyId.Layer: return true;
                    case StylePropertyId.ScrollbarColor: return true;
                    case StylePropertyId.ShadowOffsetX: return true;
                    case StylePropertyId.ShadowOffsetY: return true;
                    case StylePropertyId.ShadowSoftnessX: return true;
                    case StylePropertyId.ShadowSoftnessY: return true;
                    case StylePropertyId.ShadowIntensity: return true;

                }
    
                return false;
            }

        public static bool IsInherited(StylePropertyId propertyId) {
            switch (propertyId) {

                    case StylePropertyId.Opacity: return true;
                    case StylePropertyId.Visibility: return true;
                    case StylePropertyId.TextColor: return true;
                    case StylePropertyId.TextFontAsset: return true;
                    case StylePropertyId.TextFontSize: return true;
                    case StylePropertyId.TextFontStyle: return true;
                    case StylePropertyId.TextAlignment: return true;
                    case StylePropertyId.TextOutlineWidth: return true;
                    case StylePropertyId.TextOutlineColor: return true;
                    case StylePropertyId.TextOutlineSoftness: return true;
                    case StylePropertyId.TextGlowColor: return true;
                    case StylePropertyId.TextGlowOffset: return true;
                    case StylePropertyId.TextGlowInner: return true;
                    case StylePropertyId.TextGlowOuter: return true;
                    case StylePropertyId.TextGlowPower: return true;
                    case StylePropertyId.TextUnderlayColor: return true;
                    case StylePropertyId.TextUnderlayX: return true;
                    case StylePropertyId.TextUnderlayY: return true;
                    case StylePropertyId.TextUnderlayDilate: return true;
                    case StylePropertyId.TextUnderlaySoftness: return true;
                    case StylePropertyId.TextFaceDilate: return true;
                    case StylePropertyId.TextUnderlayType: return true;
                    case StylePropertyId.TextTransform: return true;
                    case StylePropertyId.TextWhitespaceMode: return true;
                    case StylePropertyId.ZIndex: return true;
                    case StylePropertyId.Layer: return true;

            }

            return false;
        }

    }

}