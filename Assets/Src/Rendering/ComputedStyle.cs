using System;
using Src;
using Src.Layout;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public static class DefaultStyleValues {

        public static readonly UIMeasurement borderRadiusTopLeft = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusTopRight = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusBottomRight = new UIMeasurement(0);
        public static readonly UIMeasurement borderRadiusBottomLeft = new UIMeasurement(0);

        public static readonly Color borderColor = ColorUtil.UnsetValue;
        public static readonly Color backgroundColor = ColorUtil.UnsetValue;
        public static readonly AssetPointer<Texture2D> backgroundImage = new AssetPointer<Texture2D>(AssetType.Texture, -1);
        
        public static readonly Overflow overflowX = Overflow.None;
        public static readonly Overflow overflowY = Overflow.None;

        public static readonly int flexOrderOverride = 999;
        public static readonly int flexGrowthFactor = 0;
        public static readonly int flexShrinkFactor = 0;
        public static readonly CrossAxisAlignment flexSelfAlignment = CrossAxisAlignment.Default;
        
        public static readonly LayoutDirection flexLayoutDirection = LayoutDirection.Row;
        public static readonly LayoutWrap flexLayoutWrap = LayoutWrap.None;
        public static readonly MainAxisAlignment flexLayoutMainAxisAlignment = MainAxisAlignment.Start;
        public static readonly CrossAxisAlignment flexLayoutCrossAxisAlignment = CrossAxisAlignment.Start;
        
    }

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
        
        private UIStyleSet styleSet;
        private RareStyleData rareData;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public RareStyleData RareData => rareData ?? (rareData = new RareStyleData());

        public ContentBoxRect border => new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft);
        public ContentBoxRect padding => new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, marginRight, marginBottom, marginLeft);

#region Paint

        private Color borderColor;
        private Color backgroundColor;
        private AssetPointer<Texture2D> backgroundImage;

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

#endregion

#region Overflow

        private Overflow overflowX;
        private Overflow overflowY;

        public Overflow OverflowX {
            get { return overflowX; }
            set {
                if (value == overflowX) return;
                overflowX = value;
                styleSet.styleSystem.SetOverflowX(styleSet.element, overflowX);
            }
        }
        
        public Overflow OverflowY {
            get { return overflowY; }
            set {
                if (value == overflowY) return;
                overflowY = value;
                styleSet.styleSystem.SetOverflowY(styleSet.element, overflowY);
            }
        }

#endregion

#region Flex Item 

        private int flexOrderOverride;
        private int flexGrowthFactor;
        private int flexShrinkFactor;
        private CrossAxisAlignment flexSelfAlignment;

        public int FlexItemOrder {
            get { return flexOrderOverride; }
            set {
                if (value < 0) value = 0;
                if (flexOrderOverride == value) return;
                flexOrderOverride = value;
                styleSet.styleSystem.SetFlexItemProperties(styleSet.element);
            }
        }

        public int FlexItemGrowthFactor {
            get { return flexGrowthFactor; }
            set {
                if (value < 0) value = 0;
                if (flexGrowthFactor == value) return;
                flexGrowthFactor = value;
                styleSet.styleSystem.SetFlexItemProperties(styleSet.element);
            }
        }

        public int FlexItemShrinkFactor {
            get { return flexShrinkFactor; }
            set {
                if (value < 0) value = 0;
                if (flexShrinkFactor == value) return;
                flexShrinkFactor = value;
                styleSet.styleSystem.SetFlexItemProperties(styleSet.element);
            }
        }

        public CrossAxisAlignment FlexItemSelfAlignment {
            get { return flexSelfAlignment; }
            set {
                if (value == flexSelfAlignment) return;
                flexSelfAlignment = value;
                styleSet.styleSystem.SetFlexItemProperties(styleSet.element);
            }
        }

#endregion

