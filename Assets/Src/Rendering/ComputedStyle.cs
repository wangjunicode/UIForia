using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public class ComputedStyle {

        private UIStyleSet styleSet;
        private RareStyleData rareData;
        private Dictionary<StylePropertyId, StyleProperty> properties;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public RareStyleData RareData => rareData ?? (rareData = new RareStyleData(styleSet));

        public PaddingBox border => new PaddingBox(borderTop, borderRight, borderBottom, borderLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, marginRight, marginBottom, marginLeft);

        public PaddingBox padding => new PaddingBox(paddingTop, paddingRight, paddingBottom, paddingLeft);

        public bool HasBorderRadius =>
            rareData != null
            && (rareData.borderRadiusBottomLeft.IsDefined()
                || rareData.BorderRadiusBottomRight.IsDefined()
                || rareData.BorderRadiusTopLeft.IsDefined()
                || rareData.BorderRadiusTopRight.IsDefined());

        #region Paint

        private Color borderColor = DefaultStyleValues.BorderColor;
        private Color backgroundColor = DefaultStyleValues.BackgroundColor;
        private Texture2DAssetReference backgroundImage = DefaultStyleValues.BackgroundImage;

        public Color BorderColor {
            [DebuggerStepThrough] get { return borderColor; }
            internal set {
                if (value == borderColor) {
                    return;
                }

                borderColor = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderColor, new StyleColor(borderColor).rgba));
            }
        }

        public Color BackgroundColor {
            [DebuggerStepThrough] get { return backgroundColor; }
            internal set {
                if (value == backgroundColor) {
                    return;
                }

                backgroundColor = value;
                SendEvent(new StyleProperty(StylePropertyId.BackgroundColor, new StyleColor(backgroundColor).rgba));
            }
        }

        public Texture2DAssetReference BackgroundImage {
            [DebuggerStepThrough] get { return backgroundImage; }
            internal set {
                if (backgroundImage.assetId == value.assetId) {
                    return;
                }

                backgroundImage = value;
                SendEvent(new StyleProperty(StylePropertyId.BackgroundImage, backgroundImage.assetId));
            }
        }

        #endregion

        #region Overflow

        private Overflow overflowX = DefaultStyleValues.OverflowX;
        private Overflow overflowY = DefaultStyleValues.OverflowY;

        public Overflow OverflowX {
            [DebuggerStepThrough] get { return overflowX; }
            internal set {
                if (value == overflowX) return;
                overflowX = value;
                SendEvent(new StyleProperty(StylePropertyId.OverflowX, (int) overflowX));
            }
        }

        public Overflow OverflowY {
            [DebuggerStepThrough] get { return overflowY; }
            internal set {
                if (value == overflowY) return;
                overflowY = value;
                SendEvent(new StyleProperty(StylePropertyId.OverflowY, (int) overflowY));
            }
        }

        #endregion

        #region Flex Item 

        private int flexGrowthFactor = DefaultStyleValues.FlexItemGrow;
        private int flexShrinkFactor = DefaultStyleValues.FlexItemShrink;
        private int flexOrderOverride = DefaultStyleValues.FlexItemOrder;
        private CrossAxisAlignment flexSelfAlignment = DefaultStyleValues.FlexItemSelfAlignment;

        public int FlexItemOrder {
            [DebuggerStepThrough] get { return flexOrderOverride; }
            internal set {
                if (value < 0) value = 0;
                if (flexOrderOverride == value) return;
                flexOrderOverride = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemOrder, flexOrderOverride));
            }
        }

        public int FlexItemGrowthFactor {
            [DebuggerStepThrough] get { return flexGrowthFactor; }
            internal set {
                if (value < 0) value = 0;
                if (flexGrowthFactor == value) return;
                flexGrowthFactor = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemGrow, flexGrowthFactor));
            }
        }

        public int FlexItemShrinkFactor {
            [DebuggerStepThrough] get { return flexShrinkFactor; }
            internal set {
                if (value < 0) value = 0;
                if (flexShrinkFactor == value) return;
                flexShrinkFactor = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemShrink, flexShrinkFactor));
            }
        }

        public CrossAxisAlignment FlexItemSelfAlignment {
            [DebuggerStepThrough] get { return flexSelfAlignment; }
            internal set {
                if (value == flexSelfAlignment) return;
                flexSelfAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int) flexSelfAlignment));
            }
        }

        #endregion

        #region Flex Layout

        private LayoutWrap flexLayoutWrap = DefaultStyleValues.FlexWrap;
        private LayoutDirection flexLayoutDirection = DefaultStyleValues.FlexLayoutDirection;
        private MainAxisAlignment flexLayoutMainAxisAlignment = DefaultStyleValues.FlexLayoutMainAxisAlignment;
        private CrossAxisAlignment flexLayoutCrossAxisAlignment = DefaultStyleValues.FlexLayoutCrossAxisAlignment;

        public LayoutDirection FlexLayoutDirection {
            [DebuggerStepThrough] get { return flexLayoutDirection; }
            internal set {
                if (flexLayoutDirection == value) return;
                flexLayoutDirection = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int) flexLayoutDirection));
            }
        }

        public LayoutWrap FlexLayoutWrap {
            [DebuggerStepThrough] get { return flexLayoutWrap; }
            internal set {
                if (flexLayoutWrap == value) return;
                flexLayoutWrap = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int) flexLayoutWrap));
            }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            [DebuggerStepThrough] get { return flexLayoutMainAxisAlignment; }
            internal set {
                if (flexLayoutMainAxisAlignment == value) return;
                flexLayoutMainAxisAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int) flexLayoutMainAxisAlignment));
            }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            [DebuggerStepThrough] get { return flexLayoutCrossAxisAlignment; }
            internal set {
                if (flexLayoutCrossAxisAlignment == value) return;
                flexLayoutCrossAxisAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int) flexLayoutCrossAxisAlignment));
            }
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

        private IReadOnlyList<GridTrackSize> gridLayoutColTemplate = DefaultStyleValues.GridLayoutColTemplate;
        private IReadOnlyList<GridTrackSize> gridLayoutRowTemplate = DefaultStyleValues.GridLayoutRowTemplate;

        private GridTrackSize gridLayoutColAutoSize = DefaultStyleValues.GridLayoutColAutoSize;
        private GridTrackSize gridLayoutRowAutoSize = DefaultStyleValues.GridLayoutRowAutoSize;

        public LayoutDirection GridLayoutDirection {
            [DebuggerStepThrough] get { return (LayoutDirection) ReadInt(StylePropertyId.GridLayoutDirection, (int) DefaultStyleValues.GridLayoutDirection); }
            set { WriteInt(StylePropertyId.GridLayoutDirection, (int) value); }
        }

        public GridLayoutDensity GridLayoutDensity {
            [DebuggerStepThrough] get { return (GridLayoutDensity) ReadInt(StylePropertyId.GridLayoutDensity, (int) DefaultStyleValues.GridLayoutDensity); }
            set { WriteInt(StylePropertyId.GridLayoutDensity, (int) value); }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutColTemplate {
            [DebuggerStepThrough] get { return gridLayoutColTemplate; }
            set {
                if (Equals(gridLayoutColTemplate, value)) {
                    return;
                }

                gridLayoutColTemplate = value;
                SendEvent(new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, gridLayoutColTemplate));
            }
        }

        public IReadOnlyList<GridTrackSize> GridLayoutRowTemplate {
            [DebuggerStepThrough] get { return gridLayoutRowTemplate; }
            set {
                if (Equals(gridLayoutRowTemplate, value)) {
                    return;
                }

                gridLayoutRowTemplate = value;
                SendEvent(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, gridLayoutRowTemplate));
            }
        }

        public GridTrackSize GridLayoutColAutoSize {
            [DebuggerStepThrough] get { return gridLayoutColAutoSize; }
            set {
                if (gridLayoutColAutoSize == value) {
                    return;
                }

                gridLayoutColAutoSize = value;
                SendEvent(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, FloatUtil.EncodeToInt(value.minValue), (int) value.minUnit));
            }
        }

        public GridTrackSize GridLayoutRowAutoSize {
            [DebuggerStepThrough] get { return gridLayoutRowAutoSize; }
            set {
                if (gridLayoutRowAutoSize == value) {
                    return;
                }

                gridLayoutRowAutoSize = value;
                SendEvent(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, FloatUtil.EncodeToInt(value.minValue), (int) value.minUnit));
            }
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

        private UIMeasurement minWidth = DefaultStyleValues.MinWidth;
        private UIMeasurement maxWidth = DefaultStyleValues.MaxWidth;
        private UIMeasurement preferredWidth = DefaultStyleValues.PreferredWidth;

        private UIMeasurement minHeight = DefaultStyleValues.MinHeight;
        private UIMeasurement maxHeight = DefaultStyleValues.MaxHeight;
        private UIMeasurement preferredHeight = DefaultStyleValues.PreferredHeight;

        public UIMeasurement MinWidth {
            [DebuggerStepThrough] get { return minWidth; }
            internal set {
                if (minWidth == value) return;
                minWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.MinWidth, FloatUtil.EncodeToInt(minWidth.value), (int) minWidth.unit));
            }
        }

        public UIMeasurement MaxWidth {
            [DebuggerStepThrough] get { return maxWidth; }
            internal set {
                if (maxWidth == value) return;
                maxWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.MaxWidth, FloatUtil.EncodeToInt(maxWidth.value), (int) maxWidth.unit));
            }
        }

        public UIMeasurement PreferredWidth {
            [DebuggerStepThrough] get { return preferredWidth; }
            internal set {
                if (preferredWidth == value) return;
                preferredWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.PreferredWidth, FloatUtil.EncodeToInt(preferredWidth.value), (int) preferredWidth.unit));
            }
        }

        public UIMeasurement MinHeight {
            [DebuggerStepThrough] get { return minHeight; }
            internal set {
                if (minHeight == value) return;
                minHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.MinHeight, FloatUtil.EncodeToInt(minHeight.value), (int) minHeight.unit));
            }
        }

        public UIMeasurement MaxHeight {
            [DebuggerStepThrough] get { return maxHeight; }
            internal set {
                if (maxHeight == value) return;
                maxHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.MaxHeight, FloatUtil.EncodeToInt(maxHeight.value), (int) maxHeight.unit));
            }
        }

        public UIMeasurement PreferredHeight {
            [DebuggerStepThrough] get { return preferredHeight; }
            internal set {
                if (preferredHeight == value) return;
                preferredHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.PreferredHeight, FloatUtil.EncodeToInt(preferredHeight.value), (int) preferredHeight.unit));
            }
        }

        public bool WidthIsParentBased => MinWidth.IsParentBased || MaxWidth.IsParentBased || PreferredWidth.IsParentBased;
        public bool HeightIsParentBased => MinHeight.IsParentBased || MaxHeight.IsParentBased || PreferredHeight.IsParentBased;

        public bool WidthIsContentBased => MinWidth.IsContentBased || MaxWidth.IsContentBased || PreferredWidth.IsContentBased;
        public bool HeightIsContentBased => MinHeight.IsContentBased || MaxHeight.IsContentBased || PreferredHeight.IsContentBased;

        public bool IsWidthFixed => MinWidth.IsFixed && MaxWidth.IsFixed && PreferredWidth.IsFixed;
        public bool IsHeightFixed => MinHeight.IsFixed && MaxHeight.IsFixed && PreferredHeight.IsFixed;

        #endregion

        #region Margin

        private UIMeasurement marginTop = DefaultStyleValues.MarginTop;
        private UIMeasurement marginRight = DefaultStyleValues.MarginRight;
        private UIMeasurement marginBottom = DefaultStyleValues.MarginBottom;
        private UIMeasurement marginLeft = DefaultStyleValues.MarginLeft;

        public UIMeasurement MarginTop {
            [DebuggerStepThrough] get { return marginTop; }
            internal set {
                if (marginTop == value) return;
                marginTop = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginTop, FloatUtil.EncodeToInt(marginTop.value), (int) marginTop.unit));
            }
        }

        public UIMeasurement MarginRight {
            [DebuggerStepThrough] get { return marginRight; }
            internal set {
                if (marginRight == value) return;
                marginRight = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginRight, FloatUtil.EncodeToInt(MarginRight.value), (int) MarginRight.unit));
            }
        }

        public UIMeasurement MarginBottom {
            [DebuggerStepThrough] get { return marginBottom; }
            internal set {
                if (marginBottom == value) return;
                marginBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginBottom, FloatUtil.EncodeToInt(marginBottom.value), (int) marginBottom.unit));
            }
        }

        public UIMeasurement MarginLeft {
            [DebuggerStepThrough] get { return marginLeft; }
            internal set {
                if (marginLeft == value) return;
                marginLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginLeft, FloatUtil.EncodeToInt(marginLeft.value), (int) marginLeft.unit));
            }
        }

        #endregion

        #region Border

        private UIFixedLength borderTop = DefaultStyleValues.BorderTop;
        private UIFixedLength borderRight = DefaultStyleValues.BorderRight;
        private UIFixedLength borderBottom = DefaultStyleValues.BorderBottom;
        private UIFixedLength borderLeft = DefaultStyleValues.BorderLeft;

        public UIFixedLength BorderTop {
            [DebuggerStepThrough] get { return borderTop; }
            internal set {
                if (borderTop == value) return;
                borderTop = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderTop, FloatUtil.EncodeToInt(borderTop.value), (int) borderTop.unit));
            }
        }

        public UIFixedLength BorderRight {
            [DebuggerStepThrough] get { return borderRight; }
            internal set {
                if (borderRight == value) return;
                borderRight = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderRight, FloatUtil.EncodeToInt(borderRight.value), (int) borderRight.unit));
            }
        }

        public UIFixedLength BorderBottom {
            [DebuggerStepThrough] get { return borderBottom; }
            internal set {
                if (borderBottom == value) return;
                borderBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderBottom, FloatUtil.EncodeToInt(borderBottom.value), (int) borderBottom.unit));
            }
        }

        public UIFixedLength BorderLeft {
            [DebuggerStepThrough] get { return borderLeft; }
            internal set {
                if (borderLeft == value) return;
                borderLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderLeft, FloatUtil.EncodeToInt(borderLeft.value), (int) borderLeft.unit));
            }
        }

        #endregion

        #region Padding

        private UIFixedLength paddingTop = DefaultStyleValues.PaddingTop;
        private UIFixedLength paddingRight = DefaultStyleValues.PaddingRight;
        private UIFixedLength paddingBottom = DefaultStyleValues.PaddingBottom;
        private UIFixedLength paddingLeft = DefaultStyleValues.PaddingLeft;

        public UIFixedLength PaddingTop {
            [DebuggerStepThrough] get { return paddingTop; }
            internal set {
                if (paddingTop == value) return;
                paddingTop = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingTop, FloatUtil.EncodeToInt(paddingTop.value), (int) paddingTop.unit));
            }
        }

        public UIFixedLength PaddingRight {
            [DebuggerStepThrough] get { return paddingRight; }
            internal set {
                if (paddingRight == value) return;
                paddingRight = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingRight, FloatUtil.EncodeToInt(paddingRight.value), (int) paddingRight.unit));
            }
        }

        public UIFixedLength PaddingBottom {
            [DebuggerStepThrough] get { return paddingBottom; }
            internal set {
                if (paddingBottom == value) return;
                paddingBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingBottom, FloatUtil.EncodeToInt(paddingBottom.value), (int) paddingBottom.unit));
            }
        }

        public UIFixedLength PaddingLeft {
            [DebuggerStepThrough] get { return paddingLeft; }
            internal set {
                if (paddingLeft == value) return;
                paddingLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingLeft, FloatUtil.EncodeToInt(paddingLeft.value), (int) paddingLeft.unit));
            }
        }

        #endregion

        #region Text Properties

        private int fontSize = DefaultStyleValues.TextFontSize;
        private Color textColor = DefaultStyleValues.TextColor;
        private FontAssetReference fontAsset = DefaultStyleValues.TextFontAsset;
        private TextUtil.FontStyle fontStyle = DefaultStyleValues.TextFontStyle;
        private TextUtil.TextAlignment m_TextAlignment = DefaultStyleValues.TextAlignment;
        private TextUtil.TextTransform textTransform = DefaultStyleValues.TextTransform;

        public Color TextColor {
            [DebuggerStepThrough] get { return textColor; }
            internal set {
                if (textColor == value) return;
                textColor = value;
                SendEvent(new StyleProperty(StylePropertyId.TextColor, new StyleColor(textColor).rgba));
            }
        }

        public FontAssetReference FontAsset {
            [DebuggerStepThrough] get { return fontAsset; }
            internal set {
                if (fontAsset.assetId == value.assetId) return;
                fontAsset = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontAsset, fontAsset.assetId));
            }
        }

        public int FontSize {
            [DebuggerStepThrough] get { return fontSize; }
            internal set {
                if (fontSize == value) return;
                fontSize = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontSize, fontSize));
            }
        }

        public TextUtil.FontStyle FontStyle {
            [DebuggerStepThrough] get { return fontStyle; }
            internal set {
                if (fontStyle == value) return;
                fontStyle = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontStyle, (int) fontStyle));
            }
        }

        public TextUtil.TextAlignment TextAlignment {
            [DebuggerStepThrough] get { return m_TextAlignment; }
            internal set {
                if (m_TextAlignment == value) return;
                m_TextAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.TextAnchor, (int) m_TextAlignment));
            }
        }

        public TextUtil.TextTransform TextTransform {
            [DebuggerStepThrough] get { return textTransform; }
            internal set {
                if (textTransform == value) return;
                textTransform = value;
                SendEvent(new StyleProperty(StylePropertyId.TextTransform, (int) textTransform));
            }
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

        private UIFixedLength transformPositionX = DefaultStyleValues.TransformPositionX;
        private UIFixedLength transformPositionY = DefaultStyleValues.TransformPositionY;
        private UIFixedLength transformPivotX = DefaultStyleValues.TransformPivotX;
        private UIFixedLength transformPivotY = DefaultStyleValues.TransformPivotY;
        private float transformScaleX = DefaultStyleValues.TransformScaleX;
        private float transformScaleY = DefaultStyleValues.TransformScaleY;
        private float transformRotation = DefaultStyleValues.TransformRotation;

        public UIFixedLength TransformPositionX {
            [DebuggerStepThrough] get { return transformPositionX; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.TransformPositionX;
                if (transformPositionX == value) return;
                transformPositionX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPositionX, FloatUtil.EncodeToInt(transformPositionX.value), (int) transformPositionX.unit));
            }
        }

        public UIFixedLength TransformPositionY {
            [DebuggerStepThrough] get { return transformPositionY; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.TransformPositionY;
                if (transformPositionY == value) return;
                transformPositionY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPositionY, FloatUtil.EncodeToInt(transformPositionY.value), (int) transformPositionY.unit));
            }
        }

        public UIFixedLength TransformPivotX {
            [DebuggerStepThrough] get { return transformPivotX; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.TransformPivotX;
                if (transformPivotX == value) return;
                transformPivotX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPivotX, FloatUtil.EncodeToInt(transformPivotX.value), (int) transformPivotX.unit));
            }
        }

        public UIFixedLength TransformPivotY {
            [DebuggerStepThrough] get { return transformPivotY; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.TransformPivotY;
                if (transformPivotY == value) return;
                transformPivotY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPivotY, FloatUtil.EncodeToInt(transformPivotY.value), (int) transformPivotY.unit));
            }
        }

        public float TransformScaleX {
            [DebuggerStepThrough] get { return transformScaleX; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.TransformScaleX;
                if (Mathf.Approximately(value, transformScaleX)) return;
                transformScaleX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformScaleX, FloatUtil.EncodeToInt(transformScaleX)));
            }
        }

        public float TransformScaleY {
            [DebuggerStepThrough] get { return transformScaleY; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.TransformScaleY;
                if (Mathf.Approximately(value, transformScaleY)) return;
                transformScaleY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformScaleY, FloatUtil.EncodeToInt(transformScaleY)));
            }
        }

        public float TransformRotation {
            [DebuggerStepThrough] get { return transformRotation; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.TransformRotation;
                if (Mathf.Approximately(value, transformRotation)) return;
                transformRotation = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformRotation, FloatUtil.EncodeToInt(transformRotation)));
            }
        }

        public FixedLengthVector TransformPosition {
            [DebuggerStepThrough] get { return new FixedLengthVector(transformPositionX, transformPositionY); }
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

        private LayoutType layoutType = DefaultStyleValues.LayoutType;
        private LayoutBehavior layoutBehavior = DefaultStyleValues.LayoutBehavior;

        public LayoutType LayoutType {
            [DebuggerStepThrough] get { return layoutType; }
            internal set {
                if (layoutType == value) return;
                layoutType = value;
                SendEvent(new StyleProperty(StylePropertyId.LayoutType, (int) layoutType));
            }
        }

        public LayoutBehavior LayoutBehavior {
            [DebuggerStepThrough] get { return layoutBehavior; }
            internal set {
                if (layoutBehavior == value) return;
                layoutBehavior = value;
                SendEvent(new StyleProperty(StylePropertyId.LayoutBehavior, (int) layoutBehavior));
            }
        }

        public float EmSize => fontAsset.asset.fontInfo.PointSize;

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
            get { return ReadInt(StylePropertyId.RenderLayerOffset, DefaultStyleValues.LayerOffset); }
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
                    overflowX = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowX;
                    break;
                case StylePropertyId.OverflowY:
                    overflowY = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowY;
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
                    BackgroundImage = property.IsDefined ? new Texture2DAssetReference(value0) : DefaultStyleValues.BackgroundImage;
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    RareData.BorderRadiusTopLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopLeft;
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    RareData.BorderRadiusTopRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopRight;
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    RareData.BorderRadiusBottomLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomLeft;
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
                    RareData.BorderRadiusBottomRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomRight;
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
                    FlexLayoutWrap = property.IsDefined ? (LayoutWrap) value0 : DefaultStyleValues.FlexWrap;
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
                    FlexItemGrowthFactor = property.IsDefined ? value0 : DefaultStyleValues.FlexItemGrow;
                    break;
                case StylePropertyId.FlexItemShrink:
                    FlexItemShrinkFactor = property.IsDefined ? value0 : DefaultStyleValues.FlexItemShrink;
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
                    transformPivotX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPivotX;
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
                    FontAsset = property.IsDefined ? new FontAssetReference(value0) : DefaultStyleValues.TextFontAsset;
                    break;
                case StylePropertyId.TextFontSize:
                    FontSize = property.IsDefined ? value0 : DefaultStyleValues.TextFontSize;
                    break;
                case StylePropertyId.TextFontStyle:
                    FontStyle = property.IsDefined ? (TextUtil.FontStyle) value0 : DefaultStyleValues.TextFontStyle;
                    break;
                case StylePropertyId.TextAnchor:
                    TextAlignment = property.IsDefined ? (TextUtil.TextAlignment) value0 : DefaultStyleValues.TextAlignment;
                    break;
                case StylePropertyId.TextTransform:
                    TextTransform = property.IsDefined ? (TextUtil.TextTransform) value0 : DefaultStyleValues.TextTransform;
                    break;
                case StylePropertyId.TextWhitespaceMode:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextWrapMode:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextHorizontalOverflow:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextVerticalOverflow:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextIndentFirstLine:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextIndentNewLine:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextLayoutStyle:
                    throw new NotImplementedException();
                    break;
                case StylePropertyId.TextAutoSize:
                    throw new NotImplementedException();
                    break;

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
                    LayerOffset = property.IsDefined ? property.AsInt : DefaultStyleValues.LayerOffset;
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

        internal StyleProperty GetProperty(StylePropertyId propertyId) {
            switch (propertyId) {
                case StylePropertyId.TransformRotation:
                    return StyleProperty.TransformRotation(TransformRotation);

                case StylePropertyId.TransformPositionX:
                    return StyleProperty.TransformPositionX(TransformPositionX);

                case StylePropertyId.TransformPositionY:
                    return StyleProperty.TransformPositionY(TransformPositionY);

                case StylePropertyId.MinWidth:
                    return StyleProperty.MinWidth(MinWidth);

                case StylePropertyId.MaxWidth:
                    return StyleProperty.MaxWidth(MaxWidth);

                case StylePropertyId.PreferredWidth:
                    return StyleProperty.PreferredWidth(PreferredWidth);

                case StylePropertyId.MinHeight:
                    return StyleProperty.MinHeight(MinHeight);

                case StylePropertyId.MaxHeight:
                    return StyleProperty.MaxHeight(MaxHeight);

                case StylePropertyId.PreferredHeight:
                    return StyleProperty.PreferredHeight(PreferredHeight);

                case StylePropertyId.BackgroundColor:
                    return StyleProperty.BackgroundColor(BackgroundColor);

                case StylePropertyId.PaddingTop:
                    return StyleProperty.PaddingTop(PaddingTop);

                case StylePropertyId.PaddingRight:
                    return StyleProperty.PaddingRight(PaddingRight);

                case StylePropertyId.PaddingBottom:
                    return StyleProperty.PaddingBottom(PaddingBottom);

                case StylePropertyId.PaddingLeft:
                    return StyleProperty.PaddingLeft(PaddingLeft);

                case StylePropertyId.BorderTop:
                    return StyleProperty.BorderTop(BorderTop);

                case StylePropertyId.BorderRight:
                    return StyleProperty.BorderRight(BorderRight);

                case StylePropertyId.BorderBottom:
                    return StyleProperty.BorderBottom(BorderBottom);

                case StylePropertyId.BorderLeft:
                    return StyleProperty.BorderLeft(BorderLeft);

                case StylePropertyId.MarginTop:
                    return StyleProperty.MarginTop(MarginTop);

                case StylePropertyId.MarginRight:
                    return StyleProperty.MarginRight(MarginRight);

                case StylePropertyId.MarginBottom:
                    return StyleProperty.MarginBottom(MarginBottom);

                case StylePropertyId.MarginLeft:
                    return StyleProperty.MarginLeft(MarginLeft);

                case StylePropertyId.AnchorTop:
                    return StyleProperty.AnchorTop(AnchorTop);

                case StylePropertyId.AnchorRight:
                    return StyleProperty.AnchorRight(AnchorRight);

                case StylePropertyId.AnchorBottom:
                    return StyleProperty.AnchorBottom(AnchorBottom);

                case StylePropertyId.AnchorLeft:
                    return StyleProperty.AnchorLeft(AnchorLeft);

                case StylePropertyId.AnchorTarget:
                    return StyleProperty.AnchorTarget(AnchorTarget);

                case StylePropertyId.ZIndex:
                    return StyleProperty.ZIndex(ZIndex);

                case StylePropertyId.RenderLayerOffset:
                    return StyleProperty.LayerOffset(LayerOffset);

                case StylePropertyId.RenderLayer:
                    return StyleProperty.RenderLayer(RenderLayer);

                default:
                    throw new ArgumentOutOfRangeException("Missing: " + propertyId);
            }
        }

        [DebuggerStepThrough]
        private UIFixedLength ReadFixedLength(StylePropertyId propertyId, UIFixedLength defaultValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                return retn.AsFixedLength;
            }

            return defaultValue;
        }

