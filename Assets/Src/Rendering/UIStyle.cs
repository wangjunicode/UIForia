using System;
using System.Collections.Generic;
using System.Configuration;
using Src;
using UnityEngine;
using System.Diagnostics;
using Shapes2D;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using TMPro;

namespace Rendering {

    public class BaseUIStyle : UIStyle {

        //BaseStyleBinding[] bindings;
        internal void AddBinding(StylePropertyId propertyId, Binding binding) { }

    }

    [DebuggerDisplay("{nameof(Id)}")]
    public class UIStyle {

        [Flags]
        public enum TextPropertyIdFlag {

            TextColor = 1 << 0,
            TextFontAsset = 1 << 1,
            TextFontSize = 1 << 2,
            TextFontStyle = 1 << 3,
            TextAnchor = 1 << 4,
            TextWhitespaceMode = 1 << 5,
            TextWrapMode = 1 << 6,
            TextHorizontalOverflow = 1 << 7,
            TextVerticalOverflow = 1 << 8,
            TextIndentFirstLine = 1 << 9,
            TextIndentNewLine = 1 << 10,
            TextLayoutStyle = 1 << 11,
            TextAutoSize = 1 << 12,
            TextTransform = 1 << 13

        }

        private static int NextStyleId;
        private List<StyleProperty> m_StyleProperties;
        private TextPropertyIdFlag m_DefinedTextProperties;

        // todo -- instead of every style having its own list, use a single large IntMap
        // todo    that is keyed by BitUtil.SetHighLowBits((int)propertyId, Id);
        private static readonly IntMap<StyleProperty> s_PropertyMap = new IntMap<StyleProperty>();

        public int Id { get; set; } = NextStyleId++;

        public UIStyle() {
            m_StyleProperties = ListPool<StyleProperty>.Get();
            m_DefinedTextProperties = 0;
        }

        public UIStyle(UIStyle toCopy) {
            m_StyleProperties.AddRange(toCopy.m_StyleProperties);
            m_DefinedTextProperties = toCopy.m_DefinedTextProperties;
        }

        public IReadOnlyList<StyleProperty> Properties => m_StyleProperties;

#region Overflow Properties

        public Overflow OverflowX {
            get { return (Overflow) FindEnumProperty(StylePropertyId.OverflowX); }
            set { SetEnumProperty(StylePropertyId.OverflowX, (int) value); }
        }

        public Overflow OverflowY {
            get { return (Overflow) FindEnumProperty(StylePropertyId.OverflowY); }
            set { SetEnumProperty(StylePropertyId.OverflowY, (int) value); }
        }

#endregion

#region Paint Properties

        public Color BackgroundColor {
            get { return FindColorProperty(StylePropertyId.BackgroundColor); }
            set { SetColorProperty(StylePropertyId.BackgroundColor, value); }
        }

        public Color BorderColor {
            get { return FindColorProperty(StylePropertyId.BorderColor); }
            set { SetColorProperty(StylePropertyId.BorderColor, value); }
        }

        public Texture2D BackgroundImage {
            get {
                StyleProperty property = FindProperty(StylePropertyId.BackgroundImage);
                return property.AsTexture;
            }
            set { SetObjectProperty(StylePropertyId.BackgroundImage, value); }
        }

        public BackgroundFillType BackgroundFillType {
            get { return (BackgroundFillType) FindIntProperty(StylePropertyId.BackgroundFillType); }
            set { SetIntProperty(StylePropertyId.BackgroundFillType, (int) value); }
        }
        
        public BackgroundShapeType BackgroundShapeType {
            get { return (BackgroundShapeType) FindIntProperty(StylePropertyId.BackgroundShapeType); }
            set { SetIntProperty(StylePropertyId.BackgroundShapeType, (int) value); }
        }
        
        public GradientType BackgroundGradientType {
            get { return (GradientType) FindIntProperty(StylePropertyId.BackgroundGradientType); }
            set { SetIntProperty(StylePropertyId.BackgroundGradientType, (int) value); }
        }