#region Flex Layout

        private LayoutDirection flexLayoutDirection;
        private LayoutWrap flexLayoutWrap;
        private MainAxisAlignment flexLayoutMainAxisAlignment;
        private CrossAxisAlignment flexLayoutCrossAxisAlignment;

        public LayoutDirection FlexLayoutDirection {
            get { return flexLayoutDirection; }
            set {
                if (flexLayoutDirection == value) return;
                flexLayoutDirection = value;
                styleSet.styleSystem.SetLayoutDirection(styleSet.element, flexLayoutDirection);
            }
        }

        public LayoutWrap FlexLayoutWrap {
            get { return flexLayoutWrap; }
            set {
                if (flexLayoutWrap == value) return;
                flexLayoutWrap = value;
                styleSet.styleSystem.SetLayoutWrap(styleSet.element, flexLayoutWrap);
            }
        }

        public MainAxisAlignment FlexLayoutMainAxisAlignment {
            get { return flexLayoutMainAxisAlignment; }
            set {
                if (flexLayoutMainAxisAlignment == value) return;
                MainAxisAlignment oldAlignment = flexLayoutMainAxisAlignment;
                flexLayoutMainAxisAlignment = value;
                styleSet.styleSystem.SetMainAxisAlignment(styleSet.element, flexLayoutMainAxisAlignment, oldAlignment);
            }
        }

        public CrossAxisAlignment FlexLayoutCrossAxisAlignment {
            get { return flexLayoutCrossAxisAlignment; }
            set {
                if (flexLayoutCrossAxisAlignment == value) return;
                CrossAxisAlignment oldAlignment = flexLayoutCrossAxisAlignment;
                flexLayoutCrossAxisAlignment = value;
                styleSet.styleSystem.SetCrossAxisAlignment(styleSet.element, flexLayoutCrossAxisAlignment, oldAlignment);
            }
        }

#endregion

#region Grid Item

        

#endregion

#region Grid Layout

        

#endregion

#region Size       

        private UIMeasurement minWidth;
        private UIMeasurement maxWidth;
        private UIMeasurement preferredWidth;

        private UIMeasurement minHeight;
        private UIMeasurement maxHeight;
        private UIMeasurement preferredHeight;

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

#endregion
        
