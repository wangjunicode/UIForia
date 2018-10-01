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

        public static readonly LayoutWrap flexLayoutWrap = LayoutWrap.None;
        public static readonly LayoutDirection flexLayoutDirection = LayoutDirection.Row;
        public static readonly MainAxisAlignment flexLayoutMainAxisAlignment = MainAxisAlignment.Start;
        public static readonly CrossAxisAlignment flexLayoutCrossAxisAlignment = CrossAxisAlignment.Start;

        public static readonly UIMeasurement minWidth = new UIMeasurement(0);
        public static readonly UIMeasurement maxWidth = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement preferredWidth = UIMeasurement.Auto;
        public static readonly UIMeasurement minHeight = new UIMeasurement(0);
        public static readonly UIMeasurement maxHeight = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement preferredHeight = UIMeasurement.Content100;

        public static readonly UIMeasurement marginTop = new UIMeasurement(0);
        public static readonly UIMeasurement marginRight = new UIMeasurement(0);
        public static readonly UIMeasurement marginLeft = new UIMeasurement(0);
        public static readonly UIMeasurement marginBottom = new UIMeasurement(0);

        public static readonly UIMeasurement paddingTop = new UIMeasurement(0);
        public static readonly UIMeasurement paddingRight = new UIMeasurement(0);
        public static readonly UIMeasurement paddingLeft = new UIMeasurement(0);
        public static readonly UIMeasurement paddingBottom = new UIMeasurement(0);

        public static readonly UIMeasurement borderTop = new UIMeasurement(0);
        public static readonly UIMeasurement borderRight = new UIMeasurement(0);
        public static readonly UIMeasurement borderLeft = new UIMeasurement(0);
        public static readonly UIMeasurement borderBottom = new UIMeasurement(0);

        public static readonly UIMeasurement transformPositionX = new UIMeasurement(0);
        public static readonly UIMeasurement transformPositionY = new UIMeasurement(0);
        public static readonly UIMeasurement transformPivotX = new UIMeasurement(0);
        public static readonly UIMeasurement transformPivotY = new UIMeasurement(0);
        public static readonly float transformScaleX = 1;
        public static readonly float transformScaleY = 1;
        public static readonly float transformRotation = 0;

        public static readonly LayoutType layoutType = LayoutType.Flex;
        public static readonly int fontSize = 12;
        public static readonly Color textColor = Color.black;
        public static readonly TextUtil.FontStyle fontStyle = TextUtil.FontStyle.Normal;
        public static readonly AssetPointer<Font> fontAsset = new AssetPointer<Font>(AssetType.Font, 1);
        public static readonly TextUtil.TextAnchor textAnchor = TextUtil.TextAnchor.MiddleLeft;

    }

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
                set {
                    if (borderRadiusTopLeft == value) return;
                    borderRadiusTopLeft = value;
                    styleSet.styleSystem.SetBorderRadius(styleSet.element, borderRadius);
                }
            }

            public UIMeasurement BorderRadiusTopRight {
                get { return borderRadiusTopRight; }
                set {
                    if (borderRadiusTopRight == value) return;
                    borderRadiusTopRight = value;
                    styleSet.styleSystem.SetBorderRadius(styleSet.element, borderRadius);
                }
            }

            public UIMeasurement BorderRadiusBottomRight {
                get { return borderRadiusBottomRight; }
                set {
                    if (borderRadiusBottomRight == value) return;
                    borderRadiusBottomRight = value;
                    styleSet.styleSystem.SetBorderRadius(styleSet.element, borderRadius);
                }
            }

            public UIMeasurement BorderRadiusBottomLeft {
                get { return borderRadiusBottomLeft; }
                set {
                    if (borderRadiusBottomLeft == value) return;
                    borderRadiusBottomLeft = value;
                    styleSet.styleSystem.SetBorderRadius(styleSet.element, borderRadius);
                }
            }

        }

        private UIStyleSet styleSet;
        private RareStyleData rareData;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
        }

        public RareStyleData RareData => rareData ?? (rareData = new RareStyleData(styleSet));

        public ContentBoxRect border => new ContentBoxRect(borderTop, borderRight, borderBottom, borderLeft);
        public ContentBoxRect padding => new ContentBoxRect(paddingTop, paddingRight, paddingBottom, paddingLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, marginRight, marginBottom, marginLeft);

