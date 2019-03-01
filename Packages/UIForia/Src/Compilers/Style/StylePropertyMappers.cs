using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;

namespace UIForia.Compilers.Style {
    public struct StylePropertyMappers {

        private static readonly Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> mappers
            = new Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> {
                // Overflow
                {"overflow", (targetStyle, property, context) => MapOverflows(targetStyle, property, context)},
                {"overflowx", (targetStyle, property, context) => targetStyle.OverflowX = MapEnum<Overflow>(property.children[0], context)},
                {"overflowy", (targetStyle, property, context) => targetStyle.OverflowY = MapEnum<Overflow>(property.children[0], context)},

                // Background
                {"backgroundcolor", (targetStyle, property, context) => targetStyle.BackgroundColor = MapColor(property, context)},
                {"backgroundimageoffsetx", (targetStyle, property, context) => targetStyle.BackgroundImageOffsetX = MapFixedLength(property.children[0], context)},
                {"backgroundimageoffsety", (targetStyle, property, context) => targetStyle.BackgroundImageOffsetY = MapFixedLength(property.children[0], context)},
                {"backgroundimagescalex", (targetStyle, property, context) => targetStyle.BackgroundImageScaleX = MapFixedLength(property.children[0], context)},
                {"backgroundimagescaley", (targetStyle, property, context) => targetStyle.BackgroundImageScaleY = MapFixedLength(property.children[0], context)},
                {"backgroundimagetilex", (targetStyle, property, context) => targetStyle.BackgroundImageTileX = MapFixedLength(property.children[0], context)},
                {"backgroundimagetiley", (targetStyle, property, context) => targetStyle.BackgroundImageTileY = MapFixedLength(property.children[0], context)},
                {"backgroundimagerotation", (targetStyle, property, context) => targetStyle.BackgroundImageRotation = MapFixedLength(property.children[0], context)},
                {"backgroundimage", (targetStyle, property, context) => targetStyle.BackgroundImage = MapTexture(property.children[0], context)},
                
                {"visibility", (targetStyle, property, context) => targetStyle.Visibility = MapEnum<Visibility>(property.children[0], context)},
                {"opacity", (targetStyle, property, context) => targetStyle.Opacity = MapNumber(property.children[0], context)},
                {"cursor", (targetStyle, property, context) => targetStyle.Cursor = MapCursor(property, context)},

                {"margin", (targetStyle, valueParts, context) => MapMargins(targetStyle, valueParts, context)},
                {"margintop", (targetStyle, property, context) => targetStyle.MarginTop = MapMeasurement(property.children[0], context)},
                {"marginright", (targetStyle, property, context) => targetStyle.MarginRight = MapMeasurement(property.children[0], context)},
                {"marginbottom", (targetStyle, property, context) => targetStyle.MarginBottom = MapMeasurement(property.children[0], context)},
                {"marginleft", (targetStyle, property, context) => targetStyle.MarginLeft = MapMeasurement(property.children[0], context)},

                {"padding", (targetStyle, valueParts, context) => MapPaddings(targetStyle, valueParts, context)},
                {"paddingtop", (targetStyle, property, context) => targetStyle.PaddingTop = MapFixedLength(property.children[0], context)},
                {"paddingright", (targetStyle, property, context) => targetStyle.PaddingRight = MapFixedLength(property.children[0], context)},
                {"paddingbottom", (targetStyle, property, context) => targetStyle.PaddingBottom = MapFixedLength(property.children[0], context)},
                {"paddingleft", (targetStyle, property, context) => targetStyle.PaddingLeft = MapFixedLength(property.children[0], context)},

                {"bordercolor", (targetStyle, property, context) => targetStyle.BorderColor = MapColor(property, context)},

                {"border", (targetStyle, property, context) => MapBorders(targetStyle, property, context)},
                {"bordertop", (targetStyle, property, context) => targetStyle.BorderTop = MapFixedLength(property.children[0], context)},
                {"borderright", (targetStyle, property, context) => targetStyle.BorderRight = MapFixedLength(property.children[0], context)},
                {"borderbottom", (targetStyle, property, context) => targetStyle.BorderBottom = MapFixedLength(property.children[0], context)},
                {"borderleft", (targetStyle, property, context) => targetStyle.BorderLeft = MapFixedLength(property.children[0], context)},

                {"borderradius", (targetStyle, property, context) => MapBorderRadius(targetStyle, property, context)},
                {"borderradiustopleft", (targetStyle, property, context) => targetStyle.BorderRadiusTopLeft = MapFixedLength(property.children[0], context)},
                {"borderradiustopright", (targetStyle, property, context) => targetStyle.BorderRadiusTopRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomright", (targetStyle, property, context) => targetStyle.BorderRadiusBottomRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomleft", (targetStyle, property, context) => targetStyle.BorderRadiusBottomLeft = MapFixedLength(property.children[0], context)},

                {"griditemcolstart", (targetStyle, property, context) => targetStyle.GridItemColStart = (int) MapNumber(property.children[0], context)},
                {"griditemcolspan", (targetStyle, property, context) => targetStyle.GridItemColSpan = (int) MapNumber(property.children[0], context)},
                {"griditemrowstart", (targetStyle, property, context) => targetStyle.GridItemRowStart = (int) MapNumber(property.children[0], context)},
                {"griditemrowspan", (targetStyle, property, context) => targetStyle.GridItemRowSpan = (int) MapNumber(property.children[0], context)},
                {"griditemcolselfalignment", (targetStyle, property, context) => targetStyle.GridItemColSelfAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"griditemrowselfalignment", (targetStyle, property, context) => targetStyle.GridItemRowSelfAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutcolalignment", (targetStyle, property, context) => targetStyle.GridLayoutColAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutrowalignment", (targetStyle, property, context) => targetStyle.GridLayoutRowAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutdensity", (targetStyle, property, context) => targetStyle.GridLayoutDensity = MapEnum<GridLayoutDensity>(property.children[0], context)},
                {"gridlayoutcoltemplate", (targetStyle, property, context) => targetStyle.GridLayoutColTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutrowtemplate", (targetStyle, property, context) => targetStyle.GridLayoutRowTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutdirection", (targetStyle, property, context) => targetStyle.GridLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"gridlayoutmainaxisautosize", (targetStyle, property, context) => targetStyle.GridLayoutMainAxisAutoSize = MapGridTrackSize(property.children[0], context)},
                {"gridlayoutcrossaxisautosize", (targetStyle, property, context) => targetStyle.GridLayoutCrossAxisAutoSize = MapGridTrackSize(property.children[0], context)},
                {"gridlayoutcolgap", (targetStyle, property, context) => targetStyle.GridLayoutColGap = MapNumber(property.children[0], context)},
                {"gridlayoutrowgap", (targetStyle, property, context) => targetStyle.GridLayoutRowGap = MapNumber(property.children[0], context)},

                {"flexitemselfalignment", (targetStyle, property, context) => targetStyle.FlexItemSelfAlignment = MapEnum<CrossAxisAlignment>(property.children[0], context)},
                {"flexitemorder", (targetStyle, property, context) => targetStyle.FlexItemOrder = (int) MapNumber(property.children[0], context)},
                {"flexitemgrow", (targetStyle, property, context) => targetStyle.FlexItemGrow = (int) MapNumber(property.children[0], context)},
                {"flexitemshrink", (targetStyle, property, context) => targetStyle.FlexItemShrink = (int) MapNumber(property.children[0], context)},
                {"flexlayoutwrap", (targetStyle, property, context) => targetStyle.FlexLayoutWrap = MapEnum<LayoutWrap>(property.children[0], context)},
                {"flexlayoutdirection", (targetStyle, property, context) => targetStyle.FlexLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"flexlayoutmainaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutMainAxisAlignment = MapEnum<MainAxisAlignment>(property.children[0], context)},
                {"flexlayoutcrossaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutCrossAxisAlignment = MapEnum<CrossAxisAlignment>(property.children[0], context)},

                {"transformposition", (targetStyle, property, context) => MapTransformPosition(targetStyle, property, context)},
                {"transformpositionx", (targetStyle, property, context) => targetStyle.TransformPositionX = MapTransformOffset(property.children[0], context)},
                {"transformpositiony", (targetStyle, property, context) => targetStyle.TransformPositionY = MapTransformOffset(property.children[0], context)},
                {"transformscale", (targetStyle, property, context) => MapTransformScale(targetStyle, property, context)},
                {"transformscalex", (targetStyle, property, context) => targetStyle.TransformScaleX = MapNumber(property.children[0], context)},
                {"transformscaley", (targetStyle, property, context) => targetStyle.TransformScaleY = MapNumber(property.children[0], context)},
                {"transformpivot", (targetStyle, property, context) => MapTransformPivot(targetStyle, property, context)},
                {"transformpivotx", (targetStyle, property, context) => targetStyle.TransformPivotX = MapFixedLength(property.children[0], context)},
                {"transformpivoty", (targetStyle, property, context) => targetStyle.TransformPivotY = MapFixedLength(property.children[0], context)},
                {"transformrotation", (targetStyle, property, context) => targetStyle.TransformRotation = MapNumber(property.children[0], context)},
                {"transformbehavior", (targetStyle, property, context) => MapTransformBehavior(targetStyle, property, context)},
                {"transformbehaviorx", (targetStyle, property, context) => targetStyle.TransformBehaviorX = MapEnum<TransformBehavior>(property.children[0], context)},
                {"transformbehaviory", (targetStyle, property, context) => targetStyle.TransformBehaviorY = MapEnum<TransformBehavior>(property.children[0], context)},

                {"minwidth", (targetStyle, property, context) => targetStyle.MinWidth = MapMeasurement(property.children[0], context)},
                {"minheight", (targetStyle, property, context) => targetStyle.MinHeight = MapMeasurement(property.children[0], context)},
                {"preferredwidth", (targetStyle, property, context) => targetStyle.PreferredWidth = MapMeasurement(property.children[0], context)},
                {"preferredheight", (targetStyle, property, context) => targetStyle.PreferredHeight = MapMeasurement(property.children[0], context)},
                {"maxwidth", (targetStyle, property, context) => targetStyle.MaxWidth = MapMeasurement(property.children[0], context)},
                {"maxheight", (targetStyle, property, context) => targetStyle.MaxHeight = MapMeasurement(property.children[0], context)},
                {"preferredsize", (targetStyle, property, context) => MapPreferredSize(targetStyle, property, context)},
                {"minsize", (targetStyle, property, context) => MapMinSize(targetStyle, property, context)},
                {"maxsize", (targetStyle, property, context) => MapMaxSize(targetStyle, property, context)},

                {"layouttype", (targetStyle, property, context) => targetStyle.LayoutType = MapEnum<LayoutType>(property.children[0], context)},
                {"layoutbehavior", (targetStyle, property, context) => targetStyle.LayoutBehavior = MapEnum<LayoutBehavior>(property.children[0], context)},
                {"anchortarget", (targetStyle, property, context) => targetStyle.AnchorTarget = MapEnum<AnchorTarget>(property.children[0], context)},
                {"anchortop", (targetStyle, property, context) => targetStyle.AnchorTop = MapFixedLength(property.children[0], context)},
                {"anchorright", (targetStyle, property, context) => targetStyle.AnchorRight = MapFixedLength(property.children[0], context)},
                {"anchorbottom", (targetStyle, property, context) => targetStyle.AnchorBottom = MapFixedLength(property.children[0], context)},
                {"anchorleft", (targetStyle, property, context) => targetStyle.AnchorLeft = MapFixedLength(property.children[0], context)},
                {"zindex", (targetStyle, property, context) => targetStyle.ZIndex = (int) MapNumber(property.children[0], context)},
                {"renderlayer", (targetStyle, property, context) => targetStyle.RenderLayer = MapEnum<RenderLayer>(property.children[0], context)},
                {"renderlayeroffset", (targetStyle, property, context) => targetStyle.RenderLayerOffset = (int) MapNumber(property.children[0], context)},

                // Text
                {"textcolor", (targetStyle, property, context) => targetStyle.TextColor = MapColor(property, context)},
                {"textfontasset", (targetStyle, property, context) => targetStyle.TextFontAsset = MapFont(property.children[0], context)},
                {"textfontstyle", (targetStyle, property, context) => targetStyle.TextFontStyle = MapTextFontStyle(property, context)},
                {"textfontsize", (targetStyle, property, context) => targetStyle.TextFontSize = (int) MapNumber(property.children[0], context)},
                {"textalignment", (targetStyle, property, context) => targetStyle.TextAlignment = MapEnum<UIForia.Text.TextAlignment>(property.children[0], context)},
                {"textoutlinewidth", (targetStyle, property, context) => targetStyle.TextOutlineWidth = MapNumber(property.children[0], context)},
                {"textoutlinecolor", (targetStyle, property, context) => targetStyle.TextOutlineColor = MapColor(property, context)},
                {"textglowcolor", (targetStyle, property, context) => targetStyle.TextGlowColor = MapColor(property, context)},
                {"textglowoffset", (targetStyle, property, context) => targetStyle.TextGlowOffset = MapNumber(property.children[0], context)},
                {"textglowinner", (targetStyle, property, context) => targetStyle.TextGlowInner = MapNumber(property.children[0], context)},
                {"textglowouter", (targetStyle, property, context) => targetStyle.TextGlowOuter = MapNumber(property.children[0], context)},
                {"textglowpower", (targetStyle, property, context) => targetStyle.TextGlowPower = MapNumber(property.children[0], context)},
                {"textshadowcolor", (targetStyle, property, context) => targetStyle.TextShadowColor = MapColor(property, context)},
                {"textshadowoffsetx", (targetStyle, property, context) => targetStyle.TextShadowOffsetX = MapNumber(property.children[0], context)},
                {"textshadowoffsety", (targetStyle, property, context) => targetStyle.TextShadowOffsetY = MapNumber(property.children[0], context)},
                {"textshadowintensity", (targetStyle, property, context) => targetStyle.TextShadowIntensity = MapNumber(property.children[0], context)},
                {"textshadowsoftness", (targetStyle, property, context) => targetStyle.TextShadowSoftness = MapNumber(property.children[0], context)},
                {"textshadowtype", (targetStyle, property, context) => targetStyle.TextShadowType = MapEnum<ShadowType>(property.children[0], context)},
                {"texttransform", (targetStyle, property, context) => targetStyle.TextTransform = MapEnum<TextTransform>(property.children[0], context)},

                {"painter", (targetStyle, property, context) => targetStyle.Painter = MapString(property.children[0], context)},

                // Scrollbar
                {"scrollbar", (targetStyle, property, context) => targetStyle.Scrollbar = MapString(property.children[0], context)},
                {"scrollbarsize", (targetStyle, property, context) => targetStyle.ScrollbarSize = MapMeasurement(property.children[0], context)},
                {"scrollbarcolor", (targetStyle, property, context) => targetStyle.ScrollbarColor = MapColor(property, context)},
                
                // shadows for things
                {"shadowtype", (targetStyle, property, context) => targetStyle.ShadowType = MapEnum<ShadowType>(property.children[0], context)},
                {"shadowoffsetx", (targetStyle, property, context) => targetStyle.ShadowOffsetX = MapNumber(property.children[0], context)},
                {"shadowoffsety", (targetStyle, property, context) => targetStyle.ShadowOffsetY = MapNumber(property.children[0], context)},
                {"shadowsoftnessx", (targetStyle, property, context) => targetStyle.ShadowSoftnessX = MapNumber(property.children[0], context)},
                {"shadowsoftnessy", (targetStyle, property, context) => targetStyle.ShadowSoftnessY = MapNumber(property.children[0], context)},
                {"shadowintensity", (targetStyle, property, context) => targetStyle.ShadowIntensity = MapNumber(property.children[0], context)},
            };

        private static FontStyle MapTextFontStyle(PropertyNode property, StyleCompileContext context) {
            Text.FontStyle style = Text.FontStyle.Normal;

            foreach (StyleASTNode value in property.children) {
                StyleASTNode resolvedValue = context.GetValueForReference(value);
                switch (resolvedValue) {
                    case StyleIdentifierNode identifierNode:

                        string propertyValue = identifierNode.name.ToLower();

                        if (propertyValue.Contains("bold")) {
                            style |= Text.FontStyle.Bold;
                        }

                        if (propertyValue.Contains("italic")) {
                            style |= Text.FontStyle.Italic;
                        }

                        if (propertyValue.Contains("highlight")) {
                            style |= Text.FontStyle.Highlight;
                        }

                        if (propertyValue.Contains("smallcaps")) {
                            style |= Text.FontStyle.SmallCaps;
                        }

                        if (propertyValue.Contains("superscript")) {
                            style |= Text.FontStyle.Superscript;
                        }

                        if (propertyValue.Contains("subscript")) {
                            style |= Text.FontStyle.Subscript;
                        }

                        if (propertyValue.Contains("underline")) {
                            style |= Text.FontStyle.Underline;
                        }

                        if (propertyValue.Contains("strikethrough")) {
                            style |= Text.FontStyle.StrikeThrough;
                        }

                        break;
                    default: throw new CompileException(value, $"Invalid TextFontStyle {value}. " +
                           "Make sure you use one of those: bold, italic, highlight, smallcaps, superscript, subscript, underline or strikethrough.");
                }
            }

            if ((style & Text.FontStyle.Superscript) != 0 && (style & Text.FontStyle.Subscript) != 0) {
                throw new CompileException(property, "Font style cannot be both superscript and subscript");
            }

            return style;
        }

        private static void MapMaxSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.MaxWidth = x;
            targetStyle.MaxHeight = y;
        }

        private static void MapMinSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.MinWidth = x;
            targetStyle.MinHeight = y;
        }

        private static void MapPreferredSize(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIMeasurement x = MapMeasurement(property.children[0], context);
            UIMeasurement y = x;
            if (property.children.Count > 1) {
                y = MapMeasurement(property.children[1], context);
            }

            targetStyle.PreferredWidth = x;
            targetStyle.PreferredHeight = y;
        }

        private static CursorStyle MapCursor(PropertyNode property, StyleCompileContext context) {
            float hotSpotX = 0;
            float hotSpotY = 0;
            if (property.children.Count > 1) {
                hotSpotX = MapNumber(property.children[1], context);
                if (property.children.Count > 2) {
                    hotSpotY = MapNumber(property.children[2], context);
                }
                else {
                    hotSpotY = hotSpotX;
                }
            }

            return new CursorStyle(null, MapTexture(property.children[0], context), new Vector2(hotSpotX, hotSpotY));
        }

        private static void MapTransformBehavior(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            TransformBehavior x = MapEnum<TransformBehavior>(property.children[0], context);
            TransformBehavior y = x;
            if (property.children.Count > 1) {
                y = MapEnum<TransformBehavior>(property.children[1], context);
            }

            targetStyle.TransformBehaviorX = x;
            targetStyle.TransformBehaviorY = y;
        }

        private static void MapTransformScale(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            float x = MapNumber(property.children[0], context);
            float y = x;
            if (property.children.Count > 1) {
                y = MapNumber(property.children[1], context);
            }

            targetStyle.TransformScaleX = x;
            targetStyle.TransformScaleY = y;
        }

        private static void MapTransformPivot(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength x = MapFixedLength(property.children[0], context);
            UIFixedLength y = x;
            if (property.children.Count > 1) {
                y = MapFixedLength(property.children[1], context);
            }

            targetStyle.TransformPivotX = x;
            targetStyle.TransformPivotY = y;
        }

        private static void MapTransformPosition(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            TransformOffset x = MapTransformOffset(property.children[0], context);
            TransformOffset y = x;
            if (property.children.Count > 1) {
                y = MapTransformOffset(property.children[1], context);
            }

            targetStyle.TransformPositionX = x;
            targetStyle.TransformPositionY = y;
        }

        private static IReadOnlyList<GridTrackSize> MapGridLayoutTemplate(PropertyNode propertyNode, StyleCompileContext context) {
            LightList<GridTrackSize> gridTrackSizes = LightListPool<GridTrackSize>.Get();
            foreach (StyleASTNode trackSize in propertyNode.children) {
                gridTrackSizes.Add(MapGridTrackSize(trackSize, context));
            }

            return gridTrackSizes;
        }

        private static GridTrackSize MapGridTrackSize(StyleASTNode trackSize, StyleCompileContext context) {
            StyleASTNode dereferencedValue = context.GetValueForReference(trackSize);
            switch (dereferencedValue) {
                case StyleLiteralNode literalNode:
                    if (literalNode.type == StyleASTNodeType.StringLiteral && literalNode.rawValue.ToLower() == "auto") {
                        // todo revisit this default value and replace this with something else like 1mx? 
                        return GridTrackSize.Unset;
                    }
                    else if (literalNode.type == StyleASTNodeType.NumericLiteral && TryParseFloat(literalNode.rawValue, out float number)) {
                        return new GridTrackSize(number);
                    }

                    throw new CompileException(literalNode, $"Could not create a grid track size out of the value {literalNode}.");

                case MeasurementNode measurementNode:
                    GridTemplateUnit unit = MapGridTemplateUnit(measurementNode.unit);
                    float value = MapNumber(measurementNode.value, context);
                    return new GridTrackSize(value, unit);

                default:
                    throw new CompileException(trackSize, $"Had a hard time parsing that track size: {trackSize}.");
            }
        }

        private static void MapBorders(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value1;
                targetStyle.BorderBottom = value1;
                targetStyle.BorderLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value1;
                targetStyle.BorderLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value3;
                targetStyle.BorderLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.BorderTop = value1;
                targetStyle.BorderRight = value2;
                targetStyle.BorderBottom = value3;
                targetStyle.BorderLeft = value4;
            }
        }