#region Margin

        private UIMeasurement marginTop;
        private UIMeasurement marginRight;
        private UIMeasurement marginBottom;
        private UIMeasurement marginLeft;

        public UIMeasurement MarginTop {
            get { return marginTop; }
            set {
                if (marginTop == value) return;
                marginTop = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        public UIMeasurement MarginRight {
            get { return marginRight; }
            set {
                if (marginRight == value) return;
                marginRight = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        public UIMeasurement MarginBottom {
            get { return marginBottom; }
            set {
                if (marginBottom == value) return;
                marginBottom = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

        public UIMeasurement MarginLeft {
            get { return marginLeft; }
            set {
                if (marginLeft == value) return;
                marginLeft = value;
                styleSet.styleSystem.SetMargin(styleSet.element, margin);
            }
        }

#endregion

#region Border

        private UIMeasurement borderTop;
        private UIMeasurement borderRight;
        private UIMeasurement borderBottom;
        private UIMeasurement borderLeft;

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

#endregion

#region Padding

        private UIMeasurement paddingTop;
        private UIMeasurement paddingRight;
        private UIMeasurement paddingBottom;
        private UIMeasurement paddingLeft;

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

#endregion

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

        public MeasurementVector2 TransformPosition {
            get {
                return new MeasurementVector2(transformPositionX, transformPositionY);
            }
            set {
                TransformPositionX = value.x;
                TransformPositionY = value.y;
            }
        }

#region Layout

        private LayoutType layoutType;

        public LayoutType LayoutType {
            get { return layoutType;}
            set {
                if (layoutType == value) return;
                layoutType = value;
                styleSet.styleSystem.SetLayoutType(styleSet.element, layoutType);
            }
        }
        

#endregion

#endregion

        internal void SetProperty(StyleProperty property) {
            int value0 = property.valuePart0;
            int value1 = property.valuePart1;
            switch (property.propertyId) {
#region Overflow

                case StylePropertyId.OverflowX:
                    overflowX = (Overflow) value0;
                    break;
                case StylePropertyId.OverflowY:
                    overflowY = (Overflow) value0;
                    break;

#endregion

#region Paint

                case StylePropertyId.BackgroundColor:
                    BackgroundColor = property.IsDefined ? (Color) new StyleColor(value0) : ColorUtil.UnsetValue;
                    break;
                case StylePropertyId.BorderColor:
                    BorderColor = property.IsDefined ? (Color) new StyleColor(value0) : ColorUtil.UnsetValue;
                    break;
                case StylePropertyId.BackgroundImage:
                    BackgroundImage = property.IsDefined
                        ? new AssetPointer<Texture2D>((AssetType) value0, value1)
                        : new AssetPointer<Texture2D>(AssetType.Texture, IntUtil.UnsetValue);
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
                    FlexLayoutWrap = property.IsDefined ? (LayoutWrap) value0 : LayoutWrap.None;
                    break;
                case StylePropertyId.FlexDirection:
                    FlexLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : LayoutDirection.Column;
                    break;
                case StylePropertyId.FlexMainAxisAlignment:
                    FlexLayoutMainAxisAlignment = property.IsDefined ? (MainAxisAlignment) value0 : MainAxisAlignment.Start;
                    break;
                case StylePropertyId.FlexCrossAxisAlignment:
                    FlexLayoutCrossAxisAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : CrossAxisAlignment.Start;
                    break;

#endregion

#region Flex Item

                case StylePropertyId.FlexItemSelfAlignment:
                    FlexItemSelfAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : CrossAxisAlignment.Start;
                    break;
                case StylePropertyId.FlexItemOrder:
                    FlexItemOrder = property.IsDefined ? value0 : 999;
                    break;

                case StylePropertyId.FlexItemGrow:
                    FlexItemGrowthFactor = property.IsDefined ? value0 : 0;
                    break;
                case StylePropertyId.FlexItemShrink:
                    FlexItemShrinkFactor = property.IsDefined ? value0 : 0;
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
                    BorderTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

#endregion

#region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;

#endregion

#region Transform

                case StylePropertyId.TransformPositionX:
                    TransformPositionX = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.TransformPositionY:
                    TransformPositionY = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.TransformScaleX:
                    TransformScaleX = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : 1;
                    break;
                case StylePropertyId.TransformScaleY:
                    TransformScaleY = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : 1;
                    break;
                case StylePropertyId.TransformPivotX:
                    transformPivotX = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.TransformPivotY:
                    TransformPivotY = property.IsDefined ? UIMeasurement.Decode(value0, value1) : 0;
                    break;
                case StylePropertyId.TransformRotation:
                    TransformRotation = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : 1;
                    break;

#endregion

#region Text

                case StylePropertyId.TextColor:
                    TextColor = property.IsDefined ? (Color) new StyleColor(value0) : Color.black;
                    break;
                case StylePropertyId.TextFontAsset:
                    FontAsset = property.IsDefined
                        ? new AssetPointer<Font>((AssetType) value0, value1)
                        : new AssetPointer<Font>(AssetType.Font, IntUtil.UnsetValue);
                    break;
                case StylePropertyId.TextFontSize:
                    FontSize = property.IsDefined
                        ? value0
                        : 12;
                    break;
                case StylePropertyId.TextFontStyle:
                    FontStyle = property.IsDefined ? (TextUtil.FontStyle) value0 : TextUtil.FontStyle.Normal;
                    break;
                case StylePropertyId.TextAnchor:
                    TextAnchor = property.IsDefined ? (TextUtil.TextAnchor) value0 : TextUtil.TextAnchor.MiddleLeft;
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