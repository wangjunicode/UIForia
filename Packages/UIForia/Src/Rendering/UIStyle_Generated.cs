//using System.Collections.Generic;
//using System.Diagnostics;
//using Shapes2D;
//using UIForia;
//using UIForia.Layout;
//using UIForia.Layout.LayoutTypes;
//using UIForia.Rendering;
//using UIForia.Text;
//using TMPro;
//using UIForia.Util;
//using UnityEngine;
//using FontStyle = UIForia.Text.FontStyle;
//using TextAlignment = UIForia.Text.TextAlignment;
//
//namespace UIForia.Rendering {
//
//public partial class UIStyle {		public Overflow OverflowX {
//			[DebuggerStepThrough] get { return (Overflow)FindEnumProperty(StylePropertyId.OverflowX); }
//		set { SetEnumProperty(StylePropertyId.OverflowX, (int)value); } 
//		}
//
//		public Overflow OverflowY {
//			[DebuggerStepThrough] get { return (Overflow)FindEnumProperty(StylePropertyId.OverflowY); }
//		set { SetEnumProperty(StylePropertyId.OverflowY, (int)value); } 
//		}
//
//		public Color BorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.BorderColor); }
//		set { SetColorProperty(StylePropertyId.BorderColor, value); } 
//		}
//
//		public Color BackgroundColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.BackgroundColor); }
//		set { SetColorProperty(StylePropertyId.BackgroundColor, value); } 
//		}
//
//		public Color BackgroundColorSecondary {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.BackgroundColorSecondary); }
//		set { SetColorProperty(StylePropertyId.BackgroundColorSecondary, value); } 
//		}
//
//		public Texture2D BackgroundImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.BackgroundImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.BackgroundImage, value); } 
//		}
//
//		public Texture2D BackgroundImage1 {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.BackgroundImage1).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.BackgroundImage1, value); } 
//		}
//
//		public Texture2D BackgroundImage2 {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.BackgroundImage2).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.BackgroundImage2, value); } 
//		}
//
//		public GradientType BackgroundGradientType {
//			[DebuggerStepThrough] get { return (GradientType)FindEnumProperty(StylePropertyId.BackgroundGradientType); }
//		set { SetEnumProperty(StylePropertyId.BackgroundGradientType, (int)value); } 
//		}
//
//		public GradientAxis BackgroundGradientAxis {
//			[DebuggerStepThrough] get { return (GradientAxis)FindEnumProperty(StylePropertyId.BackgroundGradientAxis); }
//		set { SetEnumProperty(StylePropertyId.BackgroundGradientAxis, (int)value); } 
//		}
//
//		public float BackgroundGradientStart {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundGradientStart); }
//		set { SetFloatProperty(StylePropertyId.BackgroundGradientStart, value); } 
//		}
//
//		public float BackgroundFillRotation {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundFillRotation); }
//		set { SetFloatProperty(StylePropertyId.BackgroundFillRotation, value); } 
//		}
//
//		public BackgroundFillType BackgroundFillType {
//			[DebuggerStepThrough] get { return (BackgroundFillType)FindEnumProperty(StylePropertyId.BackgroundFillType); }
//		set { SetEnumProperty(StylePropertyId.BackgroundFillType, (int)value); } 
//		}
//
//		public BackgroundShapeType BackgroundShapeType {
//			[DebuggerStepThrough] get { return (BackgroundShapeType)FindEnumProperty(StylePropertyId.BackgroundShapeType); }
//		set { SetEnumProperty(StylePropertyId.BackgroundShapeType, (int)value); } 
//		}
//
//		public float BackgroundFillOffsetX {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetX); }
//		set { SetFloatProperty(StylePropertyId.BackgroundFillOffsetX, value); } 
//		}
//
//		public float BackgroundFillOffsetY {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetY); }
//		set { SetFloatProperty(StylePropertyId.BackgroundFillOffsetY, value); } 
//		}
//
//		public float BackgroundFillScaleX {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleX); }
//		set { SetFloatProperty(StylePropertyId.BackgroundFillScaleX, value); } 
//		}
//
//		public float BackgroundFillScaleY {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleY); }
//		set { SetFloatProperty(StylePropertyId.BackgroundFillScaleY, value); } 
//		}
//
//		public float Opacity {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.Opacity); }
//		set { SetFloatProperty(StylePropertyId.Opacity, value); } 
//		}
//
//		public Texture2D Cursor {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.Cursor).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.Cursor, value); } 
//		}
//
//		public int FlexItemOrder {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.FlexItemOrder); }
//		set { SetIntProperty(StylePropertyId.FlexItemOrder, value); } 
//		}
//
//		public int FlexItemGrow {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.FlexItemGrow); }
//		set { SetIntProperty(StylePropertyId.FlexItemGrow, value); } 
//		}
//
//		public int FlexItemShrink {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.FlexItemShrink); }
//		set { SetIntProperty(StylePropertyId.FlexItemShrink, value); } 
//		}
//
//		public CrossAxisAlignment FlexItemSelfAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.FlexItemSelfAlignment); }
//		set { SetEnumProperty(StylePropertyId.FlexItemSelfAlignment, (int)value); } 
//		}
//
//		public LayoutDirection FlexLayoutDirection {
//			[DebuggerStepThrough] get { return (LayoutDirection)FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
//		set { SetEnumProperty(StylePropertyId.FlexLayoutDirection, (int)value); } 
//		}
//
//		public LayoutWrap FlexLayoutWrap {
//			[DebuggerStepThrough] get { return (LayoutWrap)FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
//		set { SetEnumProperty(StylePropertyId.FlexLayoutWrap, (int)value); } 
//		}
//
//		public MainAxisAlignment FlexLayoutMainAxisAlignment {
//			[DebuggerStepThrough] get { return (MainAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment); }
//		set { SetEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int)value); } 
//		}
//
//		public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment); }
//		set { SetEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int)value); } 
//		}
//
//		public int GridItemColStart {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.GridItemColStart); }
//		set { SetIntProperty(StylePropertyId.GridItemColStart, value); } 
//		}
//
//		public int GridItemColSpan {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.GridItemColSpan); }
//		set { SetIntProperty(StylePropertyId.GridItemColSpan, value); } 
//		}
//
//		public int GridItemRowStart {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.GridItemRowStart); }
//		set { SetIntProperty(StylePropertyId.GridItemRowStart, value); } 
//		}
//
//		public int GridItemRowSpan {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.GridItemRowSpan); }
//		set { SetIntProperty(StylePropertyId.GridItemRowSpan, value); } 
//		}
//
//		public CrossAxisAlignment GridItemColSelfAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.GridItemColSelfAlignment); }
//		set { SetEnumProperty(StylePropertyId.GridItemColSelfAlignment, (int)value); } 
//		}
//
//		public CrossAxisAlignment GridItemRowSelfAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.GridItemRowSelfAlignment); }
//		set { SetEnumProperty(StylePropertyId.GridItemRowSelfAlignment, (int)value); } 
//		}
//
//		public LayoutDirection GridLayoutDirection {
//			[DebuggerStepThrough] get { return (LayoutDirection)FindEnumProperty(StylePropertyId.GridLayoutDirection); }
//		set { SetEnumProperty(StylePropertyId.GridLayoutDirection, (int)value); } 
//		}
//
//		public GridLayoutDensity GridLayoutDensity {
//			[DebuggerStepThrough] get { return (GridLayoutDensity)FindEnumProperty(StylePropertyId.GridLayoutDensity); }
//		set { SetEnumProperty(StylePropertyId.GridLayoutDensity, (int)value); } 
//		}
//
//		public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.GridLayoutColTemplate).AsGridTemplate; }
//		set { SetObjectProperty(StylePropertyId.GridLayoutColTemplate, value); } 
//		}
//
//		public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.GridLayoutRowTemplate).AsGridTemplate; }
//		set { SetObjectProperty(StylePropertyId.GridLayoutRowTemplate, value); } 
//		}
//
//		public GridTrackSize GridLayoutColAutoSize {
//			[DebuggerStepThrough] get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize); }
//		set { SetGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize, value); } 
//		}
//
//		public GridTrackSize GridLayoutRowAutoSize {
//			[DebuggerStepThrough] get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize); }
//		set { SetGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize, value); } 
//		}
//
//		public float GridLayoutColGap {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.GridLayoutColGap); }
//		set { SetFloatProperty(StylePropertyId.GridLayoutColGap, value); } 
//		}
//
//		public float GridLayoutRowGap {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.GridLayoutRowGap); }
//		set { SetFloatProperty(StylePropertyId.GridLayoutRowGap, value); } 
//		}
//
//		public CrossAxisAlignment GridLayoutColAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutColAlignment); }
//		set { SetEnumProperty(StylePropertyId.GridLayoutColAlignment, (int)value); } 
//		}
//
//		public CrossAxisAlignment GridLayoutRowAlignment {
//			[DebuggerStepThrough] get { return (CrossAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutRowAlignment); }
//		set { SetEnumProperty(StylePropertyId.GridLayoutRowAlignment, (int)value); } 
//		}
//
//		public UIMeasurement MinWidth {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MinWidth); }
//		set { SetUIMeasurementProperty(StylePropertyId.MinWidth, value); } 
//		}
//
//		public UIMeasurement MaxWidth {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MaxWidth); }
//		set { SetUIMeasurementProperty(StylePropertyId.MaxWidth, value); } 
//		}
//
//		public UIMeasurement PreferredWidth {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.PreferredWidth); }
//		set { SetUIMeasurementProperty(StylePropertyId.PreferredWidth, value); } 
//		}
//
//		public UIMeasurement MinHeight {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MinHeight); }
//		set { SetUIMeasurementProperty(StylePropertyId.MinHeight, value); } 
//		}
//
//		public UIMeasurement MaxHeight {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MaxHeight); }
//		set { SetUIMeasurementProperty(StylePropertyId.MaxHeight, value); } 
//		}
//
//		public UIMeasurement PreferredHeight {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.PreferredHeight); }
//		set { SetUIMeasurementProperty(StylePropertyId.PreferredHeight, value); } 
//		}
//
//		public UIMeasurement MarginTop {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MarginTop); }
//		set { SetUIMeasurementProperty(StylePropertyId.MarginTop, value); } 
//		}
//
//		public UIMeasurement MarginRight {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MarginRight); }
//		set { SetUIMeasurementProperty(StylePropertyId.MarginRight, value); } 
//		}
//
//		public UIMeasurement MarginBottom {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MarginBottom); }
//		set { SetUIMeasurementProperty(StylePropertyId.MarginBottom, value); } 
//		}
//
//		public UIMeasurement MarginLeft {
//			[DebuggerStepThrough] get { return FindUIMeasurementProperty(StylePropertyId.MarginLeft); }
//		set { SetUIMeasurementProperty(StylePropertyId.MarginLeft, value); } 
//		}
//
//		public UIFixedLength BorderTop {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderTop); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderTop, value); } 
//		}
//
//		public UIFixedLength BorderRight {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderRight); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderRight, value); } 
//		}
//
//		public UIFixedLength BorderBottom {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderBottom); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderBottom, value); } 
//		}
//
//		public UIFixedLength BorderLeft {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderLeft); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderLeft, value); } 
//		}
//
//		public UIFixedLength BorderRadiusTopLeft {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft, value); } 
//		}
//
//		public UIFixedLength BorderRadiusTopRight {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value); } 
//		}
//
//		public UIFixedLength BorderRadiusBottomRight {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight, value); } 
//		}
//
//		public UIFixedLength BorderRadiusBottomLeft {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft); }
//		set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft, value); } 
//		}
//
//		public UIFixedLength PaddingTop {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.PaddingTop); }
//		set { SetUIFixedLengthProperty(StylePropertyId.PaddingTop, value); } 
//		}
//
//		public UIFixedLength PaddingRight {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.PaddingRight); }
//		set { SetUIFixedLengthProperty(StylePropertyId.PaddingRight, value); } 
//		}
//
//		public UIFixedLength PaddingBottom {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.PaddingBottom); }
//		set { SetUIFixedLengthProperty(StylePropertyId.PaddingBottom, value); } 
//		}
//
//		public UIFixedLength PaddingLeft {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.PaddingLeft); }
//		set { SetUIFixedLengthProperty(StylePropertyId.PaddingLeft, value); } 
//		}
//
//		public Color TextColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.TextColor); }
//		set { SetColorProperty(StylePropertyId.TextColor, value); } 
//		}
//
//		public TMP_FontAsset TextFontAsset {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.TextFontAsset).AsFont; }
//		set { SetObjectProperty(StylePropertyId.TextFontAsset, value); } 
//		}
//
//		public int TextFontSize {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.TextFontSize); }
//		set { SetIntProperty(StylePropertyId.TextFontSize, value); } 
//		}
//
//		public FontStyle TextFontStyle {
//			[DebuggerStepThrough] get { return (FontStyle)FindEnumProperty(StylePropertyId.TextFontStyle); }
//		set { SetEnumProperty(StylePropertyId.TextFontStyle, (int)value); } 
//		}
//
//		public TextAlignment TextAlignment {
//			[DebuggerStepThrough] get { return (TextAlignment)FindEnumProperty(StylePropertyId.TextAlignment); }
//		set { SetEnumProperty(StylePropertyId.TextAlignment, (int)value); } 
//		}
//
//		public TextTransform TextTransform {
//			[DebuggerStepThrough] get { return (TextTransform)FindEnumProperty(StylePropertyId.TextTransform); }
//		set { SetEnumProperty(StylePropertyId.TextTransform, (int)value); } 
//		}
//
//		public UIFixedLength AnchorTop {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.AnchorTop); }
//		set { SetUIFixedLengthProperty(StylePropertyId.AnchorTop, value); } 
//		}
//
//		public UIFixedLength AnchorRight {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.AnchorRight); }
//		set { SetUIFixedLengthProperty(StylePropertyId.AnchorRight, value); } 
//		}
//
//		public UIFixedLength AnchorBottom {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.AnchorBottom); }
//		set { SetUIFixedLengthProperty(StylePropertyId.AnchorBottom, value); } 
//		}
//
//		public UIFixedLength AnchorLeft {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.AnchorLeft); }
//		set { SetUIFixedLengthProperty(StylePropertyId.AnchorLeft, value); } 
//		}
//
//		public AnchorTarget AnchorTarget {
//			[DebuggerStepThrough] get { return (AnchorTarget)FindEnumProperty(StylePropertyId.AnchorTarget); }
//		set { SetEnumProperty(StylePropertyId.AnchorTarget, (int)value); } 
//		}
//
//		public UIFixedLength TransformPositionX {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.TransformPositionX); }
//		set { SetUIFixedLengthProperty(StylePropertyId.TransformPositionX, value); } 
//		}
//
//		public UIFixedLength TransformPositionY {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.TransformPositionY); }
//		set { SetUIFixedLengthProperty(StylePropertyId.TransformPositionY, value); } 
//		}
//
//		public UIFixedLength TransformPivotX {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotX); }
//		set { SetUIFixedLengthProperty(StylePropertyId.TransformPivotX, value); } 
//		}
//
//		public UIFixedLength TransformPivotY {
//			[DebuggerStepThrough] get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotY); }
//		set { SetUIFixedLengthProperty(StylePropertyId.TransformPivotY, value); } 
//		}
//
//		public float TransformScaleX {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
//		set { SetFloatProperty(StylePropertyId.TransformScaleX, value); } 
//		}
//
//		public float TransformScaleY {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
//		set { SetFloatProperty(StylePropertyId.TransformScaleY, value); } 
//		}
//
//		public float TransformRotation {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.TransformRotation); }
//		set { SetFloatProperty(StylePropertyId.TransformRotation, value); } 
//		}
//
//		public TransformBehavior TransformBehaviorX {
//			[DebuggerStepThrough] get { return (TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorX); }
//		set { SetEnumProperty(StylePropertyId.TransformBehaviorX, (int)value); } 
//		}
//
//		public TransformBehavior TransformBehaviorY {
//			[DebuggerStepThrough] get { return (TransformBehavior)FindEnumProperty(StylePropertyId.TransformBehaviorY); }
//		set { SetEnumProperty(StylePropertyId.TransformBehaviorY, (int)value); } 
//		}
//
//		public LayoutType LayoutType {
//			[DebuggerStepThrough] get { return (LayoutType)FindEnumProperty(StylePropertyId.LayoutType); }
//		set { SetEnumProperty(StylePropertyId.LayoutType, (int)value); } 
//		}
//
//		public LayoutBehavior LayoutBehavior {
//			[DebuggerStepThrough] get { return (LayoutBehavior)FindEnumProperty(StylePropertyId.LayoutBehavior); }
//		set { SetEnumProperty(StylePropertyId.LayoutBehavior, (int)value); } 
//		}
//
//		public int ZIndex {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.ZIndex); }
//		set { SetIntProperty(StylePropertyId.ZIndex, value); } 
//		}
//
//		public int RenderLayerOffset {
//			[DebuggerStepThrough] get { return FindIntProperty(StylePropertyId.RenderLayerOffset); }
//		set { SetIntProperty(StylePropertyId.RenderLayerOffset, value); } 
//		}
//
//		public RenderLayer RenderLayer {
//			[DebuggerStepThrough] get { return (RenderLayer)FindEnumProperty(StylePropertyId.RenderLayer); }
//		set { SetEnumProperty(StylePropertyId.RenderLayer, (int)value); } 
//		}
//
//		public VerticalScrollbarAttachment ScrollbarVerticalAttachment {
//			[DebuggerStepThrough] get { return (VerticalScrollbarAttachment)FindEnumProperty(StylePropertyId.ScrollbarVerticalAttachment); }
//		set { SetEnumProperty(StylePropertyId.ScrollbarVerticalAttachment, (int)value); } 
//		}
//
//		public ScrollbarButtonPlacement ScrollbarVerticalButtonPlacement {
//			[DebuggerStepThrough] get { return (ScrollbarButtonPlacement)FindEnumProperty(StylePropertyId.ScrollbarVerticalButtonPlacement); }
//		set { SetEnumProperty(StylePropertyId.ScrollbarVerticalButtonPlacement, (int)value); } 
//		}
//
//		public float ScrollbarVerticalTrackSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalTrackSize, value); } 
//		}
//
//		public float ScrollbarVerticalTrackBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderRadius, value); } 
//		}
//
//		public float ScrollbarVerticalTrackBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalTrackBorderSize, value); } 
//		}
//
//		public Color ScrollbarVerticalTrackBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalTrackBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarVerticalTrackImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarVerticalTrackImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarVerticalTrackImage, value); } 
//		}
//
//		public Color ScrollbarVerticalTrackColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalTrackColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalTrackColor, value); } 
//		}
//
//		public float ScrollbarVerticalHandleSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalHandleSize, value); } 
//		}
//
//		public float ScrollbarVerticalHandleBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderRadius, value); } 
//		}
//
//		public float ScrollbarVerticalHandleBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalHandleBorderSize, value); } 
//		}
//
//		public Color ScrollbarVerticalHandleBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalHandleBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarVerticalHandleImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarVerticalHandleImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarVerticalHandleImage, value); } 
//		}
//
//		public Color ScrollbarVerticalHandleColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalHandleColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalHandleColor, value); } 
//		}
//
//		public float ScrollbarVerticalIncrementSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalIncrementSize, value); } 
//		}
//
//		public float ScrollbarVerticalIncrementBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderRadius, value); } 
//		}
//
//		public float ScrollbarVerticalIncrementBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalIncrementBorderSize, value); } 
//		}
//
//		public Color ScrollbarVerticalIncrementBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalIncrementBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarVerticalIncrementImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarVerticalIncrementImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarVerticalIncrementImage, value); } 
//		}
//
//		public Color ScrollbarVerticalIncrementColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalIncrementColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalIncrementColor, value); } 
//		}
//
//		public float ScrollbarVerticalDecrementSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalDecrementSize, value); } 
//		}
//
//		public float ScrollbarVerticalDecrementBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderRadius, value); } 
//		}
//
//		public float ScrollbarVerticalDecrementBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarVerticalDecrementBorderSize, value); } 
//		}
//
//		public Color ScrollbarVerticalDecrementBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalDecrementBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarVerticalDecrementImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarVerticalDecrementImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarVerticalDecrementImage, value); } 
//		}
//
//		public Color ScrollbarVerticalDecrementColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarVerticalDecrementColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarVerticalDecrementColor, value); } 
//		}
//
//		public HorizontalScrollbarAttachment ScrollbarHorizontalAttachment {
//			[DebuggerStepThrough] get { return (HorizontalScrollbarAttachment)FindEnumProperty(StylePropertyId.ScrollbarHorizontalAttachment); }
//		set { SetEnumProperty(StylePropertyId.ScrollbarHorizontalAttachment, (int)value); } 
//		}
//
//		public ScrollbarButtonPlacement ScrollbarHorizontalButtonPlacement {
//			[DebuggerStepThrough] get { return (ScrollbarButtonPlacement)FindEnumProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement); }
//		set { SetEnumProperty(StylePropertyId.ScrollbarHorizontalButtonPlacement, (int)value); } 
//		}
//
//		public float ScrollbarHorizontalTrackSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalTrackSize, value); } 
//		}
//
//		public float ScrollbarHorizontalTrackBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderRadius, value); } 
//		}
//
//		public float ScrollbarHorizontalTrackBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalTrackBorderSize, value); } 
//		}
//
//		public Color ScrollbarHorizontalTrackBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalTrackBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarHorizontalTrackImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarHorizontalTrackImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarHorizontalTrackImage, value); } 
//		}
//
//		public Color ScrollbarHorizontalTrackColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalTrackColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalTrackColor, value); } 
//		}
//
//		public float ScrollbarHorizontalHandleSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalHandleSize, value); } 
//		}
//
//		public float ScrollbarHorizontalHandleBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderRadius, value); } 
//		}
//
//		public float ScrollbarHorizontalHandleBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalHandleBorderSize, value); } 
//		}
//
//		public Color ScrollbarHorizontalHandleBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalHandleBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarHorizontalHandleImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarHorizontalHandleImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarHorizontalHandleImage, value); } 
//		}
//
//		public Color ScrollbarHorizontalHandleColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalHandleColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalHandleColor, value); } 
//		}
//
//		public float ScrollbarHorizontalIncrementSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementSize, value); } 
//		}
//
//		public float ScrollbarHorizontalIncrementBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderRadius, value); } 
//		}
//
//		public float ScrollbarHorizontalIncrementBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderSize, value); } 
//		}
//
//		public Color ScrollbarHorizontalIncrementBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalIncrementBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarHorizontalIncrementImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarHorizontalIncrementImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarHorizontalIncrementImage, value); } 
//		}
//
//		public Color ScrollbarHorizontalIncrementColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalIncrementColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalIncrementColor, value); } 
//		}
//
//		public float ScrollbarHorizontalDecrementSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementSize, value); } 
//		}
//
//		public float ScrollbarHorizontalDecrementBorderRadius {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderRadius, value); } 
//		}
//
//		public float ScrollbarHorizontalDecrementBorderSize {
//			[DebuggerStepThrough] get { return FindFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize); }
//		set { SetFloatProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderSize, value); } 
//		}
//
//		public Color ScrollbarHorizontalDecrementBorderColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalDecrementBorderColor, value); } 
//		}
//
//		public Texture2D ScrollbarHorizontalDecrementImage {
//			[DebuggerStepThrough] get { return GetProperty(StylePropertyId.ScrollbarHorizontalDecrementImage).AsTexture2D; }
//		set { SetObjectProperty(StylePropertyId.ScrollbarHorizontalDecrementImage, value); } 
//		}
//
//		public Color ScrollbarHorizontalDecrementColor {
//			[DebuggerStepThrough] get { return FindColorProperty(StylePropertyId.ScrollbarHorizontalDecrementColor); }
//		set { SetColorProperty(StylePropertyId.ScrollbarHorizontalDecrementColor, value); } 
//		}
//
//}}