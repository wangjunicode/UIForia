using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shapes2D;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Rendering {

    // todo -- have some way of seeing if any properties changed in a given frame
    public class ComputedStyle {

        private readonly UIStyleSet styleSet;
        private readonly IntMap<StyleProperty> properties;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
            this.properties = new IntMap<StyleProperty>();
        }

        public FixedLengthRect border => new FixedLengthRect(BorderTop, BorderRight, BorderBottom, BorderLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, MarginRight, MarginBottom, MarginLeft);

        public FixedLengthRect padding => new FixedLengthRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft);

        public bool HasBorderRadius =>
            BorderRadiusTopLeft.value > 0 ||
            BorderRadiusBottomLeft.value > 0 ||
            BorderRadiusTopRight.value > 0 ||
            BorderRadiusBottomLeft.value > 0;

        #region Paint

        public Color BorderColor {
            [DebuggerStepThrough] get { return ReadColorProperty(StylePropertyId.BorderColor, DefaultStyleValues.BorderColor); }
            internal set { WriteColorProperty(StylePropertyId.BorderColor, value); }
        }

        public Color BackgroundColor {
            [DebuggerStepThrough]
            get {
                return ReadColorProperty(StylePropertyId.BackgroundColor, DefaultStyleValues.BorderColor);
                ;
            }
            internal set { WriteColorProperty(StylePropertyId.BackgroundColor, value); }
        }

        public Color BackgroundColorSecondary {
            [DebuggerStepThrough] get { return ReadColorProperty(StylePropertyId.BackgroundColorSecondary, DefaultStyleValues.BackgroundColorSecondary); }
            internal set { WriteColorProperty(StylePropertyId.BackgroundColorSecondary, value); }
        }

        public Texture2D BackgroundImage {
            [DebuggerStepThrough] get { return (Texture2D) ReadObject(StylePropertyId.BackgroundImage, DefaultStyleValues.BackgroundImage); }
            internal set { WriteObject(StylePropertyId.BackgroundImage, value); }
        }

        public GradientType GradientType {
            get { return (GradientType) ReadInt(StylePropertyId.BackgroundGradientType, (int) DefaultStyleValues.BackgroundGradientType); }
            internal set { WriteInt(StylePropertyId.BackgroundGradientType, (int) value); }
        }

        public GradientAxis GradientAxis {
            get { return (GradientAxis) ReadInt(StylePropertyId.BackgroundGradientAxis, (int) DefaultStyleValues.BackgroundGradientAxis); }
            internal set { WriteInt(StylePropertyId.BackgroundGradientAxis, (int) value); }
        }

        public float GradientStart {
            get { return ReadFloat(StylePropertyId.BackgroundGradientStart, DefaultStyleValues.BackgroundGradientStart); }
            internal set { WriteFloat(StylePropertyId.BackgroundGradientStart, value); }
        }

        public float BackgroundRotation {
            get { return ReadFloat(StylePropertyId.BackgroundFillRotation, DefaultStyleValues.BackgroundFillRotation); }
            internal set { WriteFloat(StylePropertyId.BackgroundFillRotation, value); }
        }

        public BackgroundFillType BackgroundFillType {
            get { return (BackgroundFillType) ReadInt(StylePropertyId.BackgroundFillType, (int) DefaultStyleValues.BackgroundFillType); }
            internal set { WriteInt(StylePropertyId.BackgroundFillType, (int) value); }
        }

        public BackgroundShapeType BackgroundShapeType {
            get { return (BackgroundShapeType) ReadInt(StylePropertyId.BackgroundShapeType, (int) DefaultStyleValues.BackgroundShapeType); }
            internal set { WriteInt(StylePropertyId.BackgroundShapeType, (int) value); }
        }

        public Vector2 BackgroundFillOffset => new Vector2(
            ReadFloat(StylePropertyId.BackgroundFillOffsetX, DefaultStyleValues.BackgroundFillOffsetX),
            ReadFloat(StylePropertyId.BackgroundFillOffsetY, DefaultStyleValues.BackgroundFillOffsetY)
        );

        public float BackgroundFillOffsetX {
            get { return ReadFloat(StylePropertyId.BackgroundFillOffsetX, DefaultStyleValues.BackgroundFillOffsetX); }
            internal set { WriteFloat(StylePropertyId.BackgroundFillOffsetX, value); }
        }

        public float BackgroundFillOffsetY {
            get { return ReadFloat(StylePropertyId.BackgroundFillOffsetY, DefaultStyleValues.BackgroundFillOffsetY); }
            internal set { WriteFloat(StylePropertyId.BackgroundFillOffsetY, value); }
        }

        public Vector2 BackgroundFillScale => new Vector2(
            ReadFloat(StylePropertyId.BackgroundFillScaleX, DefaultStyleValues.BackgroundFillScaleX),
            ReadFloat(StylePropertyId.BackgroundFillScaleY, DefaultStyleValues.BackgroundFillScaleY)
        );

        public float BackgroundFillScaleX {
            get { return ReadFloat(StylePropertyId.BackgroundFillScaleX, DefaultStyleValues.BackgroundFillScaleX); }
            internal set { WriteFloat(StylePropertyId.BackgroundFillScaleX, value); }
        }

        public float BackgroundFillScaleY {
            get { return ReadFloat(StylePropertyId.BackgroundFillScaleY, DefaultStyleValues.BackgroundFillScaleY); }
            internal set { WriteFloat(StylePropertyId.BackgroundFillScaleY, value); }
        }

        #endregion

        #region Overflow

        public Overflow OverflowX {
            [DebuggerStepThrough] get { return (Overflow) ReadInt(StylePropertyId.OverflowX, (int) DefaultStyleValues.OverflowX); }
            internal set { WriteInt(StylePropertyId.OverflowX, (int) value); }
        }

        public Overflow OverflowY {
            [DebuggerStepThrough] get { return (Overflow) ReadInt(StylePropertyId.OverflowY, (int) DefaultStyleValues.OverflowY); }
            internal set { WriteInt(StylePropertyId.OverflowY, (int) value); }
        }

        #endregion

        #region Flex Item 

        public int FlexItemOrder {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.FlexItemOrder, DefaultStyleValues.FlexItemOrder); }
            internal set { WriteInt(StylePropertyId.FlexItemOrder, value); }
        }

        public int FlexItemGrow {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.FlexItemGrow, DefaultStyleValues.FlexItemGrow); }
            internal set { WriteInt(StylePropertyId.FlexItemGrow, value); }
        }

        public int FlexItemShrink {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.FlexItemShrink, DefaultStyleValues.FlexItemShrink); }
            internal set { WriteInt(StylePropertyId.FlexItemShrink, value); }
        }

        public CrossAxisAlignment FlexItemSelfAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.FlexItemSelfAlignment, (int) DefaultStyleValues.FlexItemSelfAlignment); }
            internal set { WriteInt(StylePropertyId.FlexItemSelfAlignment, (int) value); }
        }

        #endregion

        #region Flex Layout

        public LayoutDirection FlexLayoutDirection {
            [DebuggerStepThrough] get { return (LayoutDirection) ReadInt(StylePropertyId.FlexLayoutDirection, (int) DefaultStyleValues.FlexLayoutDirection); }
            internal set { WriteInt(StylePropertyId.FlexLayoutDirection, (int) value); }
        }

        public LayoutWrap FlexLayoutWrap {
            [DebuggerStepThrough] get { return (LayoutWrap) ReadInt(StylePropertyId.FlexLayoutWrap, (int) DefaultStyleValues.FlexLayoutWrap); }
            internal set { WriteInt(StylePropertyId.FlexLayoutWrap, (int) value); }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            [DebuggerStepThrough] get { return (MainAxisAlignment) ReadInt(StylePropertyId.FlexLayoutMainAxisAlignment, (int) DefaultStyleValues.FlexLayoutMainAxisAlignment); }
            internal set { WriteInt(StylePropertyId.FlexLayoutMainAxisAlignment, (int) value); }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.FlexLayoutCrossAxisAlignment, (int) DefaultStyleValues.FlexLayoutCrossAxisAlignment); }
            internal set { WriteInt(StylePropertyId.FlexLayoutCrossAxisAlignment, (int) value); }
        }

        #endregion

        #region Grid Item

        public int GridItemColStart {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.GridItemColStart, DefaultStyleValues.GridItemColStart); }
            internal set { WriteInt(StylePropertyId.GridItemColStart, value); }
        }

        public int GridItemColSpan {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.GridItemColSpan, DefaultStyleValues.GridItemColSpan); }
            internal set { WriteInt(StylePropertyId.GridItemColSpan, value); }
        }

        public int GridItemRowStart {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.GridItemRowStart, DefaultStyleValues.GridItemRowStart); }
            internal set { WriteInt(StylePropertyId.GridItemRowStart, value); }
        }

        public int GridItemRowSpan {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.GridItemRowSpan, DefaultStyleValues.GridItemRowSpan); }
            internal set { WriteInt(StylePropertyId.GridItemRowSpan, value); }
        }

        public CrossAxisAlignment GridItemColSelfAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.GridItemColSelfAlignment, (int) DefaultStyleValues.GridItemColSelfAlignment); }
            internal set { WriteInt(StylePropertyId.GridItemColSelfAlignment, (int) value); }
        }

        public CrossAxisAlignment GridItemRowSelfAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.GridItemRowSelfAlignment, (int) DefaultStyleValues.GridItemRowSelfAlignment); }
            internal set { WriteInt(StylePropertyId.GridItemRowSelfAlignment, (int) value); }
        }

        #endregion

        #region Grid Layout

        public LayoutDirection GridLayoutDirection {
            [DebuggerStepThrough] get { return (LayoutDirection) ReadInt(StylePropertyId.GridLayoutDirection, (int) DefaultStyleValues.GridLayoutDirection); }
            set { WriteInt(StylePropertyId.GridLayoutDirection, (int) value); }
        }

        public GridLayoutDensity GridLayoutDensity {
            [DebuggerStepThrough] get { return (GridLayoutDensity) ReadInt(StylePropertyId.GridLayoutDensity, (int) DefaultStyleValues.GridLayoutDensity); }
            set { WriteInt(StylePropertyId.GridLayoutDensity, (int) value); }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
            [DebuggerStepThrough] get { return (IReadOnlyList<GridTrackSize>) ReadObject(StylePropertyId.GridLayoutColTemplate, DefaultStyleValues.GridLayoutColTemplate); }
            set { WriteObject(StylePropertyId.GridLayoutColTemplate, value); }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
            [DebuggerStepThrough] get { return (IReadOnlyList<GridTrackSize>) ReadObject(StylePropertyId.GridLayoutRowTemplate, DefaultStyleValues.GridLayoutRowTemplate); }
            set { WriteObject(StylePropertyId.GridLayoutRowTemplate, value); }
        }

        public GridTrackSize GridLayoutColAutoSize {
            [DebuggerStepThrough] get { return ReadGridTrackSize(StylePropertyId.GridLayoutColAutoSize, DefaultStyleValues.GridLayoutColAutoSize); }
            set { WriteGridTrackSize(StylePropertyId.GridLayoutColAutoSize, value); }
        }

        public GridTrackSize GridLayoutRowAutoSize {
            [DebuggerStepThrough] get { return ReadGridTrackSize(StylePropertyId.GridLayoutRowAutoSize, DefaultStyleValues.GridLayoutRowAutoSize); }
            set { WriteGridTrackSize(StylePropertyId.GridLayoutRowAutoSize, value); }
        }

        public float GridLayoutColGap {
            [DebuggerStepThrough] get { return ReadFloat(StylePropertyId.GridLayoutColGap, DefaultStyleValues.GridLayoutColGap); }
            set { WriteFloat(StylePropertyId.GridLayoutColGap, value); }
        }

        public float GridLayoutRowGap {
            [DebuggerStepThrough] get { return ReadFloat(StylePropertyId.GridLayoutRowGap, DefaultStyleValues.GridLayoutRowGap); }
            set { WriteFloat(StylePropertyId.GridLayoutRowGap, value); }
        }

        public CrossAxisAlignment GridLayoutColAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.GridLayoutColAlignment, (int) DefaultStyleValues.GridLayoutColAlignment); }
            set { WriteInt(StylePropertyId.GridLayoutColAlignment, (int) value); }
        }

        public CrossAxisAlignment GridLayoutRowAlignment {
            [DebuggerStepThrough] get { return (CrossAxisAlignment) ReadInt(StylePropertyId.GridLayoutRowAlignment, (int) DefaultStyleValues.GridLayoutRowAlignment); }
            set { WriteInt(StylePropertyId.GridLayoutRowAlignment, (int) value); }
        }

        #endregion

        #region Size       

        public UIMeasurement MinWidth {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MinWidth, DefaultStyleValues.MinWidth); }
            internal set { WriteMeasurement(StylePropertyId.MinWidth, value); }
        }

        public UIMeasurement MaxWidth {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MaxWidth, DefaultStyleValues.MaxWidth); }
            internal set { WriteMeasurement(StylePropertyId.MaxWidth, value); }
        }

        public UIMeasurement PreferredWidth {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.PreferredWidth, DefaultStyleValues.PreferredWidth); }
            internal set { WriteMeasurement(StylePropertyId.PreferredWidth, value); }
        }

        public UIMeasurement MinHeight {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MinHeight, DefaultStyleValues.MinHeight); }
            internal set { WriteMeasurement(StylePropertyId.MinHeight, value); }
        }

        public UIMeasurement MaxHeight {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MaxHeight, DefaultStyleValues.MaxHeight); }
            internal set { WriteMeasurement(StylePropertyId.MaxHeight, value); }
        }

        public UIMeasurement PreferredHeight {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.PreferredHeight, DefaultStyleValues.PreferredHeight); }
            internal set { WriteMeasurement(StylePropertyId.PreferredHeight, value); }
        }

        public bool WidthIsParentBased => MinWidth.IsParentBased || MaxWidth.IsParentBased || PreferredWidth.IsParentBased;
        public bool HeightIsParentBased => MinHeight.IsParentBased || MaxHeight.IsParentBased || PreferredHeight.IsParentBased;

        public bool WidthIsContentBased => MinWidth.IsContentBased || MaxWidth.IsContentBased || PreferredWidth.IsContentBased;
        public bool HeightIsContentBased => MinHeight.IsContentBased || MaxHeight.IsContentBased || PreferredHeight.IsContentBased;

        public bool IsWidthFixed => MinWidth.IsFixed && MaxWidth.IsFixed && PreferredWidth.IsFixed;
        public bool IsHeightFixed => MinHeight.IsFixed && MaxHeight.IsFixed && PreferredHeight.IsFixed;

        #endregion

        #region Margin

        public UIMeasurement MarginTop {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MarginTop, DefaultStyleValues.MarginTop); }
            internal set { WriteMeasurement(StylePropertyId.MarginTop, value); }
        }

        public UIMeasurement MarginRight {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MarginRight, DefaultStyleValues.MarginRight); }
            internal set { WriteMeasurement(StylePropertyId.MarginRight, value); }
        }

        public UIMeasurement MarginBottom {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MarginBottom, DefaultStyleValues.MarginBottom); }
            internal set { WriteMeasurement(StylePropertyId.MarginBottom, value); }
        }

        public UIMeasurement MarginLeft {
            [DebuggerStepThrough] get { return ReadMeasurement(StylePropertyId.MarginLeft, DefaultStyleValues.MarginLeft); }
            internal set { WriteMeasurement(StylePropertyId.MarginLeft, value); }
        }

        #endregion

        #region Border

        public UIFixedLength BorderTop {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderTop, DefaultStyleValues.BorderTop); }
            internal set { WriteFixedLength(StylePropertyId.BorderTop, value); }
        }

        public UIFixedLength BorderRight {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderRight, DefaultStyleValues.BorderRight); }
            internal set { WriteFixedLength(StylePropertyId.BorderRight, value); }
        }

        public UIFixedLength BorderBottom {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderBottom, DefaultStyleValues.BorderBottom); }
            internal set { WriteFixedLength(StylePropertyId.BorderBottom, value); }
        }

        public UIFixedLength BorderLeft {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderLeft, DefaultStyleValues.BorderLeft); }
            internal set { WriteFixedLength(StylePropertyId.BorderLeft, value); }
        }

        public BorderRadius BorderRadius => new BorderRadius(BorderRadiusTopLeft, BorderRadiusTopRight, BorderRadiusBottomRight, BorderRadiusBottomLeft);

        public Vector4 ResolvedBorderRadius => new Vector4(
            ResolveHorizontalFixedLength(BorderRadiusTopLeft),
            ResolveHorizontalFixedLength(BorderRadiusTopRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomLeft)
        );

        public Vector4 ResolvedBorder => new Vector4(
            ResolveVerticalFixedLength(BorderTop),
            ResolveHorizontalFixedLength(BorderRight),
            ResolveVerticalFixedLength(BorderBottom),
            ResolveHorizontalFixedLength(BorderLeft)
        );

        // I don't love having this here
        private float ResolveHorizontalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return styleSet.element.layoutResult.AllocatedWidth * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
            }
        }

        // I don't love having this here
        private float ResolveVerticalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return styleSet.element.layoutResult.AllocatedHeight * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
            }
        }

        public UIFixedLength BorderRadiusTopLeft {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderRadiusTopLeft, DefaultStyleValues.BorderRadiusTopLeft); }
            internal set { WriteFixedLength(StylePropertyId.BorderRadiusTopLeft, value); }
        }

        public UIFixedLength BorderRadiusTopRight {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderRadiusTopRight, DefaultStyleValues.BorderRadiusTopRight); }
            internal set { WriteFixedLength(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIFixedLength BorderRadiusBottomRight {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderRadiusBottomRight, DefaultStyleValues.BorderRadiusBottomRight); }
            internal set { WriteFixedLength(StylePropertyId.BorderRadiusBottomRight, value); }
        }

        public UIFixedLength BorderRadiusBottomLeft {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.BorderRadiusBottomLeft, DefaultStyleValues.BorderRadiusBottomLeft); }
            internal set { WriteFixedLength(StylePropertyId.BorderRadiusBottomLeft, value); }
        }

        #endregion

        #region Padding

        public UIFixedLength PaddingTop {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.PaddingTop, DefaultStyleValues.PaddingTop); }
            internal set { WriteFixedLength(StylePropertyId.PaddingTop, value); }
        }

        public UIFixedLength PaddingRight {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.PaddingRight, DefaultStyleValues.PaddingRight); }
            internal set { WriteFixedLength(StylePropertyId.PaddingRight, value); }
        }

        public UIFixedLength PaddingBottom {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.PaddingBottom, DefaultStyleValues.PaddingBottom); }
            internal set { WriteFixedLength(StylePropertyId.PaddingBottom, value); }
        }

        public UIFixedLength PaddingLeft {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.PaddingLeft, DefaultStyleValues.PaddingLeft); }
            internal set { WriteFixedLength(StylePropertyId.PaddingLeft, value); }
        }

        #endregion

        #region Text Properties

        public Color TextColor {
            [DebuggerStepThrough] get { return ReadColorProperty(StylePropertyId.TextColor, DefaultStyleValues.TextColor); }
            internal set { WriteColorProperty(StylePropertyId.TextColor, value); }
        }

        public TMP_FontAsset FontAsset {
            [DebuggerStepThrough] get { return (TMP_FontAsset) ReadObject(StylePropertyId.TextFontAsset, DefaultStyleValues.TextFontAsset); }
            internal set { WriteObject(StylePropertyId.TextFontAsset, value); }
        }

        public int FontSize {
            [DebuggerStepThrough] get { return ReadInt(StylePropertyId.TextFontSize, DefaultStyleValues.TextFontSize); }
            internal set { WriteInt(StylePropertyId.TextFontSize, value); }
        }

        public TextUtil.FontStyle FontStyle {
            [DebuggerStepThrough] get { return (TextUtil.FontStyle) ReadInt(StylePropertyId.TextFontStyle, (int) DefaultStyleValues.TextFontStyle); }
            internal set { WriteInt(StylePropertyId.TextFontStyle, (int) value); }
        }

        public TextUtil.TextAlignment TextAlignment {
            [DebuggerStepThrough] get { return (TextUtil.TextAlignment) ReadInt(StylePropertyId.TextAlignment, (int) DefaultStyleValues.TextAlignment); }
            internal set { WriteInt(StylePropertyId.TextAlignment, (int) value); }
        }

        public TextUtil.TextTransform TextTransform {
            [DebuggerStepThrough] get { return (TextUtil.TextTransform) ReadInt(StylePropertyId.TextTransform, (int) DefaultStyleValues.TextTransform); }
            internal set { WriteInt(StylePropertyId.TextTransform, (int) value); }
        }

        #endregion

        #region Anchor Properties

        public UIFixedLength AnchorTop {
            get { return ReadFixedLength(StylePropertyId.AnchorTop, DefaultStyleValues.AnchorTop); }
            internal set { WriteFixedLength(StylePropertyId.AnchorTop, value); }
        }

        public UIFixedLength AnchorRight {
            get { return ReadFixedLength(StylePropertyId.AnchorRight, DefaultStyleValues.AnchorRight); }
            internal set { WriteFixedLength(StylePropertyId.AnchorRight, value); }
        }

        public UIFixedLength AnchorBottom {
            get { return ReadFixedLength(StylePropertyId.AnchorBottom, DefaultStyleValues.AnchorBottom); }
            internal set { WriteFixedLength(StylePropertyId.AnchorBottom, value); }
        }

        public UIFixedLength AnchorLeft {
            get { return ReadFixedLength(StylePropertyId.AnchorLeft, DefaultStyleValues.AnchorLeft); }
            internal set { WriteFixedLength(StylePropertyId.AnchorLeft, value); }
        }

        public AnchorTarget AnchorTarget {
            get { return (AnchorTarget) ReadInt(StylePropertyId.AnchorTarget, (int) DefaultStyleValues.AnchorTarget); }
            internal set { WriteInt(StylePropertyId.AnchorTarget, (int) value); }
        }

        #endregion

        #region Transform

        public UIFixedLength TransformPositionX {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.TransformPositionX, DefaultStyleValues.TransformPositionX); }
            internal set { WriteFixedLength(StylePropertyId.TransformPositionX, value); }
        }

        public UIFixedLength TransformPositionY {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.TransformPositionY, DefaultStyleValues.TransformPositionY); }
            internal set { WriteFixedLength(StylePropertyId.TransformPositionY, value); }
        }

        public UIFixedLength TransformPivotX {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.TransformPivotX, DefaultStyleValues.TransformPivotX); }
            internal set { WriteFixedLength(StylePropertyId.TransformPivotX, value); }
        }

        public UIFixedLength TransformPivotY {
            [DebuggerStepThrough] get { return ReadFixedLength(StylePropertyId.TransformPivotY, DefaultStyleValues.TransformPivotY); }
            internal set { WriteFixedLength(StylePropertyId.TransformPivotY, value); }
        }

        public float TransformScaleX {
            [DebuggerStepThrough] get { return ReadFloat(StylePropertyId.TransformScaleX, DefaultStyleValues.TransformScaleX); }
            internal set { WriteFloat(StylePropertyId.TransformScaleX, value);}
        }

        public float TransformScaleY {
            [DebuggerStepThrough] get { return ReadFloat(StylePropertyId.TransformScaleY, DefaultStyleValues.TransformScaleY); }
            internal set { WriteFloat(StylePropertyId.TransformScaleY, value);}
        }

        public float TransformRotation {
            [DebuggerStepThrough] get { return ReadFloat(StylePropertyId.TransformRotation, DefaultStyleValues.TransformRotation); }
            internal set { WriteFloat(StylePropertyId.TransformRotation, value);}
        }

        public FixedLengthVector TransformPosition {
            [DebuggerStepThrough] get { return new FixedLengthVector(TransformPositionX, TransformPositionY); }
            internal set {
                TransformPositionX = value.x;
                TransformPositionY = value.y;
            }
        }

        public TransformBehavior TransformBehaviorX {
            [DebuggerStepThrough] get { return (TransformBehavior) ReadInt(StylePropertyId.TransformBehaviorX, (int) DefaultStyleValues.TransformBehaviorX); }
            internal set { WriteInt(StylePropertyId.TransformBehaviorX, (int) value); }
        }

        public TransformBehavior TransformBehaviorY {
            [DebuggerStepThrough] get { return (TransformBehavior) ReadInt(StylePropertyId.TransformBehaviorY, (int) DefaultStyleValues.TransformBehaviorY); }
            internal set { WriteInt(StylePropertyId.TransformBehaviorY, (int) value); }
        }

        #endregion

        #region Layout


        public LayoutType LayoutType {
            [DebuggerStepThrough] get { return (LayoutType)ReadInt(StylePropertyId.LayoutType, (int)DefaultStyleValues.LayoutType); }
            internal set { WriteInt(StylePropertyId.LayoutType, (int)value);}
        }

        public LayoutBehavior LayoutBehavior {
            [DebuggerStepThrough] get { return (LayoutBehavior)ReadInt(StylePropertyId.LayoutBehavior, (int)DefaultStyleValues.LayoutBehavior); }
            internal set { WriteInt(StylePropertyId.LayoutBehavior, (int)value);}
        }

        public float EmSize => FontAsset.fontInfo.PointSize;

        #endregion

        #region Layer

        public int ZIndex {
            get { return ReadInt(StylePropertyId.ZIndex, DefaultStyleValues.ZIndex); }
            internal set { WriteInt(StylePropertyId.ZIndex, value); }
        }

        public RenderLayer RenderLayer {
            get { return (RenderLayer) ReadInt(StylePropertyId.RenderLayer, (int) DefaultStyleValues.RenderLayer); }
            internal set { WriteInt(StylePropertyId.RenderLayer, (int) value); }
        }

        public int LayerOffset {
            get { return ReadInt(StylePropertyId.RenderLayerOffset, DefaultStyleValues.RenderLayerOffset); }
            internal set { WriteInt(StylePropertyId.RenderLayerOffset, value); }
        }

        #endregion

        private void SendEvent(StyleProperty property) {
            styleSet.styleSystem.SetStyleProperty(styleSet.element, property);
        }

        internal void SetProperty(StyleProperty property) {
            int value0 = property.valuePart0;
            int value1 = property.valuePart1;
            switch (property.propertyId) {
                case StylePropertyId.Cursor:
                case StylePropertyId.Opacity:
                    throw new NotImplementedException();

                #region  Layout

                case StylePropertyId.LayoutBehavior:
                    LayoutBehavior = property.IsDefined ? (LayoutBehavior) property.valuePart0 : DefaultStyleValues.LayoutBehavior;
                    break;

                case StylePropertyId.LayoutType:
                    LayoutType = property.IsDefined ? (LayoutType) value0 : DefaultStyleValues.LayoutType;
                    break;

                #endregion

                #region Overflow

                case StylePropertyId.OverflowX:
                    OverflowX = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowX;
                    break;
                case StylePropertyId.OverflowY:
                    OverflowY = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowY;
                    break;

                #endregion

                #region Paint

                case StylePropertyId.BackgroundColor:
                    BackgroundColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.BackgroundColor;
                    break;
                case StylePropertyId.BorderColor:
                    BorderColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.BorderColor;
                    break;
                case StylePropertyId.BackgroundImage:
                    BackgroundImage = property.IsDefined ? property.AsTexture : DefaultStyleValues.BackgroundImage;
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    BorderRadiusTopLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopLeft;
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    BorderRadiusTopRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopRight;
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    BorderRadiusBottomLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomLeft;
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
                    BorderRadiusBottomRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomRight;
                    break;

                case StylePropertyId.BackgroundFillOffsetX:
                    BackgroundFillOffsetX = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillOffsetX;
                    break;
                case StylePropertyId.BackgroundFillOffsetY:
                    BackgroundFillOffsetY = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillOffsetY;
                    break;
                case StylePropertyId.BackgroundFillScaleX:
                    BackgroundFillScaleX = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillScaleX;
                    break;
                case StylePropertyId.BackgroundFillScaleY:
                    BackgroundFillScaleY = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillScaleX;
                    break;

                #endregion

                #region Grid Item

                case StylePropertyId.GridItemColStart:
                    GridItemColStart = property.IsDefined ? value0 : DefaultStyleValues.GridItemColStart;
                    break;
                case StylePropertyId.GridItemColSpan:
                    GridItemColSpan = property.IsDefined ? value0 : DefaultStyleValues.GridItemColSpan;
                    break;
                case StylePropertyId.GridItemRowStart:
                    GridItemRowStart = property.IsDefined ? value0 : DefaultStyleValues.GridItemRowStart;
                    break;
                case StylePropertyId.GridItemRowSpan:
                    GridItemRowSpan = property.IsDefined ? value0 : DefaultStyleValues.GridItemRowSpan;
                    break;
                case StylePropertyId.GridItemColSelfAlignment:
                    GridItemColSelfAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridItemColSelfAlignment;
                    break;
                case StylePropertyId.GridItemRowSelfAlignment:
                    GridItemRowSelfAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridItemRowSelfAlignment;
                    break;

                #endregion

                #region Grid Layout

                case StylePropertyId.GridLayoutDirection:
                    GridLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : DefaultStyleValues.GridLayoutDirection;
                    break;
                case StylePropertyId.GridLayoutDensity:
                    GridLayoutDensity = property.IsDefined ? (GridLayoutDensity) value0 : DefaultStyleValues.GridLayoutDensity;
                    break;
                case StylePropertyId.GridLayoutColTemplate:
                    GridLayoutColTemplate = property.IsDefined ? property.AsGridTrackTemplate : DefaultStyleValues.GridLayoutColTemplate;
                    break;
                case StylePropertyId.GridLayoutRowTemplate:
                    GridLayoutRowTemplate = property.IsDefined ? property.AsGridTrackTemplate : DefaultStyleValues.GridLayoutRowTemplate;
                    break;
                case StylePropertyId.GridLayoutColAutoSize:
                    GridLayoutColAutoSize = property.IsDefined ? property.AsGridTrackSize : DefaultStyleValues.GridLayoutColAutoSize;
                    break;
                case StylePropertyId.GridLayoutRowAutoSize:
                    GridLayoutRowAutoSize = property.IsDefined ? property.AsGridTrackSize : DefaultStyleValues.GridLayoutRowAutoSize;
                    break;
                case StylePropertyId.GridLayoutColGap:
                    GridLayoutColGap = property.IsDefined ? property.AsFloat : DefaultStyleValues.GridLayoutColGap;
                    break;
                case StylePropertyId.GridLayoutRowGap:
                    GridLayoutRowGap = property.IsDefined ? property.AsFloat : DefaultStyleValues.GridLayoutRowGap;
                    break;
                case StylePropertyId.GridLayoutColAlignment:
                    GridLayoutColAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridLayoutColAlignment;
                    break;
                case StylePropertyId.GridLayoutRowAlignment:
                    GridLayoutRowAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridLayoutRowAlignment;
                    break;

                #endregion

                #region Flex Layout

                case StylePropertyId.FlexLayoutWrap:
                    FlexLayoutWrap = property.IsDefined ? (LayoutWrap) value0 : DefaultStyleValues.FlexLayoutWrap;
                    break;
                case StylePropertyId.FlexLayoutDirection:
                    FlexLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : DefaultStyleValues.FlexLayoutDirection;
                    break;
                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    FlexLayoutMainAxisAlignment = property.IsDefined ? (MainAxisAlignment) value0 : DefaultStyleValues.FlexLayoutMainAxisAlignment;
                    break;
                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    FlexLayoutCrossAxisAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.FlexLayoutCrossAxisAlignment;
                    break;

                #endregion

                #region Flex Item

                case StylePropertyId.FlexItemSelfAlignment:
                    FlexItemSelfAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.FlexItemSelfAlignment;
                    break;
                case StylePropertyId.FlexItemOrder:
                    FlexItemOrder = property.IsDefined ? value0 : DefaultStyleValues.FlexItemOrder;
                    break;
                case StylePropertyId.FlexItemGrow:
                    FlexItemGrow = property.IsDefined ? value0 : DefaultStyleValues.FlexItemGrow;
                    break;
                case StylePropertyId.FlexItemShrink:
                    FlexItemShrink = property.IsDefined ? value0 : DefaultStyleValues.FlexItemShrink;
                    break;

                #endregion

                #region Margin

                case StylePropertyId.MarginTop:
                    MarginTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginTop;
                    break;

                case StylePropertyId.MarginRight:
                    MarginRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginRight;
                    break;

                case StylePropertyId.MarginBottom:
                    MarginBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginBottom;
                    break;

                case StylePropertyId.MarginLeft:
                    MarginLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginLeft;
                    break;

                #endregion

                #region Border

                case StylePropertyId.BorderTop:
                    BorderTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderTop;
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRight;
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderBottom;
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderLeft;
                    break;

                #endregion

                #region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingTop;
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingRight;
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingBottom;
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingLeft;
                    break;

                #endregion

                #region Transform

                case StylePropertyId.TransformPositionX:
                    TransformPositionX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPositionX;
                    break;
                case StylePropertyId.TransformPositionY:
                    TransformPositionY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPositionY;
                    break;
                case StylePropertyId.TransformScaleX:
                    TransformScaleX = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformScaleX;
                    break;
                case StylePropertyId.TransformScaleY:
                    TransformScaleY = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformScaleY;
                    break;
                case StylePropertyId.TransformPivotX:
                    TransformPivotX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPivotX;
                    break;
                case StylePropertyId.TransformPivotY:
                    TransformPivotY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPivotY;
                    break;
                case StylePropertyId.TransformRotation:
                    TransformRotation = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformRotation;
                    break;

                case StylePropertyId.TransformBehaviorX:
                    TransformBehaviorX = property.IsDefined ? (TransformBehavior) value0 : DefaultStyleValues.TransformBehaviorX;
                    break;
                case StylePropertyId.TransformBehaviorY:
                    TransformBehaviorY = property.IsDefined ? (TransformBehavior) value0 : DefaultStyleValues.TransformBehaviorY;
                    break;

                #endregion

                #region Text

                case StylePropertyId.TextColor:
                    TextColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.TextColor;
                    break;
                case StylePropertyId.TextFontAsset:
                    FontAsset = property.IsDefined ? property.AsFont : DefaultStyleValues.TextFontAsset;
                    break;
                case StylePropertyId.TextFontSize:
                    FontSize = property.IsDefined ? value0 : DefaultStyleValues.TextFontSize;
                    break;
                case StylePropertyId.TextFontStyle:
                    FontStyle = property.IsDefined ? (TextUtil.FontStyle) value0 : DefaultStyleValues.TextFontStyle;
                    break;
                case StylePropertyId.TextAlignment:
                    TextAlignment = property.IsDefined ? (TextUtil.TextAlignment) value0 : DefaultStyleValues.TextAlignment;
                    break;
                case StylePropertyId.TextTransform:
                    TextTransform = property.IsDefined ? (TextUtil.TextTransform) value0 : DefaultStyleValues.TextTransform;
                    break;
                case StylePropertyId.TextWhitespaceMode:
                    throw new NotImplementedException();

                #endregion

                #region Size

                case StylePropertyId.MinWidth:
                    MinWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MinWidth;
                    break;

                case StylePropertyId.MaxWidth:
                    MaxWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MaxWidth;
                    break;

                case StylePropertyId.PreferredWidth:
                    PreferredWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.PreferredWidth;
                    break;

                case StylePropertyId.MinHeight:
                    MinHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MinHeight;
                    break;

                case StylePropertyId.MaxHeight:
                    MaxHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MaxHeight;
                    break;

                case StylePropertyId.PreferredHeight:
                    PreferredHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.PreferredHeight;
                    break;

                #endregion

                #region Layer

                case StylePropertyId.ZIndex:
                    ZIndex = property.IsDefined ? property.AsInt : DefaultStyleValues.ZIndex;
                    break;
                case StylePropertyId.RenderLayerOffset:
                    LayerOffset = property.IsDefined ? property.AsInt : DefaultStyleValues.RenderLayerOffset;
                    break;
                case StylePropertyId.RenderLayer:
                    RenderLayer = property.IsDefined ? property.AsRenderLayer : DefaultStyleValues.RenderLayer;
                    break;

                #endregion

                #region  Anchors

                case StylePropertyId.AnchorTarget:
                    AnchorTarget = property.IsDefined ? property.AsAnchorTarget : DefaultStyleValues.AnchorTarget;
                    break;
                case StylePropertyId.AnchorTop:
                    AnchorTop = property.IsDefined ? property.AsFixedLength : DefaultStyleValues.AnchorTop;
                    break;
                case StylePropertyId.AnchorRight:
                    AnchorRight = property.IsDefined ? property.AsFixedLength : DefaultStyleValues.AnchorRight;
                    break;
                case StylePropertyId.AnchorBottom:
                    AnchorBottom = property.IsDefined ? property.AsFixedLength : DefaultStyleValues.AnchorBottom;
                    break;
                case StylePropertyId.AnchorLeft:
                    AnchorLeft = property.IsDefined ? property.AsFixedLength : DefaultStyleValues.AnchorLeft;
                    break;

                #endregion

                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyId), property.propertyId, null);
            }
        }

        public bool IsDefined(StylePropertyId propertyId) {
            return properties.ContainsKey((int) propertyId);
        }

        public StyleProperty GetProperty(StylePropertyId propertyId) {
            StyleProperty property;

            if (properties.TryGetValue((int) propertyId, out property)) {
                return property;
            }

            return DefaultStyleValues.GetPropertyValue(propertyId);
        }

        [DebuggerStepThrough]
        private UIFixedLength ReadFixedLength(StylePropertyId propertyId, UIFixedLength defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsFixedLength;
            }

            return defaultValue;
        }

        [DebuggerStepThrough]
        private UIMeasurement ReadMeasurement(StylePropertyId propertyId, UIMeasurement defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsMeasurement;
            }

            return defaultValue;
        }
        
        [DebuggerStepThrough]
        private void WriteMeasurement(StylePropertyId propertyId, UIMeasurement newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsMeasurement == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }
        
        [DebuggerStepThrough]
        private void WriteFixedLength(StylePropertyId propertyId, UIFixedLength newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private int ReadInt(StylePropertyId propertyId, int defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsInt;
            }

            return defaultValue;
        }

        private GridTrackSize ReadGridTrackSize(StylePropertyId propertyId, GridTrackSize defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsGridTrackSize;
            }

            return defaultValue;
        }

        private Color ReadColorProperty(StylePropertyId propertyId, Color defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsColor;
            }

            return defaultValue;
        }

        private object ReadObject(StylePropertyId propertyId, object defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.objectField;
            }

            return defaultValue;
        }

        private void WriteObject(StylePropertyId propertyId, object newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) { // todo -- null?
                if (retn.objectField == newValue) {
                    return;
                }
            }

            StyleProperty property = new StyleProperty(propertyId, 0, 0, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteGridTrackSize(StylePropertyId propertyId, GridTrackSize newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsGridTrackSize == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, FloatUtil.EncodeToInt(newValue.minValue), (int) newValue.minUnit, null);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteColorProperty(StylePropertyId propertyId, Color newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsColor == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteInt(StylePropertyId propertyId, int newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private float ReadFloat(StylePropertyId propertyId, float defaultValue) {
            StyleProperty retn;
            return properties.TryGetValue((int) propertyId, out retn) ? retn.AsFloat : defaultValue;
        }

        private void WriteFloat(StylePropertyId propertyId, float newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retn.AsFloat == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, FloatUtil.EncodeToInt(newValue));
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

    }

}