        private static void MapBorderRadius(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value1;
                targetStyle.BorderRadiusBottomRight = value1;
                targetStyle.BorderRadiusBottomLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value1;
                targetStyle.BorderRadiusBottomLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value3;
                targetStyle.BorderRadiusBottomLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.BorderRadiusTopLeft = value1;
                targetStyle.BorderRadiusTopRight = value2;
                targetStyle.BorderRadiusBottomRight = value3;
                targetStyle.BorderRadiusBottomLeft = value4;
            }
        }

        private static string EnumValues(Type type) {
            return $"[{string.Join(", ", Enum.GetNames(type))}]";
        }

        private static void MapMargins(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            // We support all css notations here and accept, 1, 2, 3 and 4 values

            // - 1 value sets all 4 margins
            // - 2 values: first value sets top and bottom, second value sets left and right
            // - 3 values: first values sets top, 2nd sets left and right, 3rd sets bottom
            // - 4 values set all 4 margins from top to left, clockwise

            UIMeasurement value1 = MapMeasurement(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value1;
                targetStyle.MarginBottom = value1;
                targetStyle.MarginLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIMeasurement value2 = MapMeasurement(property.children[1], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value1;
                targetStyle.MarginLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIMeasurement value2 = MapMeasurement(property.children[1], context);
                UIMeasurement value3 = MapMeasurement(property.children[2], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value3;
                targetStyle.MarginLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIMeasurement value2 = MapMeasurement(property.children[1], context);
                UIMeasurement value3 = MapMeasurement(property.children[2], context);
                UIMeasurement value4 = MapMeasurement(property.children[3], context);
                targetStyle.MarginTop = value1;
                targetStyle.MarginRight = value2;
                targetStyle.MarginBottom = value3;
                targetStyle.MarginLeft = value4;
            }
        }

        private static void MapPaddings(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            UIFixedLength value1 = MapFixedLength(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value1;
                targetStyle.PaddingBottom = value1;
                targetStyle.PaddingLeft = value1;
            }
            else if (property.children.Count == 2) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value1;
                targetStyle.PaddingLeft = value2;
            }
            else if (property.children.Count == 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value3;
                targetStyle.PaddingLeft = value2;
            }
            else if (property.children.Count > 3) {
                UIFixedLength value2 = MapFixedLength(property.children[1], context);
                UIFixedLength value3 = MapFixedLength(property.children[2], context);
                UIFixedLength value4 = MapFixedLength(property.children[3], context);
                targetStyle.PaddingTop = value1;
                targetStyle.PaddingRight = value2;
                targetStyle.PaddingBottom = value3;
                targetStyle.PaddingLeft = value4;
            }
        }

        private static UIMeasurement MapMeasurement(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        return new UIMeasurement(measurementValue, MapUnit(measurementNode.unit));
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new UIMeasurement(literalValue);
                    }

                    break;
            }