#region Paint

        private Color borderColor = DefaultStyleValues.borderColor;
        private Color backgroundColor = DefaultStyleValues.backgroundColor;
        private AssetPointer<Texture2D> backgroundImage = DefaultStyleValues.backgroundImage;

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

        private Overflow overflowX = DefaultStyleValues.overflowX;
        private Overflow overflowY = DefaultStyleValues.overflowY;

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

        private int flexGrowthFactor = DefaultStyleValues.flexGrowthFactor;
        private int flexShrinkFactor = DefaultStyleValues.flexShrinkFactor;
        private int flexOrderOverride = DefaultStyleValues.flexOrderOverride;
        private CrossAxisAlignment flexSelfAlignment = DefaultStyleValues.flexSelfAlignment;

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

        private LayoutWrap flexLayoutWrap = DefaultStyleValues.flexLayoutWrap;
        private LayoutDirection flexLayoutDirection = DefaultStyleValues.flexLayoutDirection;
        private MainAxisAlignment flexLayoutMainAxisAlignment = DefaultStyleValues.flexLayoutMainAxisAlignment;
        private CrossAxisAlignment flexLayoutCrossAxisAlignment = DefaultStyleValues.flexLayoutCrossAxisAlignment;

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

        private UIMeasurement minWidth = DefaultStyleValues.minWidth;
        private UIMeasurement maxWidth = DefaultStyleValues.maxWidth;
        private UIMeasurement preferredWidth = DefaultStyleValues.preferredWidth;

        private UIMeasurement minHeight = DefaultStyleValues.minHeight;
        private UIMeasurement maxHeight = DefaultStyleValues.maxHeight;
        private UIMeasurement preferredHeight = DefaultStyleValues.preferredHeight;

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

        private UIMeasurement marginTop = DefaultStyleValues.marginTop;
        private UIMeasurement marginRight = DefaultStyleValues.marginRight;
        private UIMeasurement marginBottom = DefaultStyleValues.marginBottom;
        private UIMeasurement marginLeft = DefaultStyleValues.marginLeft;

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

        private UIMeasurement borderTop = DefaultStyleValues.borderTop;
        private UIMeasurement borderRight = DefaultStyleValues.borderRight;
        private UIMeasurement borderBottom = DefaultStyleValues.borderBottom;
        private UIMeasurement borderLeft = DefaultStyleValues.borderLeft;

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

        private UIMeasurement paddingTop = DefaultStyleValues.paddingTop;
        private UIMeasurement paddingRight = DefaultStyleValues.paddingRight;
        private UIMeasurement paddingBottom = DefaultStyleValues.paddingBottom;
        private UIMeasurement paddingLeft = DefaultStyleValues.paddingLeft;

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

        private int fontSize = DefaultStyleValues.fontSize;
        private Color textColor = DefaultStyleValues.textColor;
        private AssetPointer<Font> fontAsset = DefaultStyleValues.fontAsset;
        private TextUtil.FontStyle fontStyle = DefaultStyleValues.fontStyle;
        private TextUtil.TextAnchor textAnchor = DefaultStyleValues.textAnchor;

        public Color TextColor {
            get { return textColor; }
            set {
                if (textColor == value) return;
                textColor = value;
                styleSet.styleSystem.SetFontColor(styleSet.element, textColor);
            }
        }

        public AssetPointer<Font> FontAsset {
            get { return fontAsset; }
            set {
                if (fontAsset.id == value.id) return;
                fontAsset = value;
                styleSet.styleSystem.SetFontAsset(styleSet.element, fontAsset);
            }
        }

        public int FontSize {
            get { return fontSize; }
            set {
                if (fontSize == value) return;
                fontSize = value;
                styleSet.styleSystem.SetFontSize(styleSet.element, fontSize);
            }
        }

        public TextUtil.FontStyle FontStyle {
            get { return fontStyle; }
            set {
                if (fontStyle == value) return;
                fontStyle = value;
                styleSet.styleSystem.SetFontStyle(styleSet.element, fontStyle);
            }
        }

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

        private UIMeasurement transformPositionX = DefaultStyleValues.transformPositionX;
        private UIMeasurement transformPositionY = DefaultStyleValues.transformPositionY;
        private UIMeasurement transformPivotX = DefaultStyleValues.transformPivotX;
        private UIMeasurement transformPivotY = DefaultStyleValues.transformPivotY;
        private float transformScaleX = DefaultStyleValues.transformScaleX;
        private float transformScaleY = DefaultStyleValues.transformScaleY;
        private float transformRotation = DefaultStyleValues.transformRotation;

        public UIMeasurement TransformPositionX {
            get { return transformPositionX; }
            set {
                if (value == UIMeasurement.Unset) value = DefaultStyleValues.transformPositionX;
                if (transformPositionX == value) return;
                transformPositionX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPositionY {
            get { return transformPositionY; }
            set {
                if (value == UIMeasurement.Unset) value = DefaultStyleValues.transformPositionY;
                if (transformPositionY == value) return;
                transformPositionY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPivotX {
            get { return transformPivotX; }
            set {
                if (value == UIMeasurement.Unset) value = DefaultStyleValues.transformPivotX;
                if (transformPivotX == value) return;
                transformPivotX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public UIMeasurement TransformPivotY {
            get { return transformPivotY; }
            set {
                if (value == UIMeasurement.Unset) value = DefaultStyleValues.transformPivotY;
                if (transformPivotY == value) return;
                transformPivotY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformScaleX {
            get { return transformScaleX; }
            set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformScaleX;
                if (Mathf.Approximately(value, transformScaleX)) return;
                transformScaleX = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformScaleY {
            get { return transformScaleY; }
            set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformScaleY;
                if (Mathf.Approximately(value, transformScaleY)) return;
                transformScaleY = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public float TransformRotation {
            get { return transformRotation; }
            set {
                if (!FloatUtil.IsDefined(value)) value = DefaultStyleValues.transformRotation;
                if (Mathf.Approximately(value, transformRotation)) return;
                transformRotation = value;
                styleSet.styleSystem.SetTransform(styleSet.element, new UITransform());
            }
        }

        public MeasurementVector2 TransformPosition {
            get { return new MeasurementVector2(transformPositionX, transformPositionY); }
            set {
                TransformPositionX = value.x;
                TransformPositionY = value.y;
            }
        }

#region Layout

        private LayoutType layoutType = DefaultStyleValues.layoutType;

        public LayoutType LayoutType {
            get { return layoutType; }
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
                    BackgroundImage = property.IsDefined ? new AssetPointer<Texture2D>((AssetType) value0, value1) : DefaultStyleValues.backgroundImage;
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
                    BorderTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderTop;
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderRight;
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderBottom;
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.borderLeft;
                    break;

#endregion

#region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.paddingTop;
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.paddingRight;
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.paddingBottom;
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.paddingLeft;
                    break;

#endregion

#region Transform

                case StylePropertyId.TransformPositionX:
                    TransformPositionX = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.transformPositionX;
                    break;
                case StylePropertyId.TransformPositionY:
                    TransformPositionY = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.transformPositionY;
                    break;
                case StylePropertyId.TransformScaleX:
                    TransformScaleX = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.transformScaleX;
                    break;
                case StylePropertyId.TransformScaleY:
                    TransformScaleY = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.transformScaleY;
                    break;
                case StylePropertyId.TransformPivotX:
                    transformPivotX = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.transformPivotX;
                    break;
                case StylePropertyId.TransformPivotY:
                    TransformPivotY = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.transformPivotY;
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
                    FontAsset = property.IsDefined ? new AssetPointer<Font>((AssetType) value0, value1) : DefaultStyleValues.fontAsset;
                    break;
                case StylePropertyId.TextFontSize:
                    FontSize = property.IsDefined ? value0 : DefaultStyleValues.fontSize;
                    break;
                case StylePropertyId.TextFontStyle:
                    FontStyle = property.IsDefined ? (TextUtil.FontStyle) value0 : DefaultStyleValues.fontStyle;
                    break;
                case StylePropertyId.TextAnchor:
                    TextAnchor = property.IsDefined ? (TextUtil.TextAnchor) value0 : DefaultStyleValues.textAnchor;
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

                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyId), property.propertyId, null);
            }
        }

    }

}