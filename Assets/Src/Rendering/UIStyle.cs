using System;
using System.Collections.Generic;
using System.Configuration;
using Src;
using UnityEngine;
using System.Diagnostics;
using Src.Layout;
using Src.Rendering;
using Src.Util;
using TMPro;

namespace Rendering {

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
            TextAutoSize = 1 << 12

        }

        private static int NextStyleId;

        private string m_Id;
        private List<StyleProperty> m_StyleProperties;
        private TextPropertyIdFlag m_DefinedTextProperties;

        public UIStyle(string id = null) {
            m_Id = id ?? "Style: " + (NextStyleId++);
            m_StyleProperties = ListPool<StyleProperty>.Get();
            m_DefinedTextProperties = 0;
        }

        public UIStyle(UIStyle toCopy) {
            m_Id = "Copy Style: " + (NextStyleId++);
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

        public AssetPointer<Texture2D> BackgroundImage {
            get {
                StyleProperty property = FindProperty(StylePropertyId.BackgroundImage);
                return new AssetPointer<Texture2D>((AssetType) property.valuePart0, property.valuePart1);
            }
            set { SetProperty(StylePropertyId.BackgroundImage, (int) value.assetType, value.id); }
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

#endregion

#region Grid Layout Properties

        public LayoutDirection GridLayoutFlowDirection {
            get { return (LayoutDirection) FindEnumProperty(StylePropertyId.GridFlowDirection); }
            set { SetEnumProperty(StylePropertyId.GridFlowDirection, (int) value); }
        }

        public GridAutoPlaceDensity GridLayoutPlacementDensity {
            get { return (GridAutoPlaceDensity) FindEnumProperty(StylePropertyId.GridPlacementDensity); }
            set { SetEnumProperty(StylePropertyId.GridPlacementDensity, (int) value); }
        }

        public GridTrackSizer GridLayoutColTemplate {
            get { return default(GridTrackSizer); }
            set { throw new NotImplementedException(); }
        }

        public GridTrackSizer GridLayoutRowTemplate {
            get { return default(GridTrackSizer); }
            set { throw new NotImplementedException(); }
        }

        public GridTrackSizeFn GridLayoutAutoColSize {
            get { return default(GridTrackSizeFn); }
            set { throw new NotImplementedException(); }
        }

        public GridTrackSizeFn GridLayoutAutoRowSize {
            get { return default(GridTrackSizeFn); }
            set { throw new NotImplementedException(); }
        }

        public float GridLayoutColGapSize {
            get { return FindFloatProperty(StylePropertyId.GridColGap); }
            set { SetFloatProperty(StylePropertyId.GridColGap, value); }
        }

        public float GridLayoutRowGapSize {
            get { return FindFloatProperty(StylePropertyId.GridRowGap); }
            set { SetFloatProperty(StylePropertyId.GridRowGap, value); }
        }

#endregion

#region Shared Layout Properties

        public LayoutType LayoutType {
            get { return (LayoutType) FindEnumProperty(StylePropertyId.LayoutType); }
            set { SetEnumProperty(StylePropertyId.LayoutType, (int) value); }
        }

        public LayoutFlowType LayoutInFow {
            get { return (LayoutFlowType) FindProperty(StylePropertyId.IsInLayoutFlow).valuePart0; }
            set { SetProperty(StylePropertyId.IsInLayoutFlow, (int) value); }
        }

#endregion

#region Flex Layout Properties

        public LayoutWrap FlexLayoutWrap {
            get { return (LayoutWrap) FindEnumProperty(StylePropertyId.FlexWrap); }
            set { SetEnumProperty(StylePropertyId.FlexWrap, (int) value); }
        }

        public LayoutDirection FlexLayoutDirection {
            get { return (LayoutDirection) FindEnumProperty(StylePropertyId.FlexDirection); }
            set { SetEnumProperty(StylePropertyId.FlexDirection, (int) value); }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            get { return (MainAxisAlignment) FindEnumProperty(StylePropertyId.FlexMainAxisAlignment); }
            set { SetEnumProperty(StylePropertyId.FlexMainAxisAlignment, (int) value); }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            get { return (CrossAxisAlignment) FindEnumProperty(StylePropertyId.FlexCrossAxisAlignment); }
            set { SetEnumProperty(StylePropertyId.FlexCrossAxisAlignment, (int) value); }
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

        public UIMeasurement TransformPositionX {
            get { return GetUIMeasurementProperty(StylePropertyId.TransformPositionX); }
            set { SetUIMeasurementProperty(StylePropertyId.TransformPositionX, value); }
        }

        public UIMeasurement TransformPositionY {
            get { return GetUIMeasurementProperty(StylePropertyId.TransformPositionY); }
            set { SetUIMeasurementProperty(StylePropertyId.TransformPositionY, value); }
        }

        public float TransformScaleX {
            get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
            set { SetFloatProperty(StylePropertyId.TransformScaleX, value); }
        }

        public float TransformScaleY {
            get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
            set { SetFloatProperty(StylePropertyId.TransformScaleY, value); }
        }

        public UIMeasurement TransformPivotX {
            get { return GetUIMeasurementProperty(StylePropertyId.TransformPivotX); }
            set { SetUIMeasurementProperty(StylePropertyId.TransformPivotX, value); }
        }

        public UIMeasurement TransformPivotY {
            get { return GetUIMeasurementProperty(StylePropertyId.TransformPivotY); }
            set { SetUIMeasurementProperty(StylePropertyId.TransformPivotY, value); }
        }

        public float TransformRotation {
            get { return FindFloatProperty(StylePropertyId.TransformRotation); }
            set { SetFloatProperty(StylePropertyId.TransformRotation, value); }
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

        public ContentBoxRect Padding {
            get { return new ContentBoxRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft); }
            set {
                PaddingTop = value.top;
                PaddingRight = value.right;
                PaddingBottom = value.bottom;
                PaddingLeft = value.left;
            }
        }

        public UIMeasurement PaddingTop {
            get { return GetUIMeasurementProperty(StylePropertyId.PaddingTop); }
            set { SetUIMeasurementProperty(StylePropertyId.PaddingTop, value); }
        }

        public UIMeasurement PaddingRight {
            get { return GetUIMeasurementProperty(StylePropertyId.PaddingRight); }
            set { SetUIMeasurementProperty(StylePropertyId.PaddingRight, value); }
        }

        public UIMeasurement PaddingBottom {
            get { return GetUIMeasurementProperty(StylePropertyId.PaddingBottom); }
            set { SetUIMeasurementProperty(StylePropertyId.PaddingBottom, value); }
        }

        public UIMeasurement PaddingLeft {
            get { return GetUIMeasurementProperty(StylePropertyId.PaddingLeft); }
            set { SetUIMeasurementProperty(StylePropertyId.PaddingLeft, value); }
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

        public ContentBoxRect Border {
            get { return new ContentBoxRect(BorderTop, BorderRight, BorderBottom, BorderLeft); }
            set {
                BorderTop = value.top;
                BorderRight = value.right;
                BorderBottom = value.bottom;
                BorderLeft = value.left;
            }
        }
        
        public UIMeasurement BorderTop {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderTop); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderTop, value); }
        }

        public UIMeasurement BorderRight {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderRight); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderRight, value); }
        }

        public UIMeasurement BorderBottom {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderBottom); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderBottom, value); }
        }

        public UIMeasurement BorderLeft {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderLeft); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderLeft, value); }
        }

        public UIMeasurement BorderRadiusTopRight {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIMeasurement BorderRadiusTopLeft {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIMeasurement BorderRadiusBottomLeft {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

        public UIMeasurement BorderRadiusBottomRight {
            get { return GetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight); }
            set { SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight, value); }
        }

#endregion

#region Text Properties

        public Color TextColor {
            get { return FindColorProperty(StylePropertyId.TextColor); }
            set { SetColorProperty(StylePropertyId.TextColor, value); }
        }

        public int FontSize {
            get { return FindProperty(StylePropertyId.TextFontSize).valuePart0; }
            set { SetIntProperty(StylePropertyId.TextFontSize, value); }
        }

        public AssetPointer<Font> FontAsset {
            get {
                StyleProperty property = FindProperty(StylePropertyId.TextFontAsset);
                return property.IsDefined
                    ? new AssetPointer<Font>((AssetType) property.valuePart0, property.valuePart1)
                    : new AssetPointer<Font>();
            }
            set { SetProperty(StylePropertyId.TextFontAsset, (int) value.assetType, value.id); }
        }

        public TextUtil.FontStyle FontStyle {
            get { return (TextUtil.FontStyle) FindEnumProperty(StylePropertyId.TextFontStyle); }
            set { SetEnumProperty(StylePropertyId.TextFontStyle, (int) value); }
        }

        public TextUtil.TextAnchor TextAnchor {
            get { return (TextUtil.TextAnchor) FindEnumProperty(StylePropertyId.TextAnchor); }
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

#endregion

#region Methods

        internal void OnSpawn() {
            m_StyleProperties = ListPool<StyleProperty>.Get();
            m_DefinedTextProperties = 0;
        }

        internal void OnDestroy() {
            ListPool<StyleProperty>.Release(m_StyleProperties);
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

        private UIMeasurement GetUIMeasurementProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIMeasurement(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (UIUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIMeasurement.Unset;
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
            if (measurement.unit == UIUnit.Unset || !FloatUtil.IsDefined(measurement.value)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(measurement.value), (int) measurement.unit);
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