        public GradientAxis BackgroundGradientAxis {
            get { return (GradientAxis) FindIntProperty(StylePropertyId.BackgroundGradientAxis); }
            set { SetIntProperty(StylePropertyId.BackgroundGradientAxis, (int) value); }
        }
        
        public float BackgroundGradientStart {
            get { return FindFloatProperty(StylePropertyId.BackgroundGradientStart); }
            set { SetFloatProperty(StylePropertyId.BackgroundGradientStart,  value); }
        }

        public float BackgroundGridSize {
            get { return FindFloatProperty(StylePropertyId.BackgroundGridSize); }
            set { SetFloatProperty(StylePropertyId.BackgroundGridSize,  value); }
        }

        public float BackgroundLineSize {
            get { return FindFloatProperty(StylePropertyId.BackgroundLineSize); }
            set { SetFloatProperty(StylePropertyId.BackgroundLineSize,  value); }
        }
        
        public float BackgroundFillOffsetX {
            get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetX); }
            set { SetFloatProperty(StylePropertyId.BackgroundFillOffsetX,  value); }
        }
        
        public float BackgroundFillOffsetY {
            get { return FindFloatProperty(StylePropertyId.BackgroundFillOffsetY); }
            set { SetFloatProperty(StylePropertyId.BackgroundFillOffsetY,  value); }
        }
        
        public float BackgroundFillScaleX {
            get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleX); }
            set { SetFloatProperty(StylePropertyId.BackgroundFillScaleX,  value); }
        }
        
        public float BackgroundFillScaleY {
            get { return FindFloatProperty(StylePropertyId.BackgroundFillScaleY); }
            set { SetFloatProperty(StylePropertyId.BackgroundFillScaleY,  value); }
        }
        
#endregion

#region Grid Item Properties

        public int GridItemColStart {
            get { return FindIntProperty(StylePropertyId.GridItemColStart); }
            set { SetIntProperty(StylePropertyId.GridItemColStart, value); }
        }

        public int GridItemColSpan {
            get { return FindIntProperty(StylePropertyId.GridItemColSpan); }
            set { SetIntProperty(StylePropertyId.GridItemColSpan, value); }
        }

        public int GridItemRowStart {
            get { return FindIntProperty(StylePropertyId.GridItemRowStart); }
            set { SetIntProperty(StylePropertyId.GridItemRowStart, value); }
        }

        public int GridItemRowSpan {
            get { return FindIntProperty(StylePropertyId.GridItemRowSpan); }
            set { SetIntProperty(StylePropertyId.GridItemRowSpan, value); }
        }

        public CrossAxisAlignment GridItemColSelfAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.GridItemColSelfAlignment); }
            set { SetEnumProperty(StylePropertyId.GridItemColSelfAlignment, (int) value); }
        }

        public CrossAxisAlignment GridItemRowSelfAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.GridItemRowSelfAlignment); }
            set { SetEnumProperty(StylePropertyId.GridItemRowSelfAlignment, (int) value); }
        }

#endregion

