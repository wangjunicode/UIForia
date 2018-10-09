using System;
using Src;
using Src.Layout;
using Src.Rendering;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Rendering {

    public class ComputedStyle {

        public class RareStyleData {

            public UIMeasurement borderRadiusTopLeft = DefaultStyleValues.borderRadiusTopLeft;
            public UIMeasurement borderRadiusTopRight = DefaultStyleValues.borderRadiusTopRight;
            public UIMeasurement borderRadiusBottomRight = DefaultStyleValues.borderRadiusBottomRight;
            public UIMeasurement borderRadiusBottomLeft = DefaultStyleValues.borderRadiusBottomLeft;

            private UIStyleSet styleSet;

            public RareStyleData(UIStyleSet styleSet) {
                this.styleSet = styleSet;
            }

            public BorderRadius borderRadius => new BorderRadius(
                borderRadiusTopLeft,
                borderRadiusTopRight,
                borderRadiusBottomRight,
                borderRadiusBottomLeft
            );

            public UIMeasurement BorderRadiusTopLeft {
                get { return borderRadiusTopLeft; }
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
                get { return borderRadiusTopRight; }
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
                get { return borderRadiusBottomRight; }
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
                get { return borderRadiusBottomLeft; }
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

        private UIStyleSet styleSet;
        private RareStyleData rareData;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public RareStyleData RareData => rareData ?? (rareData = new RareStyleData(styleSet));

        public PaddingBox border => new PaddingBox(borderTop, borderRight, borderBottom, borderLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, marginRight, marginBottom, marginLeft);

        public PaddingBox padding => new PaddingBox(paddingTop, paddingRight, paddingBottom, paddingLeft);

        public bool HasBorderRadius => rareData != null
                                       && (rareData.borderRadiusBottomLeft.IsDefined()
                                           || rareData.BorderRadiusBottomRight.IsDefined()
                                           || rareData.BorderRadiusTopLeft.IsDefined()
                                           || rareData.BorderRadiusTopRight.IsDefined());

        #region Paint

        private Color borderColor = DefaultStyleValues.borderColor;
        private Color backgroundColor = DefaultStyleValues.backgroundColor;
        private Texture2DAssetReference backgroundImage = DefaultStyleValues.backgroundImage;

        public Color BorderColor {
            get { return borderColor; }
            internal set {
                if (value == borderColor) {
                    return;
                }

                borderColor = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderColor, new StyleColor(borderColor).rgba));
            }
        }

        public Color BackgroundColor {
            get { return backgroundColor; }
            internal set {
                if (value == backgroundColor) {
                    return;
                }

                backgroundColor = value;
                SendEvent(new StyleProperty(StylePropertyId.BackgroundColor, new StyleColor(backgroundColor).rgba));
            }
        }

        public Texture2DAssetReference BackgroundImage {
            get { return backgroundImage; }
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

        private Overflow overflowX = DefaultStyleValues.overflowX;
        private Overflow overflowY = DefaultStyleValues.overflowY;

        public Overflow OverflowX {
            get { return overflowX; }
            internal set {
                if (value == overflowX) return;
                overflowX = value;
                SendEvent(new StyleProperty(StylePropertyId.OverflowX, (int) overflowX));
            }
        }

        public Overflow OverflowY {
            get { return overflowY; }
            internal set {
                if (value == overflowY) return;
                overflowY = value;
                SendEvent(new StyleProperty(StylePropertyId.OverflowY, (int) overflowY));
            }
        }

        #endregion

        #region Flex Item 

        private int flexGrowthFactor = DefaultStyleValues.flexGrowthFactor;
        private int flexShrinkFactor = DefaultStyleValues.flexShrinkFactor;
        private int flexOrderOverride = DefaultStyleValues.flexOrderOverride;
        private CrossAxisAlignment flexSelfAlignment = DefaultStyleValues.flexSelfAlignment;

        public int FlexItemOrder {
            get { return flexOrderOverride; }
            internal set {
                if (value < 0) value = 0;
                if (flexOrderOverride == value) return;
                flexOrderOverride = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemOrder, flexOrderOverride));
            }
        }

        public int FlexItemGrowthFactor {
            get { return flexGrowthFactor; }
            internal set {
                if (value < 0) value = 0;
                if (flexGrowthFactor == value) return;
                flexGrowthFactor = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemGrow, flexGrowthFactor));
            }
        }

        public int FlexItemShrinkFactor {
            get { return flexShrinkFactor; }
            internal set {
                if (value < 0) value = 0;
                if (flexShrinkFactor == value) return;
                flexShrinkFactor = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemShrink, flexShrinkFactor));
            }
        }

        public CrossAxisAlignment FlexItemSelfAlignment {
            get { return flexSelfAlignment; }
            internal set {
                if (value == flexSelfAlignment) return;
                flexSelfAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexItemSelfAlignment, (int) flexSelfAlignment));
            }
        }

        #endregion

        #region Flex Layout

        private LayoutWrap flexLayoutWrap = DefaultStyleValues.flexLayoutWrap;
        private LayoutDirection flexLayoutDirection = DefaultStyleValues.flexLayoutDirection;
        private MainAxisAlignment flexLayoutMainAxisAlignment = DefaultStyleValues.flexLayoutMainAxisAlignment;
        private CrossAxisAlignment flexLayoutCrossAxisAlignment = DefaultStyleValues.flexLayoutCrossAxisAlignment;

        public LayoutDirection FlexLayoutDirection {
            get { return flexLayoutDirection; }
            internal set {
                if (flexLayoutDirection == value) return;
                flexLayoutDirection = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexDirection, (int) flexLayoutDirection));
            }
        }

        public LayoutWrap FlexLayoutWrap {
            get { return flexLayoutWrap; }
            internal set {
                if (flexLayoutWrap == value) return;
                flexLayoutWrap = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexWrap, (int) flexLayoutWrap));
            }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            get { return flexLayoutMainAxisAlignment; }
            internal set {
                if (flexLayoutMainAxisAlignment == value) return;
                flexLayoutMainAxisAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexMainAxisAlignment, (int) flexLayoutMainAxisAlignment));
            }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            get { return flexLayoutCrossAxisAlignment; }
            internal set {
                if (flexLayoutCrossAxisAlignment == value) return;
                flexLayoutCrossAxisAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.FlexCrossAxisAlignment, (int) flexLayoutCrossAxisAlignment));
            }
        }

        #endregion

        #region Grid Item

        #endregion

        #region Grid Layout

        #endregion

        #region Size       

        private UIMeasurement minWidth = DefaultStyleValues.minWidth;
        private UIMeasurement maxWidth = DefaultStyleValues.maxWidth;
        private UIMeasurement preferredWidth = DefaultStyleValues.preferredWidth;

        private UIMeasurement minHeight = DefaultStyleValues.minHeight;
        private UIMeasurement maxHeight = DefaultStyleValues.maxHeight;
        private UIMeasurement preferredHeight = DefaultStyleValues.preferredHeight;

        public UIMeasurement MinWidth {
            get { return minWidth; }
            internal set {
                if (minWidth == value) return;
                UIMeasurement old = minWidth;
                minWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.MinWidth, FloatUtil.EncodeToInt(minWidth.value), (int) minWidth.unit));
            }
        }

        public UIMeasurement MaxWidth {
            get { return maxWidth; }
            internal set {
                if (maxWidth == value) return;
                UIMeasurement old = maxWidth;
                maxWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.MaxWidth, FloatUtil.EncodeToInt(maxWidth.value), (int) maxWidth.unit));
            }
        }

        public UIMeasurement PreferredWidth {
            get { return preferredWidth; }
            internal set {
                if (preferredWidth == value) return;
                UIMeasurement old = preferredWidth;
                preferredWidth = value;
                SendEvent(new StyleProperty(StylePropertyId.PreferredWidth, FloatUtil.EncodeToInt(preferredWidth.value), (int) preferredWidth.unit));
            }
        }

        public UIMeasurement MinHeight {
            get { return minHeight; }
            internal set {
                if (minHeight == value) return;
                minHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.MinHeight, FloatUtil.EncodeToInt(minHeight.value), (int) minHeight.unit));
            }
        }

        public UIMeasurement MaxHeight {
            get { return maxHeight; }
            internal set {
                if (maxHeight == value) return;
                maxHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.MaxHeight, FloatUtil.EncodeToInt(maxHeight.value), (int) maxHeight.unit));
            }
        }

        public UIMeasurement PreferredHeight {
            get { return preferredHeight; }
            internal set {
                if (preferredHeight == value) return;
                preferredHeight = value;
                SendEvent(new StyleProperty(StylePropertyId.PreferredHeight, FloatUtil.EncodeToInt(preferredHeight.value), (int) preferredHeight.unit));
            }
        }

        #endregion

        #region Margin

        private UIMeasurement marginTop = DefaultStyleValues.marginTop;
        private UIMeasurement marginRight = DefaultStyleValues.marginRight;
        private UIMeasurement marginBottom = DefaultStyleValues.marginBottom;
        private UIMeasurement marginLeft = DefaultStyleValues.marginLeft;

        public UIMeasurement MarginTop {
            get { return marginTop; }
            internal set {
                if (marginTop == value) return;
                marginTop = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginTop, FloatUtil.EncodeToInt(marginTop.value), (int) marginTop.unit));
            }
        }

        public UIMeasurement MarginRight {
            get { return marginRight; }
            internal set {
                if (marginRight == value) return;
                marginRight = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginRight, FloatUtil.EncodeToInt(MarginRight.value), (int) MarginRight.unit));
            }
        }

        public UIMeasurement MarginBottom {
            get { return marginBottom; }
            internal set {
                if (marginBottom == value) return;
                marginBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginBottom, FloatUtil.EncodeToInt(marginBottom.value), (int) marginBottom.unit));
            }
        }

        public UIMeasurement MarginLeft {
            get { return marginLeft; }
            internal set {
                if (marginLeft == value) return;
                marginLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.MarginLeft, FloatUtil.EncodeToInt(marginLeft.value), (int) marginLeft.unit));
            }
        }

        #endregion

        #region Border

        private UIFixedLength borderTop = DefaultStyleValues.borderTop;
        private UIFixedLength borderRight = DefaultStyleValues.borderRight;
        private UIFixedLength borderBottom = DefaultStyleValues.borderBottom;
        private UIFixedLength borderLeft = DefaultStyleValues.borderLeft;

        public UIFixedLength BorderTop {
            get { return borderTop; }
            internal set {
                if (borderTop == value) return;
                borderTop = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderTop, FloatUtil.EncodeToInt(borderTop.value), (int) borderTop.unit));
            }
        }

        public UIFixedLength BorderRight {
            get { return borderRight; }
            internal set {
                if (borderRight == value) return;
                borderRight = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderRight, FloatUtil.EncodeToInt(borderRight.value), (int) borderRight.unit));
            }
        }

        public UIFixedLength BorderBottom {
            get { return borderBottom; }
            internal set {
                if (borderBottom == value) return;
                borderBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderBottom, FloatUtil.EncodeToInt(borderBottom.value), (int) borderBottom.unit));
            }
        }

        public UIFixedLength BorderLeft {
            get { return borderLeft; }
            internal set {
                if (borderLeft == value) return;
                borderLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.BorderLeft, FloatUtil.EncodeToInt(borderLeft.value), (int) borderLeft.unit));
            }
        }

        #endregion

        #region Padding

        private UIFixedLength paddingTop = DefaultStyleValues.paddingTop;
        private UIFixedLength paddingRight = DefaultStyleValues.paddingRight;
        private UIFixedLength paddingBottom = DefaultStyleValues.paddingBottom;
        private UIFixedLength paddingLeft = DefaultStyleValues.paddingLeft;

        public UIFixedLength PaddingTop {
            get { return paddingTop; }
            internal set {
                if (paddingTop == value) return;
                paddingTop = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingTop, FloatUtil.EncodeToInt(paddingTop.value), (int) paddingTop.unit));
            }
        }

        public UIFixedLength PaddingRight {
            get { return paddingRight; }
            internal set {
                if (paddingRight == value) return;
                paddingRight = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingRight, FloatUtil.EncodeToInt(paddingRight.value), (int) paddingRight.unit));
            }
        }

        public UIFixedLength PaddingBottom {
            get { return paddingBottom; }
            internal set {
                if (paddingBottom == value) return;
                paddingBottom = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingBottom, FloatUtil.EncodeToInt(paddingBottom.value), (int) paddingBottom.unit));
            }
        }

        public UIFixedLength PaddingLeft {
            get { return paddingLeft; }
            internal set {
                if (paddingLeft == value) return;
                paddingLeft = value;
                SendEvent(new StyleProperty(StylePropertyId.PaddingLeft, FloatUtil.EncodeToInt(paddingLeft.value), (int) paddingLeft.unit));
            }
        }

        #endregion

        #region Text Properties

        private int fontSize = DefaultStyleValues.fontSize;
        private Color textColor = DefaultStyleValues.textColor;
        private FontAssetReference fontAsset = DefaultStyleValues.fontAsset;
        private TextUtil.FontStyle fontStyle = DefaultStyleValues.fontStyle;
        private TextUtil.TextAlignment m_TextAlignment = DefaultStyleValues.TextAlignment;
        private TextUtil.TextTransform textTransform = DefaultStyleValues.textTransform;

        public Color TextColor {
            get { return textColor; }
            internal set {
                if (textColor == value) return;
                textColor = value;
                SendEvent(new StyleProperty(StylePropertyId.TextColor, new StyleColor(textColor).rgba));
            }
        }

        public FontAssetReference FontAsset {
            get { return fontAsset; }
            internal set {
                if (fontAsset.assetId == value.assetId) return;
                fontAsset = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontAsset, fontAsset.assetId));
            }
        }

        public int FontSize {
            get { return fontSize; }
            internal set {
                if (fontSize == value) return;
                fontSize = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontSize, fontSize));
            }
        }

        public TextUtil.FontStyle FontStyle {
            get { return fontStyle; }
            internal set {
                if (fontStyle == value) return;
                fontStyle = value;
                SendEvent(new StyleProperty(StylePropertyId.TextFontStyle, (int) fontStyle));
            }
        }

        public TextUtil.TextAlignment TextAlignment {
            get { return m_TextAlignment; }
            internal set {
                if (m_TextAlignment == value) return;
                m_TextAlignment = value;
                SendEvent(new StyleProperty(StylePropertyId.TextAnchor, (int) m_TextAlignment));
            }
        }

        public TextUtil.TextTransform TextTransform {
            get { return textTransform; }
            internal set {
                if (textTransform == value) return;
                textTransform = value;
                SendEvent(new StyleProperty(StylePropertyId.TextTransform, (int) textTransform));
            }
        }

        #endregion

        #region Transform

        private UIFixedLength transformPositionX = DefaultStyleValues.transformPositionX;
        private UIFixedLength transformPositionY = DefaultStyleValues.transformPositionY;
        private UIFixedLength transformPivotX = DefaultStyleValues.transformPivotX;
        private UIFixedLength transformPivotY = DefaultStyleValues.transformPivotY;
        private float transformScaleX = DefaultStyleValues.transformScaleX;
        private float transformScaleY = DefaultStyleValues.transformScaleY;
        private float transformRotation = DefaultStyleValues.transformRotation;

        public UIFixedLength TransformPositionX {
            get { return transformPositionX; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.transformPositionX;
                if (transformPositionX == value) return;
                transformPositionX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPositionX, FloatUtil.EncodeToInt(transformPositionX.value), (int) transformPositionX.unit));
            }
        }

        public UIFixedLength TransformPositionY {
            get { return transformPositionY; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.transformPositionY;
                if (transformPositionY == value) return;
                transformPositionY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPositionY, FloatUtil.EncodeToInt(transformPositionY.value), (int) transformPositionY.unit));
            }
        }

        public UIFixedLength TransformPivotX {
            get { return transformPivotX; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.transformPivotX;
                if (transformPivotX == value) return;
                transformPivotX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPivotX, FloatUtil.EncodeToInt(transformPivotX.value), (int) transformPivotX.unit));
            }
        }

        public UIFixedLength TransformPivotY {
            get { return transformPivotY; }
            internal set {
                if (value == UIFixedLength.Unset) value = DefaultStyleValues.transformPivotY;
                if (transformPivotY == value) return;
                transformPivotY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformPivotY, FloatUtil.EncodeToInt(transformPivotY.value), (int) transformPivotY.unit));
            }
        }

        public float TransformScaleX {
            get { return transformScaleX; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformScaleX;
                if (Mathf.Approximately(value, transformScaleX)) return;
                transformScaleX = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformScaleX, FloatUtil.EncodeToInt(transformScaleX)));
            }
        }

        public float TransformScaleY {
            get { return transformScaleY; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformScaleY;
                if (Mathf.Approximately(value, transformScaleY)) return;
                transformScaleY = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformScaleY, FloatUtil.EncodeToInt(transformScaleY)));
            }
        }

        public float TransformRotation {
            get { return transformRotation; }
            internal set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformRotation;
                if (Mathf.Approximately(value, transformRotation)) return;
                transformRotation = value;
                SendEvent(new StyleProperty(StylePropertyId.TransformRotation, FloatUtil.EncodeToInt(transformRotation)));
            }
        }

        public FixedLengthVector TransformPosition {
            get { return new FixedLengthVector(transformPositionX, transformPositionY); }
            internal set {
                TransformPositionX = value.x;
                TransformPositionY = value.y;
            }
        }

        #endregion

        #region Layout

        private LayoutType layoutType = DefaultStyleValues.layoutType;
        private LayoutBehavior layoutBehavior = DefaultStyleValues.layoutBehavior;

        public LayoutType LayoutType {
            get { return layoutType; }
            internal set {
                if (layoutType == value) return;
                layoutType = value;
                SendEvent(new StyleProperty(StylePropertyId.LayoutType, (int) layoutType));
            }
        }

        public LayoutBehavior LayoutBehavior {
            get { return layoutBehavior; }
            internal set {
                if (layoutBehavior == value) return;
                layoutBehavior = value;
                SendEvent(new StyleProperty(StylePropertyId.LayoutBehavior, (int) layoutBehavior));
            }
        }

        #endregion

        private void SendEvent(StyleProperty property) {
            styleSet.styleSystem.SetStyleProperty(styleSet.element, property);
        }

        internal void SetProperty(StyleProperty property) {
            int value0 = property.valuePart0;
            int value1 = property.valuePart1;
            switch (property.propertyId) {
                case StylePropertyId.LayoutType:
                    LayoutType = property.IsDefined ? (LayoutType) value0 : DefaultStyleValues.layoutType;
                    break;

                #region Overflow

                case StylePropertyId.OverflowX:
                    overflowX = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.overflowX;
                    break;
                case StylePropertyId.OverflowY:
                    overflowY = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.overflowY;
                    break;

                #endregion

                #region Paint

                case StylePropertyId.BackgroundColor:
                    BackgroundColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.backgroundColor;
                    break;
                case StylePropertyId.BorderColor:
                    BorderColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.borderColor;
                    break;
                case StylePropertyId.BackgroundImage:
                    BackgroundImage = property.IsDefined ? new Texture2DAssetReference(value0) : DefaultStyleValues.backgroundImage;
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    RareData.BorderRadiusTopLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderRadiusTopLeft;
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    RareData.BorderRadiusTopRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderRadiusTopRight;
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    RareData.BorderRadiusBottomLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderRadiusBottomLeft;
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
                    RareData.BorderRadiusBottomRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderRadiusBottomRight;
                    break;

                #endregion

                #region Grid Item

                case StylePropertyId.GridItemColStart:
                    break;
                case StylePropertyId.GridItemColSpan:
                    break;
                case StylePropertyId.GridItemRowStart:
                    break;
                case StylePropertyId.GridItemRowSpan:
                    break;

                #endregion

                #region Grid Layout

                case StylePropertyId.GridFlowDirection:
                    break;
                case StylePropertyId.GridPlacementDensity:
                    break;
                case StylePropertyId.GridColTemplate:
                    break;
                case StylePropertyId.GridRowTemplate:
                    break;
                case StylePropertyId.GridColAutoSize:
                    break;
                case StylePropertyId.GridRowAutoSize:
                    break;
                case StylePropertyId.GridColGap:
                    break;
                case StylePropertyId.GridRowGap:
                    break;

                #endregion

                #region Flex Layout

                case StylePropertyId.FlexWrap:
                    FlexLayoutWrap = property.IsDefined ? (LayoutWrap) value0 : DefaultStyleValues.flexLayoutWrap;
                    break;
                case StylePropertyId.FlexDirection:
                    FlexLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : DefaultStyleValues.flexLayoutDirection;
                    break;
                case StylePropertyId.FlexMainAxisAlignment:
                    FlexLayoutMainAxisAlignment = property.IsDefined ? (MainAxisAlignment) value0 : DefaultStyleValues.flexLayoutMainAxisAlignment;
                    break;
                case StylePropertyId.FlexCrossAxisAlignment:
                    FlexLayoutCrossAxisAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.flexLayoutCrossAxisAlignment;
                    break;

                #endregion

                #region Flex Item

                case StylePropertyId.FlexItemSelfAlignment:
                    FlexItemSelfAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.flexSelfAlignment;
                    break;
                case StylePropertyId.FlexItemOrder:
                    FlexItemOrder = property.IsDefined ? value0 : DefaultStyleValues.flexOrderOverride;
                    break;
                case StylePropertyId.FlexItemGrow:
                    FlexItemGrowthFactor = property.IsDefined ? value0 : DefaultStyleValues.flexGrowthFactor;
                    break;
                case StylePropertyId.FlexItemShrink:
                    FlexItemShrinkFactor = property.IsDefined ? value0 : DefaultStyleValues.flexShrinkFactor;
                    break;

                #endregion

                #region Margin

                case StylePropertyId.MarginTop:
                    MarginTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.marginTop;
                    break;

                case StylePropertyId.MarginRight:
                    MarginRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.marginRight;
                    break;

                case StylePropertyId.MarginBottom:
                    MarginBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.marginBottom;
                    break;

                case StylePropertyId.MarginLeft:
                    MarginLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.marginLeft;
                    break;

                #endregion

                #region Border

                case StylePropertyId.BorderTop:
                    BorderTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.borderTop;
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.borderRight;
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.borderBottom;
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.borderLeft;
                    break;

                #endregion

                #region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.paddingTop;
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.paddingRight;
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.paddingBottom;
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.paddingLeft;
                    break;

                #endregion

                #region Transform

                case StylePropertyId.TransformPositionX:
                    TransformPositionX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.transformPositionX;
                    break;
                case StylePropertyId.TransformPositionY:
                    TransformPositionY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.transformPositionY;
                    break;
                case StylePropertyId.TransformScaleX:
                    TransformScaleX = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.transformScaleX;
                    break;
                case StylePropertyId.TransformScaleY:
                    TransformScaleY = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.transformScaleY;
                    break;
                case StylePropertyId.TransformPivotX:
                    transformPivotX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.transformPivotX;
                    break;
                case StylePropertyId.TransformPivotY:
                    TransformPivotY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.transformPivotY;
                    break;
                case StylePropertyId.TransformRotation:
                    TransformRotation = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.transformRotation;
                    break;

                #endregion

                #region Text

                case StylePropertyId.TextColor:
                    TextColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.textColor;
                    break;
                case StylePropertyId.TextFontAsset:
                    FontAsset = property.IsDefined ? new FontAssetReference(value0) : DefaultStyleValues.fontAsset;
                    break;
                case StylePropertyId.TextFontSize:
                    FontSize = property.IsDefined ? value0 : DefaultStyleValues.fontSize;
                    break;
                case StylePropertyId.TextFontStyle:
                    FontStyle = property.IsDefined ? (TextUtil.FontStyle) value0 : DefaultStyleValues.fontStyle;
                    break;
                case StylePropertyId.TextAnchor:
                    TextAlignment = property.IsDefined ? (TextUtil.TextAlignment) value0 : DefaultStyleValues.TextAlignment;
                    break;
                case StylePropertyId.TextTransform:
                    TextTransform = property.IsDefined ? (TextUtil.TextTransform) value0 : DefaultStyleValues.textTransform;
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
                    MinWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.minWidth;
                    break;

                case StylePropertyId.MaxWidth:
                    MaxWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.maxWidth;
                    break;

                case StylePropertyId.PreferredWidth:
                    PreferredWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.preferredWidth;
                    break;

                case StylePropertyId.MinHeight:
                    MinHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.minHeight;
                    break;

                case StylePropertyId.MaxHeight:
                    MaxHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.maxHeight;
                    break;

                case StylePropertyId.PreferredHeight:
                    PreferredHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.preferredHeight;
                    break;

                #endregion

                // SizeParameters -> None | IgnoreScaleForLayout | IgnoreRotationForLayout | PreMultiplyScale
                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                    break;
                case StylePropertyId.LayoutBehavior:
                    // FlowType -> Normal | Ignored | TranslationAsOffset
                    LayoutBehavior = property.IsDefined ? (LayoutBehavior) property.valuePart0 : DefaultStyleValues.layoutBehavior;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyId), property.propertyId, null);
            }
        }

    }

}