            throw new CompileException(value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIFixedLength MapFixedLength(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        UIFixedUnit unit = MapFixedUnit(measurementNode.unit);
                        if (unit == UIFixedUnit.Percent) {
                            measurementValue *= 0.01f;
                        }
                        return new UIFixedLength(measurementValue, unit);
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new UIFixedLength(literalValue);
                    }

                    break;
            }

            throw new CompileException(value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIMeasurementUnit MapUnit(UnitNode unitNode) {
            if (unitNode == null) return UIMeasurementUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIMeasurementUnit.Pixel;
                case "pca":
                    return UIMeasurementUnit.ParentContentArea;
                case "pcz":
                    return UIMeasurementUnit.ParentSize;
                case "em":
                    return UIMeasurementUnit.Em;
                case "cnt":
                    return UIMeasurementUnit.Content;
                case "lh":
                    return UIMeasurementUnit.LineHeight;
                case "aw":
                    return UIMeasurementUnit.AnchorWidth;
                case "ah":
                    return UIMeasurementUnit.AnchorHeight;
                case "vw":
                    return UIMeasurementUnit.ViewportWidth;
                case "vh":
                    return UIMeasurementUnit.ViewportHeight;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} but this unit isn't supported. " +
                             "Try px, pca, pcz, em, cnt, aw, ah, vw, vh or lh instead (see UIMeasurementUnit). Will fall back to px.");

