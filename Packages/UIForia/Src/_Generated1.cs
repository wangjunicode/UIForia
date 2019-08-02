
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {
    
    public partial struct UIStyleSetStateProxy {
        
        public Overflow OverflowX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state); }
        }
        
        public Overflow OverflowY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state); }
        }
        
        public Color BackgroundColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state); }
        }
        
        public Color BackgroundTint {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state); }
        }
        
        public Color BorderColorTop {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state); }
        }
        
        public Color BorderColorRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state); }
        }
        
        public Color BorderColorBottom {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state); }
        }
        
        public Color BorderColorLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state); }
        }
        
        public UIFixedLength BackgroundImageOffsetX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state); }
        }
        
        public UIFixedLength BackgroundImageOffsetY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state); }
        }
        
        public UIFixedLength BackgroundImageScaleX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state); }
        }
        
        public UIFixedLength BackgroundImageScaleY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state); }
        }
        
        public UIFixedLength BackgroundImageTileX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state); }
        }
        
        public UIFixedLength BackgroundImageTileY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state); }
        }
        
        public UIFixedLength BackgroundImageRotation {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state); }
        }
        
        public Texture2D BackgroundImage {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state); }
        }
        
        public string Painter {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Painter, state).AsString; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Painter, value), state); }
        }
        
        public float Opacity {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state); }
        }
        
        public CursorStyle Cursor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state); }
        }
        
        public Visibility Visibility {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Visibility, state).AsVisibility; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state); }
        }
        
        public int FlexItemOrder {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemOrder, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemOrder, value), state); }
        }
        
        public int FlexItemGrow {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value), state); }
        }
        
        public int FlexItemShrink {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value), state); }
        }
        
        public LayoutDirection FlexLayoutDirection {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state); }
        }
        
        public LayoutWrap FlexLayoutWrap {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state); }
        }
        
        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutMainAxisAlignment, state).AsMainAxisAlignment; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value), state); }
        }
        
        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutCrossAxisAlignment, state).AsCrossAxisAlignment; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value), state); }
        }
        
        public GridItemPlacement GridItemX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemX, state).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemX, value), state); }
        }
        
        public GridItemPlacement GridItemY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemY, state).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemY, value), state); }
        }
        
        public GridItemPlacement GridItemWidth {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemWidth, state).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value), state); }
        }
        
        public GridItemPlacement GridItemHeight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemHeight, state).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value), state); }
        }
        
        public LayoutDirection GridLayoutDirection {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state); }
        }
        
        public GridLayoutDensity GridLayoutDensity {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state); }
        }
        
        public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state); }
        }
        
        public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state); }
        }
        
        public GridTrackSize GridLayoutMainAxisAutoSize {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTrackSize; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state); }
        }
        
        public GridTrackSize GridLayoutCrossAxisAutoSize {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAutoSize, state).AsGridTrackSize; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state); }
        }
        
        public float GridLayoutColGap {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value), state); }
        }
        
        public float GridLayoutRowGap {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value), state); }
        }
        
        public GridAxisAlignment GridLayoutColAlignment {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state); }
        }
        
        public GridAxisAlignment GridLayoutRowAlignment {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsGridAxisAlignment; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state); }
        }
        
        public float RadialLayoutStartAngle {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutStartAngle, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value), state); }
        }
        
        public float RadialLayoutEndAngle {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutEndAngle, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value), state); }
        }
        
        public UIFixedLength RadialLayoutRadius {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state); }
        }
        
        public AlignmentTarget AlignmentTargetX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state); }
        }
        
        public AlignmentTarget AlignmentTargetY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state); }
        }
        
        public AlignmentBehavior AlignmentBehaviorX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBehaviorX, state).AsAlignmentBehavior; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value), state); }
        }
        
        public AlignmentBehavior AlignmentBehaviorY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBehaviorY, state).AsAlignmentBehavior; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value), state); }
        }
        
        public float AlignmentPivotX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentPivotX, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotX, value), state); }
        }
        
        public float AlignmentPivotY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentPivotY, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotY, value), state); }
        }
        
        public UIFixedLength AlignmentOffsetX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state); }
        }
        
        public UIFixedLength AlignmentOffsetY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state); }
        }
        
        public Fit FitX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitX, state).AsFit; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value), state); }
        }
        
        public Fit FitY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitY, state).AsFit; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value), state); }
        }
        
        public UIMeasurement MinWidth {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state); }
        }
        
        public UIMeasurement MaxWidth {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state); }
        }
        
        public UIMeasurement PreferredWidth {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state); }
        }
        
        public UIMeasurement MinHeight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state); }
        }
        
        public UIMeasurement MaxHeight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state); }
        }
        
        public UIMeasurement PreferredHeight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state); }
        }
        
        public UIMeasurement MarginTop {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state); }
        }
        
        public UIMeasurement MarginRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state); }
        }
        
        public UIMeasurement MarginBottom {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state); }
        }
        
        public UIMeasurement MarginLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state); }
        }
        
        public Color BorderColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state); }
        }
        
        public UIFixedLength BorderTop {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state); }
        }
        
        public UIFixedLength BorderRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state); }
        }
        
        public UIFixedLength BorderBottom {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state); }
        }
        
        public UIFixedLength BorderLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state); }
        }
        
        public UIFixedLength BorderRadiusTopLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state); }
        }
        
        public UIFixedLength BorderRadiusTopRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state); }
        }
        
        public UIFixedLength BorderRadiusBottomRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state); }
        }
        
        public UIFixedLength BorderRadiusBottomLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state); }
        }
        
        public UIFixedLength PaddingTop {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state); }
        }
        
        public UIFixedLength PaddingRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state); }
        }
        
        public UIFixedLength PaddingBottom {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state); }
        }
        
        public UIFixedLength PaddingLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state); }
        }
        
        public Color TextColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state); }
        }
        
        public FontAsset TextFontAsset {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state); }
        }
        
        public UIFixedLength TextFontSize {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state); }
        }
        
        public FontStyle TextFontStyle {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state); }
        }
        
        public TextAlignment TextAlignment {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state); }
        }
        
        public float TextOutlineWidth {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state); }
        }
        
        public Color TextOutlineColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state); }
        }
        
        public float TextOutlineSoftness {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state); }
        }
        
        public Color TextGlowColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state); }
        }
        
        public float TextGlowOffset {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOffset, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value), state); }
        }
        
        public float TextGlowInner {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowInner, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value), state); }
        }
        
        public float TextGlowOuter {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOuter, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value), state); }
        }
        
        public float TextGlowPower {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowPower, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value), state); }
        }
        
        public Color TextUnderlayColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state); }
        }
        
        public float TextUnderlayX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayX, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value), state); }
        }
        
        public float TextUnderlayY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayY, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value), state); }
        }
        
        public float TextUnderlayDilate {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayDilate, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value), state); }
        }
        
        public float TextUnderlaySoftness {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlaySoftness, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value), state); }
        }
        
        public float TextFaceDilate {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFaceDilate, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value), state); }
        }
        
        public UnderlayType TextUnderlayType {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state); }
        }
        
        public TextTransform TextTransform {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state); }
        }
        
        public WhitespaceMode TextWhitespaceMode {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state); }
        }
        
        public UIFixedLength AnchorTop {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorTop, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value), state); }
        }
        
        public UIFixedLength AnchorRight {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorRight, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value), state); }
        }
        
        public UIFixedLength AnchorBottom {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorBottom, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value), state); }
        }
        
        public UIFixedLength AnchorLeft {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorLeft, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value), state); }
        }
        
        public AnchorTarget AnchorTarget {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AnchorTarget, state).AsAnchorTarget; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value), state); }
        }
        
        public TransformOffset TransformPositionX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsTransformOffset; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state); }
        }
        
        public TransformOffset TransformPositionY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsTransformOffset; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state); }
        }
        
        public UIFixedLength TransformPivotX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state); }
        }
        
        public UIFixedLength TransformPivotY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotY, state).AsUIFixedLength; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state); }
        }
        
        public float TransformScaleX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value), state); }
        }
        
        public float TransformScaleY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value), state); }
        }
        
        public float TransformRotation {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformRotation, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value), state); }
        }
        
        public TransformBehavior TransformBehaviorX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsTransformBehavior; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value), state); }
        }
        
        public TransformBehavior TransformBehaviorY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformBehaviorY, state).AsTransformBehavior; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value), state); }
        }
        
        public LayoutType LayoutType {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state); }
        }
        
        public LayoutBehavior LayoutBehavior {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutBehavior, state).AsLayoutBehavior; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state); }
        }
        
        public int ZIndex {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ZIndex, value), state); }
        }
        
        public int RenderLayerOffset {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value), state); }
        }
        
        public RenderLayer RenderLayer {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state); }
        }
        
        public int Layer {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Layer, state).AsInt; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Layer, value), state); }
        }
        
        public string Scrollbar {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Scrollbar, state).AsString; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Scrollbar, value), state); }
        }
        
        public UIMeasurement ScrollbarSize {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarSize, state).AsUIMeasurement; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value), state); }
        }
        
        public Color ScrollbarColor {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollbarColor, state).AsColor; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value), state); }
        }
        
        public UnderlayType ShadowType {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowType, state).AsUnderlayType; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value), state); }
        }
        
        public float ShadowOffsetX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetX, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value), state); }
        }
        
        public float ShadowOffsetY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetY, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value), state); }
        }
        
        public float ShadowSoftnessX {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSoftnessX, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessX, value), state); }
        }
        
        public float ShadowSoftnessY {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSoftnessY, state).AsFloat; }
            [DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessY, value), state); }
        }
        
        public float ShadowIntensity {
            [DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowIntensity, state).AsFloat; }
            [DebuggerStepThrough]
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
                    case StylePropertyId.FlexLayoutDirection: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutWrap: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutMainAxisAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.FlexLayoutCrossAxisAlignment: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridItemX: return !IntUtil.IsDefined(int0) &&  objectField == null;
                    case StylePropertyId.GridItemY: return !IntUtil.IsDefined(int0) &&  objectField == null;
                    case StylePropertyId.GridItemWidth: return !IntUtil.IsDefined(int0) &&  objectField == null;
                    case StylePropertyId.GridItemHeight: return !IntUtil.IsDefined(int0) &&  objectField == null;
                    case StylePropertyId.GridLayoutDirection: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutDensity: return int0 == 0 || IntUtil.UnsetValue == int0;
                    case StylePropertyId.GridLayoutColTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutRowTemplate: return objectField == null;
                    case StylePropertyId.GridLayoutColAutoSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.GridLayoutRowAutoSize: return !FloatUtil.IsDefined(float1) || int1 == 0;
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
                    case StylePropertyId.AlignmentOffsetX: return !FloatUtil.IsDefined(float1) || int1 == 0;
                    case StylePropertyId.AlignmentOffsetY: return !FloatUtil.IsDefined(float1) || int1 == 0;
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
    
        
        public Overflow OverflowX {
            [DebuggerStepThrough]
            get { return (Overflow)FindEnumProperty(StylePropertyId.OverflowX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value)); }
        }
            
        public Overflow OverflowY {
            [DebuggerStepThrough]
            get { return (Overflow)FindEnumProperty(StylePropertyId.OverflowY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value)); }
        }
            
        public Color BackgroundColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value)); }
        }
            
        public Color BackgroundTint {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundTint); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value)); }
        }
            
        public Color BorderColorTop {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorTop); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value)); }
        }
            
        public Color BorderColorRight {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value)); }
        }
            
        public Color BorderColorBottom {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorBottom); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value)); }
        }
            
        public Color BorderColorLeft {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value)); }
        }
            
        public UIFixedLength BackgroundImageOffsetX {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value)); }
        }
            
        public UIFixedLength BackgroundImageOffsetY {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value)); }
        }
            
        public UIFixedLength BackgroundImageScaleX {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageScaleX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value)); }
        }
            
        public UIFixedLength BackgroundImageScaleY {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageScaleY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value)); }
        }
            
        public UIFixedLength BackgroundImageTileX {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageTileX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value)); }
        }
            
        public UIFixedLength BackgroundImageTileY {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageTileY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value)); }
        }
            
        public UIFixedLength BackgroundImageRotation {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageRotation); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value)); }
        }
            
        public Texture2D BackgroundImage {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage).AsTexture2D; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value)); }
        }
            
        public string Painter {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Painter).AsString; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Painter, value)); }
        }
            
        public float Opacity {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.Opacity); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Opacity, value)); }
        }
            
        public CursorStyle Cursor {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Cursor).AsCursorStyle; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Cursor, value)); }
        }
            
        public Visibility Visibility {
            [DebuggerStepThrough]
            get { return (Visibility)FindEnumProperty(StylePropertyId.Visibility); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value)); }
        }
            
        public int FlexItemOrder {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemOrder); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemOrder, value)); }
        }
            
        public int FlexItemGrow {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemGrow); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value)); }
        }
            
        public int FlexItemShrink {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemShrink); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value)); }
        }
            
        public LayoutDirection FlexLayoutDirection {
            [DebuggerStepThrough]
            get { return (LayoutDirection)FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value)); }
        }
            
        public LayoutWrap FlexLayoutWrap {
            [DebuggerStepThrough]
            get { return (LayoutWrap)FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value)); }
        }
            
        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            [DebuggerStepThrough]
            get { return (MainAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value)); }
        }
            
        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [DebuggerStepThrough]
            get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value)); }
        }
            
        public GridItemPlacement GridItemX {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemX).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemX, value)); }
        }
            
        public GridItemPlacement GridItemY {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemY).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemY, value)); }
        }
            
        public GridItemPlacement GridItemWidth {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemWidth).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value)); }
        }
            
        public GridItemPlacement GridItemHeight {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemHeight).AsGridItemPlacement; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value)); }
        }
            
        public LayoutDirection GridLayoutDirection {
            [DebuggerStepThrough]
            get { return (LayoutDirection)FindEnumProperty(StylePropertyId.GridLayoutDirection); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value)); }
        }
            
        public GridLayoutDensity GridLayoutDensity {
            [DebuggerStepThrough]
            get { return (GridLayoutDensity)FindEnumProperty(StylePropertyId.GridLayoutDensity); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value)); }
        }
            
        public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutColTemplate).AsGridTemplate; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value)); }
        }
            
        public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutRowTemplate).AsGridTemplate; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value)); }
        }
            
        public GridTrackSize GridLayoutColAutoSize {
            [DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value)); }
        }
            
        public GridTrackSize GridLayoutRowAutoSize {
            [DebuggerStepThrough]
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value)); }
        }
            
        public float GridLayoutColGap {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutColGap); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value)); }
        }
            
        public float GridLayoutRowGap {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutRowGap); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value)); }
        }
            
        public GridAxisAlignment GridLayoutColAlignment {
            [DebuggerStepThrough]
            get { return (GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutColAlignment); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value)); }
        }
            
        public GridAxisAlignment GridLayoutRowAlignment {
            [DebuggerStepThrough]
            get { return (GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutRowAlignment); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value)); }
        }
            
        public float RadialLayoutStartAngle {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutStartAngle); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value)); }
        }
            
        public float RadialLayoutEndAngle {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutEndAngle); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value)); }
        }
            
        public UIFixedLength RadialLayoutRadius {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.RadialLayoutRadius); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value)); }
        }
            
        public AlignmentTarget AlignmentTargetX {
            [DebuggerStepThrough]
            get { return (AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value)); }
        }
            
        public AlignmentTarget AlignmentTargetY {
            [DebuggerStepThrough]
            get { return (AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value)); }
        }
            
        public AlignmentBehavior AlignmentBehaviorX {
            [DebuggerStepThrough]
            get { return (AlignmentBehavior)FindEnumProperty(StylePropertyId.AlignmentBehaviorX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value)); }
        }
            
        public AlignmentBehavior AlignmentBehaviorY {
            [DebuggerStepThrough]
            get { return (AlignmentBehavior)FindEnumProperty(StylePropertyId.AlignmentBehaviorY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value)); }
        }
            
        public float AlignmentPivotX {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentPivotX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotX, value)); }
        }
            
        public float AlignmentPivotY {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignmentPivotY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentPivotY, value)); }
        }
            
        public UIFixedLength AlignmentOffsetX {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AlignmentOffsetX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value)); }
        }
            
        public UIFixedLength AlignmentOffsetY {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AlignmentOffsetY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value)); }
        }
            
        public Fit FitX {
            [DebuggerStepThrough]
            get { return (Fit)FindEnumProperty(StylePropertyId.FitX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value)); }
        }
            
        public Fit FitY {
            [DebuggerStepThrough]
            get { return (Fit)FindEnumProperty(StylePropertyId.FitY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value)); }
        }
            
        public UIMeasurement MinWidth {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinWidth); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinWidth, value)); }
        }
            
        public UIMeasurement MaxWidth {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxWidth); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value)); }
        }
            
        public UIMeasurement PreferredWidth {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredWidth); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value)); }
        }
            
        public UIMeasurement MinHeight {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinHeight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinHeight, value)); }
        }
            
        public UIMeasurement MaxHeight {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxHeight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value)); }
        }
            
        public UIMeasurement PreferredHeight {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredHeight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value)); }
        }
            
        public UIMeasurement MarginTop {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginTop); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginTop, value)); }
        }
            
        public UIMeasurement MarginRight {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginRight, value)); }
        }
            
        public UIMeasurement MarginBottom {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginBottom); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value)); }
        }
            
        public UIMeasurement MarginLeft {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MarginLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value)); }
        }
            
        public Color BorderColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColor, value)); }
        }
            
        public UIFixedLength BorderTop {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderTop); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderTop, value)); }
        }
            
        public UIFixedLength BorderRight {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRight, value)); }
        }
            
        public UIFixedLength BorderBottom {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderBottom); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value)); }
        }
            
        public UIFixedLength BorderLeft {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value)); }
        }
            
        public UIFixedLength BorderRadiusTopLeft {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value)); }
        }
            
        public UIFixedLength BorderRadiusTopRight {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value)); }
        }
            
        public UIFixedLength BorderRadiusBottomRight {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value)); }
        }
            
        public UIFixedLength BorderRadiusBottomLeft {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value)); }
        }
            
        public UIFixedLength PaddingTop {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingTop); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value)); }
        }
            
        public UIFixedLength PaddingRight {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value)); }
        }
            
        public UIFixedLength PaddingBottom {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingBottom); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value)); }
        }
            
        public UIFixedLength PaddingLeft {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value)); }
        }
            
        public Color TextColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextColor, value)); }
        }
            
        public FontAsset TextFontAsset {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.TextFontAsset).AsFont; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value)); }
        }
            
        public UIFixedLength TextFontSize {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TextFontSize); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value)); }
        }
            
        public FontStyle TextFontStyle {
            [DebuggerStepThrough]
            get { return (FontStyle)FindEnumProperty(StylePropertyId.TextFontStyle); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value)); }
        }
            
        public TextAlignment TextAlignment {
            [DebuggerStepThrough]
            get { return (TextAlignment)FindEnumProperty(StylePropertyId.TextAlignment); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value)); }
        }
            
        public float TextOutlineWidth {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineWidth); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value)); }
        }
            
        public Color TextOutlineColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextOutlineColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value)); }
        }
            
        public float TextOutlineSoftness {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineSoftness); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value)); }
        }
            
        public Color TextGlowColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextGlowColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value)); }
        }
            
        public float TextGlowOffset {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOffset); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value)); }
        }
            
        public float TextGlowInner {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowInner); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value)); }
        }
            
        public float TextGlowOuter {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOuter); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value)); }
        }
            
        public float TextGlowPower {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowPower); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value)); }
        }
            
        public Color TextUnderlayColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextUnderlayColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value)); }
        }
            
        public float TextUnderlayX {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value)); }
        }
            
        public float TextUnderlayY {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value)); }
        }
            
        public float TextUnderlayDilate {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayDilate); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value)); }
        }
            
        public float TextUnderlaySoftness {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlaySoftness); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value)); }
        }
            
        public float TextFaceDilate {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextFaceDilate); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value)); }
        }
            
        public UnderlayType TextUnderlayType {
            [DebuggerStepThrough]
            get { return (UnderlayType)FindEnumProperty(StylePropertyId.TextUnderlayType); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value)); }
        }
            
        public TextTransform TextTransform {
            [DebuggerStepThrough]
            get { return (TextTransform)FindEnumProperty(StylePropertyId.TextTransform); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value)); }
        }
            
        public WhitespaceMode TextWhitespaceMode {
            [DebuggerStepThrough]
            get { return (WhitespaceMode)FindEnumProperty(StylePropertyId.TextWhitespaceMode); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value)); }
        }
            
        public UIFixedLength AnchorTop {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorTop); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value)); }
        }
            
        public UIFixedLength AnchorRight {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorRight); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value)); }
        }
            
        public UIFixedLength AnchorBottom {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorBottom); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value)); }
        }
            
        public UIFixedLength AnchorLeft {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.AnchorLeft); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value)); }
        }
            
        public AnchorTarget AnchorTarget {
            [DebuggerStepThrough]
            get { return (AnchorTarget)FindEnumProperty(StylePropertyId.AnchorTarget); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value)); }
        }
            
        public TransformOffset TransformPositionX {
            [DebuggerStepThrough]
            get { return FindTransformOffsetProperty(StylePropertyId.TransformPositionX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value)); }
        }
            
        public TransformOffset TransformPositionY {
            [DebuggerStepThrough]
            get { return FindTransformOffsetProperty(StylePropertyId.TransformPositionY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value)); }
        }
            
        public UIFixedLength TransformPivotX {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value)); }
        }
            
        public UIFixedLength TransformPivotY {
            [DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value)); }
        }
            
        public float TransformScaleX {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value)); }
        }
            
        public float TransformScaleY {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value)); }
        }
            
        public float TransformRotation {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformRotation); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value)); }
        }
            
        public TransformBehavior TransformBehaviorX {
            [DebuggerStepThrough]
            get { return (TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value)); }
        }
            
        public TransformBehavior TransformBehaviorY {
            [DebuggerStepThrough]
            get { return (TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value)); }
        }
            
        public LayoutType LayoutType {
            [DebuggerStepThrough]
            get { return (LayoutType)FindEnumProperty(StylePropertyId.LayoutType); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value)); }
        }
            
        public LayoutBehavior LayoutBehavior {
            [DebuggerStepThrough]
            get { return (LayoutBehavior)FindEnumProperty(StylePropertyId.LayoutBehavior); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value)); }
        }
            
        public int ZIndex {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.ZIndex); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ZIndex, value)); }
        }
            
        public int RenderLayerOffset {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.RenderLayerOffset); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value)); }
        }
            
        public RenderLayer RenderLayer {
            [DebuggerStepThrough]
            get { return (RenderLayer)FindEnumProperty(StylePropertyId.RenderLayer); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value)); }
        }
            
        public int Layer {
            [DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.Layer); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Layer, value)); }
        }
            
        public string Scrollbar {
            [DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Scrollbar).AsString; }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Scrollbar, value)); }
        }
            
        public UIMeasurement ScrollbarSize {
            [DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.ScrollbarSize); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value)); }
        }
            
        public Color ScrollbarColor {
            [DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ScrollbarColor); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value)); }
        }
            
        public UnderlayType ShadowType {
            [DebuggerStepThrough]
            get { return (UnderlayType)FindEnumProperty(StylePropertyId.ShadowType); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value)); }
        }
            
        public float ShadowOffsetX {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowOffsetX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value)); }
        }
            
        public float ShadowOffsetY {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowOffsetY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value)); }
        }
            
        public float ShadowSoftnessX {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSoftnessX); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessX, value)); }
        }
            
        public float ShadowSoftnessY {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSoftnessY); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSoftnessY, value)); }
        }
            
        public float ShadowIntensity {
            [DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowIntensity); }
            [DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value)); }
        }
            
        
    }

    public partial class UIStyleSet {
    
        

            public Overflow OverflowX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowX, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowX;
                }
            }

            public Overflow OverflowY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowY, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowY;
                }
            }

            public Color BackgroundColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundColor;
                }
            }

            public Color BackgroundTint { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundTint, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundTint;
                }
            }

            public Color BorderColorTop { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorTop, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorTop;
                }
            }

            public Color BorderColorRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorRight, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorRight;
                }
            }

            public Color BorderColorBottom { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorBottom, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorBottom;
                }
            }

            public Color BorderColorLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorLeft, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorLeft;
                }
            }

            public UIFixedLength BackgroundImageOffsetX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetX;
                }
            }

            public UIFixedLength BackgroundImageOffsetY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetY;
                }
            }

            public UIFixedLength BackgroundImageScaleX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageScaleX;
                }
            }

            public UIFixedLength BackgroundImageScaleY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageScaleY;
                }
            }

            public UIFixedLength BackgroundImageTileX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageTileX;
                }
            }

            public UIFixedLength BackgroundImageTileY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageTileY;
                }
            }

            public UIFixedLength BackgroundImageRotation { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageRotation, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageRotation;
                }
            }

            public Texture2D BackgroundImage { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage;
                }
            }

            public string Painter { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Painter, out property)) return property.AsString;
                    return DefaultStyleValues_Generated.Painter;
                }
            }

            public float Opacity { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Opacity, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Opacity), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.Opacity;
                }
            }

            public CursorStyle Cursor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Cursor, out property)) return property.AsCursorStyle;
                    return DefaultStyleValues_Generated.Cursor;
                }
            }

            public Visibility Visibility { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Visibility, out property)) return property.AsVisibility;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Visibility), out property)) return property.AsVisibility;
                    return DefaultStyleValues_Generated.Visibility;
                }
            }

            public int FlexItemOrder { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemOrder, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemOrder;
                }
            }

            public int FlexItemGrow { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemGrow, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemGrow;
                }
            }

            public int FlexItemShrink { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemShrink, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemShrink;
                }
            }

            public LayoutDirection FlexLayoutDirection { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.FlexLayoutDirection;
                }
            }

            public LayoutWrap FlexLayoutWrap { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutWrap, out property)) return property.AsLayoutWrap;
                    return DefaultStyleValues_Generated.FlexLayoutWrap;
                }
            }

            public MainAxisAlignment FlexLayoutMainAxisAlignment { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutMainAxisAlignment, out property)) return property.AsMainAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutMainAxisAlignment;
                }
            }

            public CrossAxisAlignment FlexLayoutCrossAxisAlignment { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutCrossAxisAlignment, out property)) return property.AsCrossAxisAlignment;
                    return DefaultStyleValues_Generated.FlexLayoutCrossAxisAlignment;
                }
            }

            public GridItemPlacement GridItemX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemX, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemX;
                }
            }

            public GridItemPlacement GridItemY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemY, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemY;
                }
            }

            public GridItemPlacement GridItemWidth { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemWidth, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemWidth;
                }
            }

            public GridItemPlacement GridItemHeight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemHeight, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemHeight;
                }
            }

            public LayoutDirection GridLayoutDirection { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.GridLayoutDirection;
                }
            }

            public GridLayoutDensity GridLayoutDensity { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDensity, out property)) return property.AsGridLayoutDensity;
                    return DefaultStyleValues_Generated.GridLayoutDensity;
                }
            }

            public IReadOnlyList<GridTrackSize> GridLayoutColTemplate { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutColTemplate;
                }
            }

            public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutRowTemplate;
                }
            }

            public GridTrackSize GridLayoutColAutoSize { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutMainAxisAutoSize;
                }
            }

            public GridTrackSize GridLayoutRowAutoSize { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAutoSize, out property)) return property.AsGridTrackSize;
                    return DefaultStyleValues_Generated.GridLayoutCrossAxisAutoSize;
                }
            }

            public float GridLayoutColGap { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutColGap;
                }
            }

            public float GridLayoutRowGap { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutRowGap;
                }
            }

            public GridAxisAlignment GridLayoutColAlignment { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutColAlignment;
                }
            }

            public GridAxisAlignment GridLayoutRowAlignment { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutRowAlignment;
                }
            }

            public float RadialLayoutStartAngle { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutStartAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutStartAngle;
                }
            }

            public float RadialLayoutEndAngle { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutEndAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutEndAngle;
                }
            }

            public UIFixedLength RadialLayoutRadius { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutRadius, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.RadialLayoutRadius;
                }
            }

            public AlignmentTarget AlignmentTargetX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetX, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetX;
                }
            }

            public AlignmentTarget AlignmentTargetY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetY, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetY;
                }
            }

            public AlignmentBehavior AlignmentBehaviorX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBehaviorX, out property)) return property.AsAlignmentBehavior;
                    return DefaultStyleValues_Generated.AlignmentBehaviorX;
                }
            }

            public AlignmentBehavior AlignmentBehaviorY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBehaviorY, out property)) return property.AsAlignmentBehavior;
                    return DefaultStyleValues_Generated.AlignmentBehaviorY;
                }
            }

            public float AlignmentPivotX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentPivotX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentPivotX;
                }
            }

            public float AlignmentPivotY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentPivotY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignmentPivotY;
                }
            }

            public UIFixedLength AlignmentOffsetX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AlignmentOffsetX;
                }
            }

            public UIFixedLength AlignmentOffsetY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AlignmentOffsetY;
                }
            }

            public Fit FitX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitX, out property)) return property.AsFit;
                    return DefaultStyleValues_Generated.FitX;
                }
            }

            public Fit FitY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitY, out property)) return property.AsFit;
                    return DefaultStyleValues_Generated.FitY;
                }
            }

            public UIMeasurement MinWidth { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinWidth;
                }
            }

            public UIMeasurement MaxWidth { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxWidth;
                }
            }

            public UIMeasurement PreferredWidth { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredWidth;
                }
            }

            public UIMeasurement MinHeight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinHeight;
                }
            }

            public UIMeasurement MaxHeight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxHeight;
                }
            }

            public UIMeasurement PreferredHeight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredHeight;
                }
            }

            public UIMeasurement MarginTop { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginTop, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginTop;
                }
            }

            public UIMeasurement MarginRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginRight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginRight;
                }
            }

            public UIMeasurement MarginBottom { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginBottom, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginBottom;
                }
            }

            public UIMeasurement MarginLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginLeft, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MarginLeft;
                }
            }

            public Color BorderColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColor;
                }
            }

            public UIFixedLength BorderTop { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderTop;
                }
            }

            public UIFixedLength BorderRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRight;
                }
            }

            public UIFixedLength BorderBottom { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderBottom;
                }
            }

            public UIFixedLength BorderLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderLeft;
                }
            }

            public UIFixedLength BorderRadiusTopLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopLeft;
                }
            }

            public UIFixedLength BorderRadiusTopRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopRight;
                }
            }

            public UIFixedLength BorderRadiusBottomRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomRight;
                }
            }

            public UIFixedLength BorderRadiusBottomLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomLeft;
                }
            }

            public UIFixedLength PaddingTop { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingTop;
                }
            }

            public UIFixedLength PaddingRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingRight;
                }
            }

            public UIFixedLength PaddingBottom { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingBottom;
                }
            }

            public UIFixedLength PaddingLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingLeft;
                }
            }

            public Color TextColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextColor;
                }
            }

            public FontAsset TextFontAsset { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontAsset, out property)) return property.AsFont;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontAsset), out property)) return property.AsFont;
                    return DefaultStyleValues_Generated.TextFontAsset;
                }
            }

            public UIFixedLength TextFontSize { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontSize, out property)) return property.AsUIFixedLength;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontSize), out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TextFontSize;
                }
            }

            public FontStyle TextFontStyle { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontStyle, out property)) return property.AsFontStyle;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontStyle), out property)) return property.AsFontStyle;
                    return DefaultStyleValues_Generated.TextFontStyle;
                }
            }

            public TextAlignment TextAlignment { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextAlignment, out property)) return property.AsTextAlignment;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextAlignment), out property)) return property.AsTextAlignment;
                    return DefaultStyleValues_Generated.TextAlignment;
                }
            }

            public float TextOutlineWidth { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineWidth, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineWidth), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineWidth;
                }
            }

            public Color TextOutlineColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextOutlineColor;
                }
            }

            public float TextOutlineSoftness { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineSoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineSoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineSoftness;
                }
            }

            public Color TextGlowColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextGlowColor;
                }
            }

            public float TextGlowOffset { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOffset, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOffset), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOffset;
                }
            }

            public float TextGlowInner { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowInner, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowInner), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowInner;
                }
            }

            public float TextGlowOuter { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOuter, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOuter), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOuter;
                }
            }

            public float TextGlowPower { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowPower, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowPower), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowPower;
                }
            }

            public Color TextUnderlayColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextUnderlayColor;
                }
            }

            public float TextUnderlayX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayX, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayX), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayX;
                }
            }

            public float TextUnderlayY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayY, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayY), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayY;
                }
            }

            public float TextUnderlayDilate { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayDilate;
                }
            }

            public float TextUnderlaySoftness { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlaySoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlaySoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlaySoftness;
                }
            }

            public float TextFaceDilate { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFaceDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFaceDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextFaceDilate;
                }
            }

            public UnderlayType TextUnderlayType { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayType, out property)) return property.AsUnderlayType;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayType), out property)) return property.AsUnderlayType;
                    return DefaultStyleValues_Generated.TextUnderlayType;
                }
            }

            public TextTransform TextTransform { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextTransform, out property)) return property.AsTextTransform;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextTransform), out property)) return property.AsTextTransform;
                    return DefaultStyleValues_Generated.TextTransform;
                }
            }

            public WhitespaceMode TextWhitespaceMode { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextWhitespaceMode, out property)) return property.AsWhitespaceMode;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextWhitespaceMode), out property)) return property.AsWhitespaceMode;
                    return DefaultStyleValues_Generated.TextWhitespaceMode;
                }
            }

            public UIFixedLength AnchorTop { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorTop;
                }
            }

            public UIFixedLength AnchorRight { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorRight;
                }
            }

            public UIFixedLength AnchorBottom { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorBottom;
                }
            }

            public UIFixedLength AnchorLeft { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.AnchorLeft;
                }
            }

            public AnchorTarget AnchorTarget { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AnchorTarget, out property)) return property.AsAnchorTarget;
                    return DefaultStyleValues_Generated.AnchorTarget;
                }
            }

            public TransformOffset TransformPositionX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionX, out property)) return property.AsTransformOffset;
                    return DefaultStyleValues_Generated.TransformPositionX;
                }
            }

            public TransformOffset TransformPositionY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionY, out property)) return property.AsTransformOffset;
                    return DefaultStyleValues_Generated.TransformPositionY;
                }
            }

            public UIFixedLength TransformPivotX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotX;
                }
            }

            public UIFixedLength TransformPivotY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotY;
                }
            }

            public float TransformScaleX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleX;
                }
            }

            public float TransformScaleY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleY;
                }
            }

            public float TransformRotation { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformRotation;
                }
            }

            public TransformBehavior TransformBehaviorX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorX, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorX;
                }
            }

            public TransformBehavior TransformBehaviorY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformBehaviorY, out property)) return property.AsTransformBehavior;
                    return DefaultStyleValues_Generated.TransformBehaviorY;
                }
            }

            public LayoutType LayoutType { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutType, out property)) return property.AsLayoutType;
                    return DefaultStyleValues_Generated.LayoutType;
                }
            }

            public LayoutBehavior LayoutBehavior { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutBehavior, out property)) return property.AsLayoutBehavior;
                    return DefaultStyleValues_Generated.LayoutBehavior;
                }
            }

            public int ZIndex { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ZIndex, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.ZIndex), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.ZIndex;
                }
            }

            public int RenderLayerOffset { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayerOffset, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.RenderLayerOffset;
                }
            }

            public RenderLayer RenderLayer { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayer, out property)) return property.AsRenderLayer;
                    return DefaultStyleValues_Generated.RenderLayer;
                }
            }

            public int Layer { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Layer, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Layer), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.Layer;
                }
            }

            public string Scrollbar { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Scrollbar, out property)) return property.AsString;
                    return DefaultStyleValues_Generated.Scrollbar;
                }
            }

            public UIMeasurement ScrollbarSize { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollbarSize, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.ScrollbarSize;
                }
            }

            public Color ScrollbarColor { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollbarColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ScrollbarColor;
                }
            }

            public UnderlayType ShadowType { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowType, out property)) return property.AsUnderlayType;
                    return DefaultStyleValues_Generated.ShadowType;
                }
            }

            public float ShadowOffsetX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowOffsetX;
                }
            }

            public float ShadowOffsetY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowOffsetY;
                }
            }

            public float ShadowSoftnessX { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSoftnessX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSoftnessX;
                }
            }

            public float ShadowSoftnessY { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSoftnessY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSoftnessY;
                }
            }

            public float ShadowIntensity { 
                [DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowIntensity, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowIntensity;
                }
            }

        
        public void SetOverflowX(Overflow value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state);
        }

        public Overflow GetOverflowX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow;
        }
        
        public void SetOverflowY(Overflow value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state);
        }

        public Overflow GetOverflowY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow;
        }
        
        public void SetBackgroundColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state);
        }

        public Color GetBackgroundColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor;
        }
        
        public void SetBackgroundTint(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state);
        }

        public Color GetBackgroundTint(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor;
        }
        
        public void SetBorderColorTop(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state);
        }

        public Color GetBorderColorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor;
        }
        
        public void SetBorderColorRight(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state);
        }

        public Color GetBorderColorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor;
        }
        
        public void SetBorderColorBottom(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state);
        }

        public Color GetBorderColorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor;
        }
        
        public void SetBorderColorLeft(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state);
        }

        public Color GetBorderColorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor;
        }
        
        public void SetBackgroundImageOffsetX(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state);
        }

        public UIFixedLength GetBackgroundImageOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageOffsetY(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state);
        }

        public UIFixedLength GetBackgroundImageOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageScaleX(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state);
        }

        public UIFixedLength GetBackgroundImageScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageScaleY(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state);
        }

        public UIFixedLength GetBackgroundImageScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageTileX(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state);
        }

        public UIFixedLength GetBackgroundImageTileX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageTileY(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state);
        }

        public UIFixedLength GetBackgroundImageTileY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageRotation(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state);
        }

        public UIFixedLength GetBackgroundImageRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImage(Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state);
        }

        public Texture2D GetBackgroundImage(StyleState state) {
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
        
        public void SetCursor(CursorStyle value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state);
        }

        public CursorStyle GetCursor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle;
        }
        
        public void SetVisibility(Visibility value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state);
        }

        public Visibility GetVisibility(StyleState state) {
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
        
        public void SetFlexLayoutDirection(LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state);
        }

        public LayoutDirection GetFlexLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetFlexLayoutWrap(LayoutWrap value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state);
        }

        public LayoutWrap GetFlexLayoutWrap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap;
        }
        
        public void SetFlexLayoutMainAxisAlignment(MainAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value), state);
        }

        public MainAxisAlignment GetFlexLayoutMainAxisAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutMainAxisAlignment, state).AsMainAxisAlignment;
        }
        
        public void SetFlexLayoutCrossAxisAlignment(CrossAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value), state);
        }

        public CrossAxisAlignment GetFlexLayoutCrossAxisAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutCrossAxisAlignment, state).AsCrossAxisAlignment;
        }
        
        public void SetGridItemX(GridItemPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemX, value), state);
        }

        public GridItemPlacement GetGridItemX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemX, state).AsGridItemPlacement;
        }
        
        public void SetGridItemY(GridItemPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemY, value), state);
        }

        public GridItemPlacement GetGridItemY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemY, state).AsGridItemPlacement;
        }
        
        public void SetGridItemWidth(GridItemPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value), state);
        }

        public GridItemPlacement GetGridItemWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemWidth, state).AsGridItemPlacement;
        }
        
        public void SetGridItemHeight(GridItemPlacement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value), state);
        }

        public GridItemPlacement GetGridItemHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemHeight, state).AsGridItemPlacement;
        }
        
        public void SetGridLayoutDirection(LayoutDirection value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state);
        }

        public LayoutDirection GetGridLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetGridLayoutDensity(GridLayoutDensity value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state);
        }

        public GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity;
        }
        
        public void SetGridLayoutColTemplate(IReadOnlyList<GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state);
        }

        public IReadOnlyList<GridTrackSize> GetGridLayoutColTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutRowTemplate(IReadOnlyList<GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state);
        }

        public IReadOnlyList<GridTrackSize> GetGridLayoutRowTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutColAutoSize(GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state);
        }

        public GridTrackSize GetGridLayoutMainAxisAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTrackSize;
        }
        
        public void SetGridLayoutRowAutoSize(GridTrackSize value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state);
        }

        public GridTrackSize GetGridLayoutCrossAxisAutoSize(StyleState state) {
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
        
        public void SetGridLayoutColAlignment(GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state);
        }

        public GridAxisAlignment GetGridLayoutColAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetGridLayoutRowAlignment(GridAxisAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state);
        }

        public GridAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
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
        
        public void SetRadialLayoutRadius(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state);
        }

        public UIFixedLength GetRadialLayoutRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength;
        }
        
        public void SetAlignmentTargetX(AlignmentTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state);
        }

        public AlignmentTarget GetAlignmentTargetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentTargetY(AlignmentTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state);
        }

        public AlignmentTarget GetAlignmentTargetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentBehaviorX(AlignmentBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorX, (int)value), state);
        }

        public AlignmentBehavior GetAlignmentBehaviorX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentBehaviorX, state).AsAlignmentBehavior;
        }
        
        public void SetAlignmentBehaviorY(AlignmentBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBehaviorY, (int)value), state);
        }

        public AlignmentBehavior GetAlignmentBehaviorY(StyleState state) {
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
        
        public void SetAlignmentOffsetX(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state);
        }

        public UIFixedLength GetAlignmentOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsUIFixedLength;
        }
        
        public void SetAlignmentOffsetY(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state);
        }

        public UIFixedLength GetAlignmentOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsUIFixedLength;
        }
        
        public void SetFitX(Fit value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitX, (int)value), state);
        }

        public Fit GetFitX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitX, state).AsFit;
        }
        
        public void SetFitY(Fit value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitY, (int)value), state);
        }

        public Fit GetFitY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitY, state).AsFit;
        }
        
        public void SetMinWidth(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state);
        }

        public UIMeasurement GetMinWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement;
        }
        
        public void SetMaxWidth(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state);
        }

        public UIMeasurement GetMaxWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement;
        }
        
        public void SetPreferredWidth(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state);
        }

        public UIMeasurement GetPreferredWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement;
        }
        
        public void SetMinHeight(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state);
        }

        public UIMeasurement GetMinHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement;
        }
        
        public void SetMaxHeight(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state);
        }

        public UIMeasurement GetMaxHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement;
        }
        
        public void SetPreferredHeight(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state);
        }

        public UIMeasurement GetPreferredHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement;
        }
        
        public void SetMarginTop(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state);
        }

        public UIMeasurement GetMarginTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement;
        }
        
        public void SetMarginRight(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state);
        }

        public UIMeasurement GetMarginRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement;
        }
        
        public void SetMarginBottom(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state);
        }

        public UIMeasurement GetMarginBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement;
        }
        
        public void SetMarginLeft(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state);
        }

        public UIMeasurement GetMarginLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement;
        }
        
        public void SetBorderColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColor, value), state);
        }

        public Color GetBorderColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColor, state).AsColor;
        }
        
        public void SetBorderTop(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state);
        }

        public UIFixedLength GetBorderTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength;
        }
        
        public void SetBorderRight(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state);
        }

        public UIFixedLength GetBorderRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength;
        }
        
        public void SetBorderBottom(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state);
        }

        public UIFixedLength GetBorderBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength;
        }
        
        public void SetBorderLeft(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state);
        }

        public UIFixedLength GetBorderLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopLeft(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state);
        }

        public UIFixedLength GetBorderRadiusTopLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopRight(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state);
        }

        public UIFixedLength GetBorderRadiusTopRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomRight(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state);
        }

        public UIFixedLength GetBorderRadiusBottomRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomLeft(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state);
        }

        public UIFixedLength GetBorderRadiusBottomLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength;
        }
        
        public void SetPaddingTop(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state);
        }

        public UIFixedLength GetPaddingTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength;
        }
        
        public void SetPaddingRight(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state);
        }

        public UIFixedLength GetPaddingRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength;
        }
        
        public void SetPaddingBottom(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state);
        }

        public UIFixedLength GetPaddingBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength;
        }
        
        public void SetPaddingLeft(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state);
        }

        public UIFixedLength GetPaddingLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength;
        }
        
        public void SetTextColor(in Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state);
        }

        public Color GetTextColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor;
        }
        
        public void SetTextFontAsset(FontAsset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state);
        }

        public FontAsset GetTextFontAsset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont;
        }
        
        public void SetTextFontSize(in UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state);
        }

        public UIFixedLength GetTextFontSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength;
        }
        
        public void SetTextFontStyle(FontStyle value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state);
        }

        public FontStyle GetTextFontStyle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle;
        }
        
        public void SetTextAlignment(TextAlignment value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state);
        }

        public TextAlignment GetTextAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment;
        }
        
        public void SetTextOutlineWidth(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state);
        }

        public float GetTextOutlineWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat;
        }
        
        public void SetTextOutlineColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state);
        }

        public Color GetTextOutlineColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor;
        }
        
        public void SetTextOutlineSoftness(float value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state);
        }

        public float GetTextOutlineSoftness(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat;
        }
        
        public void SetTextGlowColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state);
        }

        public Color GetTextGlowColor(StyleState state) {
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
        
        public void SetTextUnderlayColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state);
        }

        public Color GetTextUnderlayColor(StyleState state) {
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
        
        public void SetTextUnderlayType(UnderlayType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state);
        }

        public UnderlayType GetTextUnderlayType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType;
        }
        
        public void SetTextTransform(TextTransform value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state);
        }

        public TextTransform GetTextTransform(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform;
        }
        
        public void SetTextWhitespaceMode(WhitespaceMode value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state);
        }

        public WhitespaceMode GetTextWhitespaceMode(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode;
        }
        
        public void SetAnchorTop(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorTop, value), state);
        }

        public UIFixedLength GetAnchorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTop, state).AsUIFixedLength;
        }
        
        public void SetAnchorRight(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorRight, value), state);
        }

        public UIFixedLength GetAnchorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorRight, state).AsUIFixedLength;
        }
        
        public void SetAnchorBottom(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorBottom, value), state);
        }

        public UIFixedLength GetAnchorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorBottom, state).AsUIFixedLength;
        }
        
        public void SetAnchorLeft(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorLeft, value), state);
        }

        public UIFixedLength GetAnchorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorLeft, state).AsUIFixedLength;
        }
        
        public void SetAnchorTarget(AnchorTarget value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AnchorTarget, (int)value), state);
        }

        public AnchorTarget GetAnchorTarget(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AnchorTarget, state).AsAnchorTarget;
        }
        
        public void SetTransformPositionX(TransformOffset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state);
        }

        public TransformOffset GetTransformPositionX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsTransformOffset;
        }
        
        public void SetTransformPositionY(TransformOffset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state);
        }

        public TransformOffset GetTransformPositionY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsTransformOffset;
        }
        
        public void SetTransformPivotX(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state);
        }

        public UIFixedLength GetTransformPivotX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength;
        }
        
        public void SetTransformPivotY(UIFixedLength value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state);
        }

        public UIFixedLength GetTransformPivotY(StyleState state) {
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
        
        public void SetTransformBehaviorX(TransformBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorX, (int)value), state);
        }

        public TransformBehavior GetTransformBehaviorX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsTransformBehavior;
        }
        
        public void SetTransformBehaviorY(TransformBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformBehaviorY, (int)value), state);
        }

        public TransformBehavior GetTransformBehaviorY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformBehaviorY, state).AsTransformBehavior;
        }
        
        public void SetLayoutType(LayoutType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state);
        }

        public LayoutType GetLayoutType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType;
        }
        
        public void SetLayoutBehavior(LayoutBehavior value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state);
        }

        public LayoutBehavior GetLayoutBehavior(StyleState state) {
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
        
        public void SetRenderLayer(RenderLayer value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state);
        }

        public RenderLayer GetRenderLayer(StyleState state) {
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
        
        public void SetScrollbarSize(UIMeasurement value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarSize, value), state);
        }

        public UIMeasurement GetScrollbarSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarSize, state).AsUIMeasurement;
        }
        
        public void SetScrollbarColor(Color value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollbarColor, value), state);
        }

        public Color GetScrollbarColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollbarColor, state).AsColor;
        }
        
        public void SetShadowType(UnderlayType value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowType, (int)value), state);
        }

        public UnderlayType GetShadowType(StyleState state) {
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
				case StylePropertyId.FlexLayoutDirection:
					 return new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)FlexLayoutDirection);
				case StylePropertyId.FlexLayoutWrap:
					 return new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)FlexLayoutWrap);
				case StylePropertyId.FlexLayoutMainAxisAlignment:
					 return new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)FlexLayoutMainAxisAlignment);
				case StylePropertyId.FlexLayoutCrossAxisAlignment:
					 return new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)FlexLayoutCrossAxisAlignment);
				case StylePropertyId.GridItemX:
					 return new StyleProperty(StylePropertyId.GridItemX, GridItemX);
				case StylePropertyId.GridItemY:
					 return new StyleProperty(StylePropertyId.GridItemY, GridItemY);
				case StylePropertyId.GridItemWidth:
					 return new StyleProperty(StylePropertyId.GridItemWidth, GridItemWidth);
				case StylePropertyId.GridItemHeight:
					 return new StyleProperty(StylePropertyId.GridItemHeight, GridItemHeight);
				case StylePropertyId.GridLayoutDirection:
					 return new StyleProperty(StylePropertyId.GridLayoutDirection, (int)GridLayoutDirection);
				case StylePropertyId.GridLayoutDensity:
					 return new StyleProperty(StylePropertyId.GridLayoutDensity, (int)GridLayoutDensity);
				case StylePropertyId.GridLayoutColTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutColTemplate, GridLayoutColTemplate);
				case StylePropertyId.GridLayoutRowTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, GridLayoutRowTemplate);
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
				default: throw new ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
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