#region Grid Layout Properties

        public LayoutDirection GridLayoutDirection {
            get { return (LayoutDirection) FindEnumProperty(StylePropertyId.GridLayoutDirection); }
            set { SetEnumProperty(StylePropertyId.GridLayoutDirection, (int) value); }
        }

        public GridLayoutDensity GridLayoutPlacementDensity {
            get { return (GridLayoutDensity) FindEnumProperty(StylePropertyId.GridLayoutDensity); }
            set { SetEnumProperty(StylePropertyId.GridLayoutDensity, (int) value); }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
            get {
                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == StylePropertyId.GridLayoutColTemplate) {
                        return m_StyleProperties[i].AsGridTrackTemplate;
                    }
                }

                return ListPool<GridTrackSize>.Empty;
            }
            set {
                if (value == null) {
                    RemoveProperty(StylePropertyId.GridLayoutColTemplate);
                    return;
                }

                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == StylePropertyId.GridLayoutColTemplate) {
                        m_StyleProperties[i] = new StyleProperty(StylePropertyId.GridLayoutColTemplate, (int) LayoutDirection.Column, 0, value);
                        return;
                    }
                }

                m_StyleProperties.Add(new StyleProperty(StylePropertyId.GridLayoutColTemplate, (int) LayoutDirection.Column, 0, value));
            }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
            get {
                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == StylePropertyId.GridLayoutRowTemplate) {
                        return m_StyleProperties[i].AsGridTrackTemplate;
                    }
                }

                return ListPool<GridTrackSize>.Empty;
            }
            set {
                if (value == null) {
                    RemoveProperty(StylePropertyId.GridLayoutRowTemplate);
                    return;
                }

                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == StylePropertyId.GridLayoutRowTemplate) {
                        m_StyleProperties[i] = new StyleProperty(StylePropertyId.GridLayoutRowTemplate, (int) LayoutDirection.Row, 0, value);
                        return;
                    }
                }

                m_StyleProperties.Add(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, (int) LayoutDirection.Row, 0, value));
            }
        }

        public GridTrackSize GridLayoutColAutoSize {
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize); }
            set { SetGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize, value); }
        }

        public GridTrackSize GridLayoutRowAutoSize {
            get { return FindGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize); }
            set { SetGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize, value); }
        }

        public float GridLayoutColGapSize {
            get { return FindFloatProperty(StylePropertyId.GridLayoutColGap); }
            set { SetFloatProperty(StylePropertyId.GridLayoutColGap, value); }
        }

        public float GridLayoutRowGapSize {
            get { return FindFloatProperty(StylePropertyId.GridLayoutRowGap); }
            set { SetFloatProperty(StylePropertyId.GridLayoutRowGap, value); }
        }

        public CrossAxisAlignment GridLayoutColAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.GridLayoutColAlignment); }
            set { SetEnumProperty(StylePropertyId.GridLayoutColAlignment, (int) value); }
        }

        public CrossAxisAlignment GridLayoutRowAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.GridLayoutRowAlignment); }
            set { SetEnumProperty(StylePropertyId.GridLayoutRowAlignment, (int) value); }
        }

#endregion

#region Layout Type and Behavior

        public LayoutType LayoutType {
            get { return (LayoutType) FindEnumProperty(StylePropertyId.LayoutType); }
            set { SetEnumProperty(StylePropertyId.LayoutType, (int) value); }
        }

        public LayoutBehavior LayoutBehavior {
            get { return (LayoutBehavior) FindEnumProperty(StylePropertyId.LayoutBehavior); }
            set { SetEnumProperty(StylePropertyId.LayoutBehavior, (int) value); }
        }

#endregion

#region Flex Layout Properties

        public LayoutWrap FlexLayoutWrap {
            get { return (LayoutWrap) FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
            set { SetEnumProperty(StylePropertyId.FlexLayoutWrap, (int) value); }
        }

        public LayoutDirection FlexLayoutDirection {
            get { return (LayoutDirection) FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
            set { SetEnumProperty(StylePropertyId.FlexLayoutDirection, (int) value); }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            get { return (MainAxisAlignment) FindEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment); }
            set { SetEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int) value); }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment); }
            set { SetEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int) value); }
        }

#endregion

#region Flex Item Properties

        public int FlexItemGrowthFactor {
            get { return FindIntProperty(StylePropertyId.FlexItemGrow); }
            set { SetIntProperty(StylePropertyId.FlexItemGrow, value); }
        }

        public int FlexItemShrinkFactor {
            get { return FindIntProperty(StylePropertyId.FlexItemShrink); }
            set { SetIntProperty(StylePropertyId.FlexItemShrink, value); }
        }

        public int FlexItemOrder {
            get { return FindIntProperty(StylePropertyId.FlexItemOrder); }
            set { SetIntProperty(StylePropertyId.FlexItemOrder, value); }
        }

        public CrossAxisAlignment FlexItemSelfAlign {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.FlexItemSelfAlignment); }
            set { SetEnumProperty(StylePropertyId.FlexItemSelfAlignment, (int) value); }
        }