//        [DebuggerStepThrough]
        private void WriteFixedLength(StylePropertyId propertyId, UIFixedLength newValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private int ReadInt(StylePropertyId propertyId, int defaultValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                return retn.AsInt;
            }

            return defaultValue;
        }

        private void WriteInt(StylePropertyId propertyId, int newValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private float ReadFloat(StylePropertyId propertyId, float defaultValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                return retn.AsFloat;
            }

            return defaultValue;
        }

        private void WriteFloat(StylePropertyId propertyId, float newValue) {
            StyleProperty retn;
            properties = properties ?? new Dictionary<StylePropertyId, StyleProperty>();
            if (properties.TryGetValue(propertyId, out retn)) {
                if (retn.AsFloat == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, FloatUtil.EncodeToInt(newValue));
            properties[propertyId] = property;
            SendEvent(property);
        }

    }

    public class RareStyleData {

        public UIMeasurement borderRadiusTopLeft = DefaultStyleValues.BorderRadiusTopLeft;
        public UIMeasurement borderRadiusTopRight = DefaultStyleValues.BorderRadiusTopRight;
        public UIMeasurement borderRadiusBottomRight = DefaultStyleValues.BorderRadiusBottomRight;
        public UIMeasurement borderRadiusBottomLeft = DefaultStyleValues.BorderRadiusBottomLeft;

        private UIStyleSet styleSet;

        public RareStyleData(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public BorderRadius borderRadius =>
            new BorderRadius(
                borderRadiusTopLeft,
                borderRadiusTopRight,
                borderRadiusBottomRight,
                borderRadiusBottomLeft
            );

        public UIMeasurement BorderRadiusTopLeft {
            [DebuggerStepThrough] get { return borderRadiusTopLeft; }
            internal set {
                if (borderRadiusTopLeft == value) return;
                borderRadiusTopLeft = value;
                styleSet.styleSystem.SetStyleProperty(
                    styleSet.element,
                    new StyleProperty(
                        StylePropertyId.BorderRadiusTopLeft,
                        FloatUtil.EncodeToInt(BorderRadiusTopLeft.value),
                        (int) BorderRadiusTopLeft.unit
                    )
                );
            }
        }

        public UIMeasurement BorderRadiusTopRight {
            [DebuggerStepThrough] get { return borderRadiusTopRight; }
            internal set {
                if (borderRadiusTopRight == value) return;
                borderRadiusTopRight = value;
                styleSet.styleSystem.SetStyleProperty(
                    styleSet.element,
                    new StyleProperty(
                        StylePropertyId.BorderRadiusTopRight,
                        FloatUtil.EncodeToInt(BorderRadiusTopRight.value),
                        (int) BorderRadiusTopRight.unit
                    )
                );
            }
        }

        public UIMeasurement BorderRadiusBottomRight {
            [DebuggerStepThrough] get { return borderRadiusBottomRight; }
            internal set {
                if (borderRadiusBottomRight == value) return;
                borderRadiusBottomRight = value;
                styleSet.styleSystem.SetStyleProperty(
                    styleSet.element,
                    new StyleProperty(
                        StylePropertyId.BorderRadiusBottomRight,
                        FloatUtil.EncodeToInt(BorderRadiusBottomRight.value),
                        (int) BorderRadiusBottomRight.unit
                    )
                );
            }
        }

        public UIMeasurement BorderRadiusBottomLeft {
            [DebuggerStepThrough] get { return borderRadiusBottomLeft; }
            internal set {
                if (borderRadiusBottomLeft == value) return;
                borderRadiusBottomLeft = value;
                styleSet.styleSystem.SetStyleProperty(
                    styleSet.element,
                    new StyleProperty(
                        StylePropertyId.BorderRadiusBottomLeft,
                        FloatUtil.EncodeToInt(borderRadiusBottomLeft.value),
                        (int) borderRadiusBottomLeft.unit
                    )
                );
            }
        }

    }

}