            return UIMeasurementUnit.Pixel;
        }

        private static UIFixedUnit MapFixedUnit(UnitNode unitNode) {
            if (unitNode == null) return UIFixedUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIFixedUnit.Pixel;
                case "%":
                    return UIFixedUnit.Percent;
                case "vh":
                    return UIFixedUnit.ViewportHeight;
                case "vw":
                    return UIFixedUnit.ViewportWidth;
                case "em":
                    return UIFixedUnit.Em;
                case "lh":
                    return UIFixedUnit.LineHeight;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} but this unit isn't supported. " +
                             "Try px, %, em, vw, vh or lh instead (see UIFixedUnit). Will fall back to px.");

            return UIFixedUnit.Pixel;
        }

        private static GridTemplateUnit MapGridTemplateUnit(UnitNode unitNode) {
            if (unitNode == null) return GridTemplateUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return GridTemplateUnit.Pixel;
                case "mx":
                    return GridTemplateUnit.MaxContent;
                case "mn":
                    return GridTemplateUnit.MinContent;
                case "fr":
                    return GridTemplateUnit.FractionalRemaining;
                case "vw":
                    return GridTemplateUnit.ViewportWidth;
                case "vh":
                    return GridTemplateUnit.ViewportHeight;
                case "em":
                    return GridTemplateUnit.Em;
                case "cca":
                    return GridTemplateUnit.ContainerContentArea;
                case "cnt":
                    return GridTemplateUnit.Container;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} but this unit isn't supported. " +
                             "Try px, mx, mn, em, vw, vh, cca, fr or cnt instead (see GridTemplateUnit). Will fall back to px.");

            return GridTemplateUnit.Pixel;
        }

        private static TransformOffset MapTransformOffset(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        return new TransformOffset(measurementValue, MapTransformUnit(measurementNode.unit));
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new TransformOffset(literalValue);
                    }

                    break;
            }

            throw new CompileException(value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static TransformUnit MapTransformUnit(UnitNode unitNode) {
            if (unitNode == null) return TransformUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return TransformUnit.Pixel;
                case "w":
                    return TransformUnit.ActualWidth;
                case "h":
                    return TransformUnit.ActualHeight;
                case "alw":
                    return TransformUnit.AllocatedWidth;
                case "alh":
                    return TransformUnit.AllocatedHeight;
                case "cw":
                    return TransformUnit.ContentWidth;
                case "ch":
                    return TransformUnit.ContentHeight;
                case "em":
                    return TransformUnit.Em;
                case "caw":
                    return TransformUnit.ContentAreaWidth;
                case "cah":
                    return TransformUnit.ContentAreaHeight;
                case "aw":
                    return TransformUnit.AnchorWidth;
                case "ah":
                    return TransformUnit.AnchorHeight;
                case "vw":
                    return TransformUnit.ViewportWidth;
                case "vh":
                    return TransformUnit.ViewportHeight;
                case "pw":
                    return TransformUnit.ParentWidth;
                case "ph":
                    return TransformUnit.ParentHeight;
                case "pcaw":
                    return TransformUnit.ParentContentAreaWidth;
                case "pcah":
                    return TransformUnit.ParentContentAreaHeight;
                case "sw":
                    return TransformUnit.ScreenWidth;
                case "sh":
                    return TransformUnit.ScreenHeight;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} but this unit isn't supported. " +
                             "Try px, w, h, alw, alh, cw, ch, em, caw, cah, aw, ah, vw, vh, pw, ph, pcaw, pcah, sw, or sh instead (see TransformUnit). Will fall back to px.");

            return TransformUnit.Pixel;
        }

        private static void MapOverflows(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            Overflow overflowX = MapEnum<Overflow>(property.children[0], context);
            Overflow overflowY = overflowX;

            if (property.children.Count == 2) {
                overflowY = MapEnum<Overflow>(property.children[1], context);
            }

            // should we check for more than 2 values and log a warning?

            targetStyle.OverflowX = overflowX;
            targetStyle.OverflowY = overflowY;
        }

        private static Texture2D MapTexture(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            switch (node) {
                case UrlNode urlNode:
                    return ResourceManager.GetTexture(TransformUrlNode(urlNode, context));
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(node, $"Expected url(path/to/texture) but found {node}.");
        }

        private static TMP_FontAsset MapFont(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            switch (node) {
                case UrlNode urlNode:
                    return Resources.Load<TMP_FontAsset>(TransformUrlNode(urlNode, context));
                    // return ResourceManager.GetFont(TransformUrlNode(urlNode, context));
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(node, $"Expected url(path/to/font) but found {node}.");
        }

        private static string TransformUrlNode(UrlNode urlNode, StyleCompileContext context) {
            StyleASTNode url = context.GetValueForReference(urlNode.url);

            if (url.type == StyleASTNodeType.Identifier) {
                return ((StyleIdentifierNode) url).name;
            }

            if (url.type == StyleASTNodeType.StringLiteral) {
                return ((StyleLiteralNode) url).rawValue;
            }

            throw new CompileException(url, "Invalid url value.");
        }

        private static Color MapColor(PropertyNode property, StyleCompileContext context) {
            AssertSingleValue(property.children);

            var styleAstNode = context.GetValueForReference(property.children[0]);
            switch (styleAstNode) {
                case StyleIdentifierNode identifierNode:
                    Color color;
                    ColorUtility.TryParseHtmlString(identifierNode.name, out color);
                    return color;
                case ColorNode colorNode: return colorNode.color;
                case RgbaNode rgbaNode: return MapRbgaNodeToColor(rgbaNode, context);
                case RgbNode rgbNode: return MapRgbNodeToColor(rgbNode, context);
                default:
                    throw new CompileException(styleAstNode, $"Unsupported color value.");
            }
        }

        private static Color MapRbgaNodeToColor(RgbaNode rgbaNode, StyleCompileContext context) {
            byte red = (byte) MapNumber(rgbaNode.red, context);
            byte green = (byte) MapNumber(rgbaNode.green, context);
            byte blue = (byte) MapNumber(rgbaNode.blue, context);
            byte alpha = (byte) MapNumber(rgbaNode.alpha, context);

            return new Color32(red, green, blue, alpha);
        }

        private static Color MapRgbNodeToColor(RgbNode rgbaNode, StyleCompileContext context) {
            byte red = (byte) MapNumber(rgbaNode.red, context);
            byte green = (byte) MapNumber(rgbaNode.green, context);
            byte blue = (byte) MapNumber(rgbaNode.blue, context);

            return new Color32(red, green, blue, 255);
        }

        private static float MapNumber(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            if (node is StyleIdentifierNode identifierNode) {
                if (TryParseFloat(identifierNode.name, out float number)) {
                    return number;
                }
            }

            if (node.type == StyleASTNodeType.NumericLiteral) {
                if (TryParseFloat(((StyleLiteralNode) node).rawValue, out float number)) {
                    return number;
                }
            }

            throw new CompileException(node, $"Expected a numeric value but all I got was this lousy {node}");
        }

        private static string MapString(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                return identifierNode.name;
            }

            if (node is StyleLiteralNode literalNode && literalNode.type == StyleASTNodeType.StringLiteral) {
                return literalNode.rawValue;
            }

            throw new CompileException(node, $"Expected a string value but all I got was this lousy {node}");
        }

        public static void MapProperty(UIStyle targetStyle, PropertyNode node, StyleCompileContext context) {
            string propertyName = node.identifier;
            LightList<StyleASTNode> propertyValues = node.children;

            if (propertyValues.Count == 0) {
                throw new CompileException(node, "Property does not have a value.");
            }

            string propertyKey = propertyName.ToLower();

            mappers.TryGetValue(propertyKey, out Action<UIStyle, PropertyNode, StyleCompileContext> action);
            action?.Invoke(targetStyle, node, context);
            if (action == null) Debug.LogWarning($"{propertyKey} is an unknown style property.");
        }

        private static T MapEnum<T>(StyleASTNode node, StyleCompileContext context) where T : struct {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                if (Enum.TryParse(identifierNode.name, true, out T thing)) {
                    return thing;
                }
            }

            throw new CompileException(node, $"Expected a proper {typeof(T).Name} value, which must be one of " +
                                             $"{EnumValues(typeof(T))} and your " +
                                             $"value {node} does not match any of them.");
        }

        private static bool TryParseFloat(string input, out float result) {
           return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static void AssertSingleValue(LightList<StyleASTNode> propertyValues) {
            if (propertyValues.Count > 1) {
                throw new CompileException(propertyValues[1], "Found too many values.");
            }
        }
    }
}