#endregion

#region Transform Properties

        public UIFixedLength TransformPositionX {
            get { return GetUIFixedLengthProperty(StylePropertyId.TransformPositionX); }
            set { SetUIFixedLengthProperty(StylePropertyId.TransformPositionX, value); }
        }

        public UIFixedLength TransformPositionY {
            get { return GetUIFixedLengthProperty(StylePropertyId.TransformPositionY); }
            set { SetUIFixedLengthProperty(StylePropertyId.TransformPositionY, value); }
        }

        public float TransformScaleX {
            get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
            set { SetFloatProperty(StylePropertyId.TransformScaleX, value); }
        }

        public float TransformScaleY {
            get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
            set { SetFloatProperty(StylePropertyId.TransformScaleY, value); }
        }

        public UIFixedLength TransformPivotX {
            get { return GetUIFixedLengthProperty(StylePropertyId.TransformPivotX); }
            set { SetUIFixedLengthProperty(StylePropertyId.TransformPivotX, value); }
        }

        public UIFixedLength TransformPivotY {
            get { return GetUIFixedLengthProperty(StylePropertyId.TransformPivotY); }
            set { SetUIFixedLengthProperty(StylePropertyId.TransformPivotY, value); }
        }

        public float TransformRotation {
            get { return FindFloatProperty(StylePropertyId.TransformRotation); }
            set { SetFloatProperty(StylePropertyId.TransformRotation, value); }
        }

        public TransformBehavior TransformBehaviorX {
            get { return (TransformBehavior) FindEnumProperty(StylePropertyId.TransformBehaviorX); }
            set { SetEnumProperty(StylePropertyId.TransformBehaviorX, (int) value); }
        }

        public TransformBehavior TransformBehaviorY {
            get { return (TransformBehavior) FindEnumProperty(StylePropertyId.TransformBehaviorY); }
            set { SetEnumProperty(StylePropertyId.TransformBehaviorY, (int) value); }
        }

#endregion

#region Size Properties

        public UIMeasurement PreferredWidth {
            get { return GetUIMeasurementProperty(StylePropertyId.PreferredWidth); }
            set { SetUIMeasurementProperty(StylePropertyId.PreferredWidth, value); }
        }

        public UIMeasurement MinWidth {
            get { return GetUIMeasurementProperty(StylePropertyId.MinWidth); }
            set { SetUIMeasurementProperty(StylePropertyId.MinWidth, value); }
        }

        public UIMeasurement MaxWidth {
            get { return GetUIMeasurementProperty(StylePropertyId.MaxWidth); }
            set { SetUIMeasurementProperty(StylePropertyId.MaxWidth, value); }
        }

        public UIMeasurement PreferredHeight {
            get { return GetUIMeasurementProperty(StylePropertyId.PreferredHeight); }
            set { SetUIMeasurementProperty(StylePropertyId.PreferredHeight, value); }
        }

        public UIMeasurement MinHeight {
            get { return GetUIMeasurementProperty(StylePropertyId.MinHeight); }
            set { SetUIMeasurementProperty(StylePropertyId.MinHeight, value); }
        }

        public UIMeasurement MaxHeight {
            get { return GetUIMeasurementProperty(StylePropertyId.MaxHeight); }
            set { SetUIMeasurementProperty(StylePropertyId.MaxHeight, value); }
        }

#endregion

