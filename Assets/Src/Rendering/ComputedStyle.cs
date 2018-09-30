using System;
using Src;
using Src.Layout;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public class ComputedStyle {

        public class RareStyleData {

            public UIMeasurement borderRadiusTopLeft;
            public UIMeasurement borderRadiusTopRight;
            public UIMeasurement borderRadiusBottomRight;
            public UIMeasurement borderRadiusBottomLeft;

            public BorderRadius borderRadius => new BorderRadius(
                borderRadiusTopLeft,
                borderRadiusTopRight,
                borderRadiusBottomRight,
                borderRadiusBottomLeft
            );

        }

        public class FlexItemProperties { }

        public class FlexLayoutProperties { }

        public class GridItemProperties { }

        public class GridLayoutProperties { }

        public class TextProperties { }

        private UIMeasurement borderTop;
        private UIMeasurement borderRight;
        private UIMeasurement borderBottom;
        private UIMeasurement borderLeft;

        private UIMeasurement paddingTop;
        private UIMeasurement paddingRight;
        private UIMeasurement paddingBottom;
        private UIMeasurement paddingLeft;

        private UIMeasurement minWidth;
        private UIMeasurement maxWidth;
        private UIMeasurement preferredWidth;

        private UIMeasurement minHeight;
        private UIMeasurement maxHeight;
        private UIMeasurement preferredHeight;

        private Overflow overflowX;
        private Overflow overflowY;

        private RareStyleData rareData;

        public LayoutWrap flexWrapMode;
        public LayoutDirection flexDirection;
        public MainAxisAlignment flexMainAxisAlignment;
        public CrossAxisAlignment flexCrossAxisAlignment;

        public int flexOrderOverride;
        public int flexGrowthFactor;
        public int flexShrinkFactor;
        public CrossAxisAlignment flexSelfAlignment;
        public LayoutType layoutType;

        private UIStyleSet styleSet;
        public Color backgroundColor;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public RareStyleData RareData => rareData ?? (rareData = new RareStyleData());

        public ContentBoxRect border => new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft);
        public ContentBoxRect padding => new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, marginRight, marginBottom, marginLeft);

        private Color borderColor;

        public Color BorderColor {
            get { return borderColor; }
            set {
                if (value == borderColor) {
                    return;
                }

                borderColor = value;
                styleSet.styleSystem.SetPaint(styleSet.element, new Paint(backgroundColor, borderColor, backgroundImage.asset));
            }
        }

        public Color BackgroundColor {
            get { return backgroundColor; }
            set {
                if (value == backgroundColor) {
                    return;
                }

                backgroundColor = value;
                styleSet.styleSystem.SetPaint(styleSet.element, new Paint(backgroundColor, borderColor, backgroundImage.asset));
            }
        }

        private AssetPointer<Texture2D> backgroundImage;

        public AssetPointer<Texture2D> BackgroundImage {
            get { return backgroundImage; }
            set {
                if (backgroundImage.id == value.id) {
                    return;
                }

                backgroundImage = value;
                styleSet.styleSystem.SetPaint(styleSet.element, new Paint(backgroundColor, borderColor, backgroundImage.asset));
            }
        }

        private UIMeasurement marginTop;

        public UIMeasurement MarginTop {
            get { return marginTop; }
            set {
                if (marginTop == value) return;
                marginTop = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        private UIMeasurement marginRight;

        public UIMeasurement MarginRight {
            get { return marginRight; }
            set {
                if (marginRight == value) return;
                marginRight = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        private UIMeasurement marginBottom;

        public UIMeasurement MarginBottom {
            get { return marginBottom; }
            set {
                if (marginBottom == value) return;
                marginBottom = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        private UIMeasurement marginLeft;

        public UIMeasurement MarginLeft {
            get { return marginLeft; }
            set {
                if (marginLeft == value) return;
                marginLeft = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        public UIMeasurement MinWidth {
            get { return minWidth; }
            set {
                if (minWidth == value) return;
                UIMeasurement old = minWidth;
                minWidth = value;
                styleSet.styleSystem.SetMinWidth(styleSet.element, minWidth, old);
            }
        }

        public UIMeasurement MaxWidth {
            get { return maxWidth; }
            set {
                if (maxWidth == value) return;
                UIMeasurement old = maxWidth;
                maxWidth = value;
                styleSet.styleSystem.SetMaxWidth(styleSet.element, maxWidth, old);
            }
        }

        public UIMeasurement PreferredWidth {
            get { return preferredWidth; }
            set {
                if (preferredWidth == value) return;
                UIMeasurement old = preferredWidth;
                preferredWidth = value;
                styleSet.styleSystem.SetPreferredWidth(styleSet.element, preferredWidth, old);
            }
        }

        public UIMeasurement MinHeight {
            get { return minHeight; }
            set {
                if (minHeight == value) return;
                UIMeasurement old = minHeight;
                minHeight = value;
                styleSet.styleSystem.SetMinHeight(styleSet.element, minHeight, old);
            }
        }

        public UIMeasurement MaxHeight {
            get { return maxHeight; }
            set {
                if (maxHeight == value) return;
                UIMeasurement old = maxHeight;
                maxHeight = value;
                styleSet.styleSystem.SetMaxHeight(styleSet.element, maxHeight, old);
            }
        }

        public UIMeasurement PreferredHeight {
            get { return preferredHeight; }
            set {
                if (preferredHeight == value) return;
                UIMeasurement old = preferredHeight;
                preferredHeight = value;
                styleSet.styleSystem.SetPreferredHeight(styleSet.element, preferredHeight, old);
            }
        }

        public UIMeasurement BorderTop {
            get { return borderTop; }
            set {
                if (borderTop == value) return;
                borderTop = value;
                styleSet.styleSystem.SetBorder(styleSet.element, border);
            }
        }

        public UIMeasurement BorderRight {
            get { return borderRight; }
            set {
                if (borderRight == value) return;
                borderRight = value;
                styleSet.styleSystem.SetBorder(styleSet.element, border);
            }
        }

        public UIMeasurement BorderBottom {
            get { return borderBottom; }
            set {
                if (borderBottom == value) return;
                borderBottom = value;
                styleSet.styleSystem.SetBorder(styleSet.element, border);
            }
        }

        public UIMeasurement BorderLeft {
            get { return borderLeft; }
            set {
                if (borderLeft == value) return;
                borderLeft = value;
                styleSet.styleSystem.SetBorder(styleSet.element, border);
            }
        }

        public UIMeasurement PaddingTop {
            get { return paddingTop; }
            set {
                if (paddingTop == value) return;
                paddingTop = value;
                styleSet.styleSystem.SetPadding(styleSet.element, padding);
            }
        }

        public UIMeasurement PaddingRight {
            get { return paddingRight; }
            set {
                if (paddingRight == value) return;
                paddingRight = value;
                styleSet.styleSystem.SetPadding(styleSet.element, padding);
            }
        }

        public UIMeasurement PaddingBottom {
            get { return paddingBottom; }
            set {
                if (paddingBottom == value) return;
                paddingBottom = value;
                styleSet.styleSystem.SetPadding(styleSet.element, padding);
            }
        }

        public UIMeasurement PaddingLeft {
            get { return paddingLeft; }
            set {
                if (paddingLeft == value) return;
                paddingLeft = value;
                styleSet.styleSystem.SetPadding(styleSet.element, padding);
            }
        }

#region Text Properties

        private Color textColor;

        public Color TextColor {
            get { return textColor; }
            set {
                if (textColor == value) return;
                textColor = value;
                styleSet.styleSystem.SetFontColor(styleSet.element, textColor);
            }
        }

        private AssetPointer<Font> fontAsset;

        public AssetPointer<Font> FontAsset {
            get { return fontAsset; }
            set {
                if (fontAsset.id == value.id) return;
                fontAsset = value;
                styleSet.styleSystem.SetFontAsset(styleSet.element, fontAsset);
            }
        }

        private int fontSize;

        public int FontSize {
            get { return fontSize; }
            set {
                if (fontSize == value) return;
                fontSize = value;
                styleSet.styleSystem.SetFontSize(styleSet.element, fontSize);
            }
        }

        private TextUtil.FontStyle fontStyle;

        public TextUtil.FontStyle FontStyle {
            get { return fontStyle; }
            set {
                if (fontStyle == value) return;
                fontStyle = value;
                styleSet.styleSystem.SetFontStyle(styleSet.element, fontStyle);
            }
        }

        private TextUtil.TextAnchor textAnchor;

        public TextUtil.TextAnchor TextAnchor {
            get { return textAnchor; }
            set {
                if (textAnchor == value) return;
                textAnchor = value;
                styleSet.styleSystem.SetTextAnchor(styleSet.element, textAnchor);
            }
        }

#endregion

#region Transform

        private UIMeasurement transformPositionX;
        private UIMeasurement transformPositionY;
        private UIMeasurement transformPivotX;
        private UIMeasurement transformPivotY;
        private float transformScaleX;
        private float transformScaleY;
        private float transformRotation;

        public UIMeasurement TransformPositionX {
            get { return transformPositionX; }
            set {
                if (value == UIMeasurement.Unset) value = new UIMeasurement(0);
                if (transformPositionX == value) return;
                transformPositionX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPositionY {
            get { return transformPositionY; }
            set {
                if (value == UIMeasurement.Unset) value = new UIMeasurement(0);
                if (transformPositionY == value) return;
                transformPositionY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPivotX {
            get { return transformPivotX; }
            set {
                if (value == UIMeasurement.Unset) value = new UIMeasurement(0);
                if (transformPivotX == value) return;
                transformPivotX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPivotY {
            get { return transformPivotY; }
            set {
                if (value == UIMeasurement.Unset) value = new UIMeasurement(0);
                if (transformPivotY == value) return;
                transformPivotY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformScaleX {
            get { return transformScaleX; }
            set {
                if (!FloatUtil.IsDefined(value)) value = 0;
                if (Mathf.Approximately(value, transformScaleX)) return;
                transformScaleX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformScaleY {
            get { return transformScaleY; }
            set {
                if (!FloatUtil.IsDefined(value)) value = 0;
                if (Mathf.Approximately(value, transformScaleY)) return;
                transformScaleY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformRotation {
            get { return transformRotation; }
            set {
                if (!FloatUtil.IsDefined(value)) value = 0;
                if (Mathf.Approximately(value, transformRotation)) return;
                transformRotation = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

#endregion

        internal void SetProperty(StyleProperty property) {
            int value0 = property.valuePart0;
            int value1 = property.valuePart1;
            switch (property.propertyId) {
                case StylePropertyId.OverflowX:
                    overflowX = (Overflow) value0;
                    break;
                case StylePropertyId.OverflowY:
                    overflowY = (Overflow) value0;
                    break;

#region Paint

                case StylePropertyId.BackgroundColor:
                    break;
                case StylePropertyId.BorderColor:
                    break;
                case StylePropertyId.BackgroundImage:

                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
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
                    break;
                case StylePropertyId.FlexDirection:
                    break;
                case StylePropertyId.FlexMainAxisAlignment:
                    break;
                case StylePropertyId.FlexCrossAxisAlignment:
                    break;

#endregion

#region Flex Item

                case StylePropertyId.FlexItemSelfAlignment:
                    break;
                case StylePropertyId.FlexItemOrder:
                    break;

                case StylePropertyId.FlexItemGrow:
                    break;
                case StylePropertyId.FlexItemShrink:
                    break;

#endregion

#region Margin

                case StylePropertyId.MarginTop:
                    MarginTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

                case StylePropertyId.MarginRight:
                    MarginRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

                case StylePropertyId.MarginBottom:
                    MarginBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

                case StylePropertyId.MarginLeft:
                    MarginLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

#endregion

#region Border

                case StylePropertyId.BorderTop:
                    BorderTop = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = UIMeasurement.Decode(value0, value1);
                    break;

#endregion

#region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = UIMeasurement.Decode(value0, value1);
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = UIMeasurement.Decode(value0, value1);
                    break;

#endregion

#region Transform

                case StylePropertyId.TransformPositionX:
                    break;
                case StylePropertyId.TransformPositionY:
                    break;
                case StylePropertyId.TransformScaleX:
                    break;
                case StylePropertyId.TransformScaleY:
                    break;
                case StylePropertyId.TransformPivotX:
                    break;
                case StylePropertyId.TransformPivotY:
                    break;
                case StylePropertyId.TransformRotation:
                    break;

#endregion

#region Text

                case StylePropertyId.TextColor:
                    TextColor = property.IsDefined ? (Color) new StyleColor(value0) : Color.black;
                    break;
                case StylePropertyId.TextFontAsset:
                    break;
                case StylePropertyId.TextFontSize:
                    break;
                case StylePropertyId.TextFontStyle:
                    break;
                case StylePropertyId.TextAnchor:
                    break;
                case StylePropertyId.TextWhitespaceMode:
                    break;
                case StylePropertyId.TextWrapMode:
                    break;
                case StylePropertyId.TextHorizontalOverflow:
                    break;
                case StylePropertyId.TextVerticalOverflow:
                    break;
                case StylePropertyId.TextIndentFirstLine:
                    break;
                case StylePropertyId.TextIndentNewLine:
                    break;
                case StylePropertyId.TextLayoutStyle:
                    break;
                case StylePropertyId.TextAutoSize:
                    break;

#endregion

#region Size

                case StylePropertyId.MinWidth:
                    MinWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

                case StylePropertyId.MaxWidth:
                    MaxWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : float.MaxValue;
                    break;

                case StylePropertyId.PreferredWidth:
                    PreferredWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : UIMeasurement.Auto;
                    break;

                case StylePropertyId.MinHeight:
                    MinHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

                case StylePropertyId.MaxHeight:
                    MaxHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : float.MaxValue;
                    break;

                case StylePropertyId.PreferredHeight:
                    PreferredHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : UIMeasurement.Content100;
                    break;

#endregion

                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyId), property.propertyId, null);
            }
        }

    }

}