#region Padding Properties

        public FixedLengthRect Padding {
            get { return new FixedLengthRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft); }
            set {
                PaddingTop = value.top;
                PaddingRight = value.right;
                PaddingBottom = value.bottom;
                PaddingLeft = value.left;
            }
        }

        public UIFixedLength PaddingTop {
            get { return GetUIFixedLengthProperty(StylePropertyId.PaddingTop); }
            set { SetUIFixedLengthProperty(StylePropertyId.PaddingTop, value); }
        }

        public UIFixedLength PaddingRight {
            get { return GetUIFixedLengthProperty(StylePropertyId.PaddingRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.PaddingRight, value); }
        }

        public UIFixedLength PaddingBottom {
            get { return GetUIFixedLengthProperty(StylePropertyId.PaddingBottom); }
            set { SetUIFixedLengthProperty(StylePropertyId.PaddingBottom, value); }
        }

        public UIFixedLength PaddingLeft {
            get { return GetUIFixedLengthProperty(StylePropertyId.PaddingLeft); }
            set { SetUIFixedLengthProperty(StylePropertyId.PaddingLeft, value); }
        }

#endregion

#region Margin Properties

        public ContentBoxRect Margin {
            get { return new ContentBoxRect(MarginTop, MarginRight, MarginBottom, MarginLeft); }
            set {
                MarginTop = value.top;
                MarginRight = value.right;
                MarginBottom = value.bottom;
                MarginLeft = value.left;
            }
        }

        public UIMeasurement MarginTop {
            get { return GetUIMeasurementProperty(StylePropertyId.MarginTop); }
            set { SetUIMeasurementProperty(StylePropertyId.MarginTop, value); }
        }

        public UIMeasurement MarginRight {
            get { return GetUIMeasurementProperty(StylePropertyId.MarginRight); }
            set { SetUIMeasurementProperty(StylePropertyId.MarginRight, value); }
        }

        public UIMeasurement MarginBottom {
            get { return GetUIMeasurementProperty(StylePropertyId.MarginBottom); }
            set { SetUIMeasurementProperty(StylePropertyId.MarginBottom, value); }
        }

        public UIMeasurement MarginLeft {
            get { return GetUIMeasurementProperty(StylePropertyId.MarginLeft); }
            set { SetUIMeasurementProperty(StylePropertyId.MarginLeft, value); }
        }

#endregion

#region BorderProperties

        public FixedLengthRect Border {
            get { return new FixedLengthRect(BorderTop, BorderRight, BorderBottom, BorderLeft); }
            set {
                BorderTop = value.top;
                BorderRight = value.right;
                BorderBottom = value.bottom;
                BorderLeft = value.left;
            }
        }

        public UIFixedLength BorderTop {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderTop); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderTop, value); }
        }

        public UIFixedLength BorderRight {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderRight, value); }
        }

        public UIFixedLength BorderBottom {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderBottom); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderBottom, value); }
        }

        public UIFixedLength BorderLeft {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderLeft); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderLeft, value); }
        }

        public UIFixedLength BorderRadiusTopRight {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIFixedLength BorderRadiusTopLeft {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIFixedLength BorderRadiusBottomLeft {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIFixedLength BorderRadiusBottomRight {
            get { return GetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

#endregion

#region Text Properties

        public Color TextColor {
            get { return FindColorProperty(StylePropertyId.TextColor); }
            set { SetColorProperty(StylePropertyId.TextColor, value); }
        }

        public TextUtil.TextTransform TextTransform {
            get { return (TextUtil.TextTransform) FindEnumProperty(StylePropertyId.TextTransform); }
            set { SetEnumProperty(StylePropertyId.TextTransform, (int) value); }
        }

        public int FontSize {
            get { return FindProperty(StylePropertyId.TextFontSize).valuePart0; }
            set { SetIntProperty(StylePropertyId.TextFontSize, value); }
        }

        public TMP_FontAsset FontAsset {
            get {
                StyleProperty property = FindProperty(StylePropertyId.TextFontAsset);
                return property.IsDefined
                    ? property.AsFont
                    : null;
            }
            set { SetObjectProperty(StylePropertyId.TextFontAsset, value); }
        }

        public TextUtil.FontStyle FontStyle {
            get { return (TextUtil.FontStyle) FindEnumProperty(StylePropertyId.TextFontStyle); }
            set { SetEnumProperty(StylePropertyId.TextFontStyle, (int) value); }
        }

        public TextUtil.TextAlignment TextAlignment {
            get { return (TextUtil.TextAlignment) FindEnumProperty(StylePropertyId.TextAnchor); }
            set { SetEnumProperty(StylePropertyId.TextAnchor, (int) value); }
        }

        public WhitespaceMode WhitespaceMode {
            get { return (WhitespaceMode) FindEnumProperty(StylePropertyId.TextWhitespaceMode); }
            set { SetEnumProperty(StylePropertyId.TextWhitespaceMode, (int) value); }
        }

        // todo -- wrap mode
        // todo -- overflow mode h & v
        // todo -- layout style
        // todo -- auto sizing

        public float TextIndentFirstLine {
            get { return FindFloatProperty(StylePropertyId.TextIndentFirstLine); }
            set { SetFloatProperty(StylePropertyId.TextIndentFirstLine, value); }
        }

        public float TextIndentNewLine {
            get { return FindFloatProperty(StylePropertyId.TextIndentNewLine); }
            set { SetFloatProperty(StylePropertyId.TextIndentNewLine, value); }
        }

        public BorderRadius BorderRadius {
            set {
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft, value.topLeft);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value.topRight);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft, value.bottomLeft);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight, value.bottomRight);
            }
        }

#endregion

#region Anchor Properties

        public UIFixedLength AnchorTop {
            get { return GetUIFixedLengthProperty(StylePropertyId.AnchorTop); }
            set { SetUIFixedLengthProperty(StylePropertyId.AnchorTop, value); }
        }

        public UIFixedLength AnchorRight {
            get { return GetUIFixedLengthProperty(StylePropertyId.AnchorRight); }
            set { SetUIFixedLengthProperty(StylePropertyId.AnchorRight, value); }
        }

        public UIFixedLength AnchorBottom {
            get { return GetUIFixedLengthProperty(StylePropertyId.AnchorBottom); }
            set { SetUIFixedLengthProperty(StylePropertyId.AnchorBottom, value); }
        }

        public UIFixedLength AnchorLeft {
            get { return GetUIFixedLengthProperty(StylePropertyId.AnchorLeft); }
            set { SetUIFixedLengthProperty(StylePropertyId.AnchorLeft, value); }
        }

        public AnchorTarget AnchorTarget {
            get { return (AnchorTarget) FindEnumProperty(StylePropertyId.AnchorTarget); }
            set { SetEnumProperty(StylePropertyId.AnchorTarget, (int) value); }
        }

#endregion

#region Layer Properties

        public int LayerOffset {
            get { return FindIntProperty(StylePropertyId.RenderLayerOffset); }
            set { SetIntProperty(StylePropertyId.RenderLayerOffset, value); }
        }

        public int ZIndex {
            get { return FindIntProperty(StylePropertyId.ZIndex); }
            set { SetIntProperty(StylePropertyId.ZIndex, value); }
        }

        public RenderLayer RenderLayer {
            get { return (RenderLayer) FindEnumProperty(StylePropertyId.RenderLayer); }
            set { SetIntProperty(StylePropertyId.RenderLayer, (int) value); }
        }

#endregion

#region Methods

        internal void OnSpawn() {
            m_StyleProperties = ListPool<StyleProperty>.Get();
            m_DefinedTextProperties = 0;
        }

        internal void OnDestroy() {
            ListPool<StyleProperty>.Release(ref m_StyleProperties);
        }

        public bool DefinesProperty(StylePropertyId propertyId) {
//            const int TextPropertyStart = (int) StylePropertyId.__TextPropertyStart__;
//            const int TextPropertyEnd = (int) StylePropertyId.__TextPropertyEnd__;
//
//            int intPropertyId = (int) propertyId;
//            if (intPropertyId > TextPropertyStart && intPropertyId < TextPropertyEnd) {
//                TextPropertyIdFlag flags = (TextPropertyIdFlag) (intPropertyId - TextPropertyStart);
//                return (m_DefinedTextProperties & flags) != 0;
//            }

            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) return true;
            }

            return false;
        }

        private GridTrackSize FindGridTrackSizeProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new GridTrackSize(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (GridTemplateUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return GridTrackSize.Unset;
        }

        private UIMeasurement GetUIMeasurementProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIMeasurement(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (UIMeasurementUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIMeasurement.Unset;
        }

        private UIFixedLength GetUIFixedLengthProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIFixedLength(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (UIFixedUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIFixedLength.Unset;
        }

        private void RemoveProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    m_StyleProperties.RemoveAt(i);
                    return;
                }
            }
        }

        internal StyleProperty FindProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return m_StyleProperties[i];
                }
            }

            return new StyleProperty(propertyId, int.MaxValue, int.MaxValue);
        }

        private float FindFloatProperty(StylePropertyId propertyId) {
            StyleProperty property = FindProperty(propertyId);
            return property.IsDefined ? FloatUtil.DecodeToFloat(property.valuePart0) : FloatUtil.UnsetValue;
        }

        private int FindIntProperty(StylePropertyId propertyId) {
            StyleProperty property = FindProperty(propertyId);
            return property.IsDefined ? property.valuePart0 : IntUtil.UnsetValue;
        }

        private int FindEnumProperty(StylePropertyId propertyId) {
            StyleProperty property = FindProperty(propertyId);
            return property.IsDefined ? property.valuePart0 : 0;
        }

        private Color FindColorProperty(StylePropertyId propertyId) {
            StyleProperty property = FindProperty(propertyId);
            return property.IsDefined ? (Color) new StyleColor(property.valuePart0) : ColorUtil.UnsetValue;
        }

        internal void SetUIMeasurementProperty(StylePropertyId propertyId, UIMeasurement measurement) {
            if (measurement.unit == UIMeasurementUnit.Unset || !FloatUtil.IsDefined(measurement.value)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(measurement.value), (int) measurement.unit);
            }
        }

        internal void SetGridTrackSizeProperty(StylePropertyId propertyId, GridTrackSize size) {
            if (size.minUnit == GridTemplateUnit.Unset || !FloatUtil.IsDefined(size.minValue)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(size.minValue), (int) size.minUnit);
            }
        }

        internal void SetObjectProperty(StylePropertyId propertyId, object value) {
            if (value == null) {
                RemoveProperty(propertyId);
            }
            else {
                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == propertyId) {
                        m_StyleProperties[i] = new StyleProperty(propertyId, 0, 0, value);
                        return;
                    }
                }

                m_StyleProperties.Add(new StyleProperty(propertyId, 0, 0, value));
            }
        }

        internal void SetUIFixedLengthProperty(StylePropertyId propertyId, UIFixedLength length) {
            if (length.unit == UIFixedUnit.Unset || !FloatUtil.IsDefined(length.value)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(length.value), (int) length.unit);
            }
        }

        internal void SetProperty(StylePropertyId propertyId, int value0, int value1 = 0) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    m_StyleProperties[i] = new StyleProperty(propertyId, value0, value1);
                    return;
                }
            }

            m_StyleProperties.Add(new StyleProperty(propertyId, value0, value1));
        }

        internal void SetIntProperty(StylePropertyId propertyId, int value) {
            if (!IntUtil.IsDefined(value)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, value);
        }

        internal void SetFloatProperty(StylePropertyId propertyId, float value) {
            if (!FloatUtil.IsDefined(value)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, FloatUtil.EncodeToInt(value));
        }

        internal void SetEnumProperty(StylePropertyId propertyId, int value0) {
            if (value0 == 0 || !IntUtil.IsDefined(value0)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, value0);
        }

        internal void SetColorProperty(StylePropertyId propertyId, Color color) {
            if (!ColorUtil.IsDefined(color)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, new StyleColor(color).rgba);
        }

#endregion

    }

}