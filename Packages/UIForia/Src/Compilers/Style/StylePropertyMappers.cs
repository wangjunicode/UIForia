using System;
using System.Collections.Generic;
using System.Globalization;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

// ReSharper disable StringLiteralTypo

namespace UIForia.Compilers.Style {

    public static class StylePropertyMappers {

        public const string k_RepeatFit = "fit";
        public const string k_RepeatFill = "fill";

        private static readonly Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> mappers
            = new Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> {
                // Overflow
                {"overflow", (targetStyle, property, context) => MapOverflows(targetStyle, property, context)},
                {"overflowx", (targetStyle, property, context) => targetStyle.OverflowX = MapEnum<Overflow>(property.children[0], context)},
                {"overflowy", (targetStyle, property, context) => targetStyle.OverflowY = MapEnum<Overflow>(property.children[0], context)},

                // Alignment
                {"alignmentbehaviorx", (targetStyle, property, context) => targetStyle.AlignmentBehaviorX = MapEnum<AlignmentBehavior>(property.children[0], context)},
                {"alignmentbehaviory", (targetStyle, property, context) => targetStyle.AlignmentBehaviorY = MapEnum<AlignmentBehavior>(property.children[0], context)},
                {"alignmenttargetx", (targetStyle, property, context) => targetStyle.AlignmentTargetX = MapEnum<AlignmentTarget>(property.children[0], context)},
                {"alignmenttargety", (targetStyle, property, context) => targetStyle.AlignmentTargetY = MapEnum<AlignmentTarget>(property.children[0], context)},
                {"alignmentpivotx", (targetStyle, property, context) => targetStyle.AlignmentPivotX = MapNumber(property.children[0], context)},
                {"alignmentpivoty", (targetStyle, property, context) => targetStyle.AlignmentPivotY = MapNumber(property.children[0], context)},
                {"alignmentoffsetx", (targetStyle, property, context) => targetStyle.AlignmentOffsetX = MapFixedLength(property.children[0], context)},
                {"alignmentoffsety", (targetStyle, property, context) => targetStyle.AlignmentOffsetY = MapFixedLength(property.children[0], context)},

                {"fithorizontal", (targetStyle, property, context) => targetStyle.FitHorizontal = MapEnum<Fit>(property.children[0], context)},
                {"fitvertical", (targetStyle, property, context) => targetStyle.FitVertical = MapEnum<Fit>(property.children[0], context)},
                
                // Background
                {"backgroundcolor", (targetStyle, property, context) => targetStyle.BackgroundColor = MapColor(property, context)},
                {"backgroundtint", (targetStyle, property, context) => targetStyle.BackgroundTint = MapColor(property, context)},
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

                {"bordercolor", (targetStyle, property, context) => MapBorderColors(targetStyle, property, context)},
                {"bordercolortop", (targetStyle, property, context) => targetStyle.BorderColorTop = MapColor(property, context)},
                {"bordercolorright", (targetStyle, property, context) => targetStyle.BorderColorRight = MapColor(property, context)},
                {"bordercolorbottom", (targetStyle, property, context) => targetStyle.BorderColorBottom = MapColor(property, context)},
                {"bordercolorleft", (targetStyle, property, context) => targetStyle.BorderColorLeft = MapColor(property, context)},

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

                {"griditemx", (targetStyle, property, context) => targetStyle.GridItemX = MapGridItemPlacement(property, context)},
                {"griditemy", (targetStyle, property, context) => targetStyle.GridItemY = MapGridItemPlacement(property, context)},
                {"griditemwidth", (targetStyle, property, context) => targetStyle.GridItemWidth = MapGridItemPlacement(property, context)},
                {"griditemheight", (targetStyle, property, context) => targetStyle.GridItemHeight = MapGridItemPlacement(property, context)},
                {"gridlayoutcolalignment", (targetStyle, property, context) => targetStyle.GridLayoutColAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutrowalignment", (targetStyle, property, context) => targetStyle.GridLayoutRowAlignment = MapEnum<GridAxisAlignment>(property.children[0], context)},
                {"gridlayoutdensity", (targetStyle, property, context) => targetStyle.GridLayoutDensity = MapEnum<GridLayoutDensity>(property.children[0], context)},
                {"gridlayoutcoltemplate", (targetStyle, property, context) => targetStyle.GridLayoutColTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutrowtemplate", (targetStyle, property, context) => targetStyle.GridLayoutRowTemplate = MapGridLayoutTemplate(property, context)},
                {"gridlayoutdirection", (targetStyle, property, context) => targetStyle.GridLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"gridlayoutcolautosize", (targetStyle, property, context) => targetStyle.GridLayoutColAutoSize = MapGridTrackSize(property.children[0], context)},
                {"gridlayoutrowautosize", (targetStyle, property, context) => targetStyle.GridLayoutRowAutoSize = MapGridTrackSize(property.children[0], context)},
                {"gridlayoutcolgap", (targetStyle, property, context) => targetStyle.GridLayoutColGap = MapNumber(property.children[0], context)},
                {"gridlayoutrowgap", (targetStyle, property, context) => targetStyle.GridLayoutRowGap = MapNumber(property.children[0], context)},

                {"flexitemgrow", (targetStyle, property, context) => targetStyle.FlexItemGrow = (int) MapNumber(property.children[0], context)},
                {"flexitemshrink", (targetStyle, property, context) => targetStyle.FlexItemShrink = (int) MapNumber(property.children[0], context)},
                {"flexlayoutwrap", (targetStyle, property, context) => targetStyle.FlexLayoutWrap = MapEnum<LayoutWrap>(property.children[0], context)},
                {"flexlayoutdirection", (targetStyle, property, context) => targetStyle.FlexLayoutDirection = MapEnum<LayoutDirection>(property.children[0], context)},
                {"flexlayoutmainaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutMainAxisAlignment = MapEnum<MainAxisAlignment>(property.children[0], context)},
                {"flexlayoutcrossaxisalignment", (targetStyle, property, context) => targetStyle.FlexLayoutCrossAxisAlignment = MapEnum<CrossAxisAlignment>(property.children[0], context)},

                {"radiallayoutstartangle", (targetStyle, property, context) => targetStyle.RadialLayoutStartAngle = MapNumber(property.children[0], context)},
                {"radiallayoutendangle", (targetStyle, property, context) => targetStyle.RadialLayoutEndAngle = MapNumber(property.children[0], context)},
                {"radiallayoutradius", (targetStyle, property, context) => targetStyle.RadialLayoutRadius = MapFixedLength(property.children[0], context)},

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
                {"layer", (targetStyle, property, context) => targetStyle.Layer = (int) MapNumber(property.children[0], context)},

                // Text
                {"textcolor", (targetStyle, property, context) => targetStyle.TextColor = MapColor(property, context)},
                {"textfontasset", (targetStyle, property, context) => targetStyle.TextFontAsset = MapFont(property.children[0], context)},
                {"textfontstyle", (targetStyle, property, context) => targetStyle.TextFontStyle = MapTextFontStyle(property, context)},
                {"textfontsize", (targetStyle, property, context) => targetStyle.TextFontSize = MapFixedLength(property.children[0], context)},
                {"textfacedilate", (targetStyle, property, context) => targetStyle.TextFaceDilate = Mathf.Clamp01(MapNumber(property.children[0], context))},
                {"textalignment", (targetStyle, property, context) => targetStyle.TextAlignment = MapEnum<TextAlignment>(property.children[0], context)},
                {"textoutlinewidth", (targetStyle, property, context) => targetStyle.TextOutlineWidth = MapNumber(property.children[0], context)},
                {"textoutlinecolor", (targetStyle, property, context) => targetStyle.TextOutlineColor = MapColor(property, context)},
                {"textoutlinesoftness", (targetStyle, property, context) => targetStyle.TextOutlineSoftness = MapNumber(property.children[0], context)},
                {"textglowcolor", (targetStyle, property, context) => targetStyle.TextGlowColor = MapColor(property, context)},
                {"textglowoffset", (targetStyle, property, context) => targetStyle.TextGlowOffset = MapNumber(property.children[0], context)},
                {"textglowinner", (targetStyle, property, context) => targetStyle.TextGlowInner = MapNumber(property.children[0], context)},
                {"textglowouter", (targetStyle, property, context) => targetStyle.TextGlowOuter = MapNumber(property.children[0], context)},
                {"textglowpower", (targetStyle, property, context) => targetStyle.TextGlowPower = MapNumber(property.children[0], context)},
                {"textunderlaycolor", (targetStyle, property, context) => targetStyle.TextUnderlayColor = MapColor(property, context)},
                {"textunderlayx", (targetStyle, property, context) => targetStyle.TextUnderlayX = MapNumber(property.children[0], context)},
                {"textunderlayy", (targetStyle, property, context) => targetStyle.TextUnderlayY = MapNumber(property.children[0], context)},
                {"textunderlaydilate", (targetStyle, property, context) => targetStyle.TextUnderlayDilate = MapNumber(property.children[0], context)},
                {"textunderlaysoftness", (targetStyle, property, context) => targetStyle.TextUnderlaySoftness = MapNumber(property.children[0], context)},
                {"textunderlaytype", (targetStyle, property, context) => targetStyle.TextUnderlayType = MapEnum<UnderlayType>(property.children[0], context)},
                {"texttransform", (targetStyle, property, context) => targetStyle.TextTransform = MapEnum<TextTransform>(property.children[0], context)}, {
                    "textwhitespacemode", (targetStyle, property, context) => {
                        // couldn't find a generic version for merging a list of enum flags... just one that involves boxing: https://stackoverflow.com/questions/987607/c-flags-enum-generic-function-to-look-for-a-flag
                        WhitespaceMode result = MapEnum<WhitespaceMode>(property.children[0], context);
                        if (property.children.Count > 1) {
                            for (int index = 1; index < property.children.Count; index++) {
                                result |= MapEnum<WhitespaceMode>(property.children[index], context);
                            }
                        }

                        targetStyle.TextWhitespaceMode = result;
                    }
                },

                {"painter", (targetStyle, property, context) => targetStyle.Painter = MapPainter(property, context)},

                // Scrollbar
                {"scrollbar", (targetStyle, property, context) => targetStyle.Scrollbar = MapString(property.children[0], context)},
                {"scrollbarsize", (targetStyle, property, context) => targetStyle.ScrollbarSize = MapMeasurement(property.children[0], context)},
                {"scrollbarcolor", (targetStyle, property, context) => targetStyle.ScrollbarColor = MapColor(property, context)},

                // shadows for things
                {"shadowtype", (targetStyle, property, context) => targetStyle.ShadowType = MapEnum<UnderlayType>(property.children[0], context)},
                {"shadowoffsetx", (targetStyle, property, context) => targetStyle.ShadowOffsetX = MapNumber(property.children[0], context)},
                {"shadowoffsety", (targetStyle, property, context) => targetStyle.ShadowOffsetY = MapNumber(property.children[0], context)},
                {"shadowsoftnessx", (targetStyle, property, context) => targetStyle.ShadowSoftnessX = MapNumber(property.children[0], context)},
                {"shadowsoftnessy", (targetStyle, property, context) => targetStyle.ShadowSoftnessY = MapNumber(property.children[0], context)},
                {"shadowintensity", (targetStyle, property, context) => targetStyle.ShadowIntensity = MapNumber(property.children[0], context)},
            };

        private static string MapPainter(PropertyNode property, StyleCompileContext context) {
            string customPainter = MapString(property.children[0], context);

            if (customPainter == "self" || customPainter == "none") {
                return customPainter;
            }

            if (string.IsNullOrEmpty(customPainter) || !Application.HasCustomPainter(customPainter)) {
                Debug.Log($"Could not find your custom painter {customPainter} in file {context.fileName}.");
            }

            return customPainter;
        }

        private static FontStyle MapTextFontStyle(PropertyNode property, StyleCompileContext context) {
            FontStyle style = FontStyle.Normal;

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

                        if (propertyValue.Contains("underline")) {
                            style |= Text.FontStyle.Underline;
                        }

                        if (propertyValue.Contains("strikethrough")) {
                            style |= Text.FontStyle.StrikeThrough;
                        }

                        break;
                    default:
                        throw new CompileException(context.fileName, value, $"Invalid TextFontStyle {value}. " +
                                                                            "Make sure you use one of those: bold, italic, underline or strikethrough.");
                }
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

        private static GridItemPlacement MapGridItemPlacement(PropertyNode property, StyleCompileContext context) {
            StyleASTNode dereferencedValue = context.GetValueForReference(property.children[0]);
            
            switch (dereferencedValue.type) {
                case StyleASTNodeType.NumericLiteral:
                    int number = (int) MapNumber(dereferencedValue, context);
                    if (number < 0) {
                        return new GridItemPlacement(IntUtil.UnsetValue);
                    }

                    return new GridItemPlacement(number);

                case StyleASTNodeType.StringLiteral:
                    string placementName = MapString(property.children[0], context);
                    if (string.IsNullOrEmpty(placementName) || string.IsNullOrWhiteSpace(placementName) || placementName == ".") {
                        return new GridItemPlacement(IntUtil.UnsetValue);
                    }
                    else {
                        return new GridItemPlacement(placementName);
                    }
            }
            throw new CompileException(context.fileName, property, $"Had a hard time parsing that grid item placement: {property}.");
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
            LightList<GridTrackSize> gridTrackSizes = LightList<GridTrackSize>.Get();
            for (int index = 0; index < propertyNode.children.Count; index++) {
                StyleASTNode trackSize = propertyNode.children[index];
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

                    throw new CompileException(context.fileName, literalNode, $"Could not create a grid track size out of the value {literalNode}.");

                case MeasurementNode measurementNode:
                    GridTemplateUnit unit = MapGridTemplateUnit(measurementNode.unit, context);
                    float value = MapNumber(measurementNode.value, context);
                    return new GridTrackSize(value, unit);

                case StyleFunctionNode functionNode:

                    GridTrackSizeType trackSizeType;
                    GridTrackSize size = default;

                    switch (functionNode.identifier.ToLower()) {
                        case "repeat":
                            trackSizeType = GridTrackSizeType.Repeat;
                            if (functionNode.children.Count < 2) {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. Repeat must have at least two arguments.");
                            }

                            StyleASTNode firstChild = context.GetValueForReference(functionNode.children[0]);
                            if (firstChild is StyleLiteralNode literalNode) {
                                if (literalNode.type != StyleASTNodeType.NumericLiteral) {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }

                                float v = MapNumber(literalNode, context);
                                if (Mathf.Floor(v) != v || v < 1) {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }

                                size.value = v;
                                size.type = GridTrackSizeType.Repeat;
                            }
                            else if (firstChild is StyleIdentifierNode identifierNode) {
                                if (identifierNode.name == k_RepeatFill) {
                                    size.type = GridTrackSizeType.RepeatFill;
                                }
                                else if (identifierNode.name == k_RepeatFit) {
                                    size.type = GridTrackSizeType.RepeatFit;
                                }
                                else {
                                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                                }
                            }
                            else {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. The first argument of repeat() must be a positive integer > 0 or one of the keywords {k_RepeatFill} or {k_RepeatFit}.");
                            }

                            size.pattern = MapGridTrackSizePattern(1, functionNode.children, context, true);

                            break;
                        case "minmax":
                            trackSizeType = GridTrackSizeType.MinMax;
                            if (functionNode.children.Count != 2) {
                                throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. minmax() must have two arguments.");
                            }

                            size.type = GridTrackSizeType.MinMax;
                            size.pattern = MapGridTrackSizePattern(0, functionNode.children, context, false);
                            
                            break;
                        default:
                            throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}. Expected a known track size function (repeat, grow, shrink) but all I got was {functionNode.identifier}");
                    }

                    GridTrackSize[] pattern = new GridTrackSize[functionNode.children.Count];

                    return size;

                default:
                    throw new CompileException(context.fileName, trackSize, $"Had a hard time parsing that track size: {trackSize}.");
            }
        }

        private static GridTrackSize[] MapGridTrackSizePattern(int startIndex, LightList<StyleASTNode> nodes, StyleCompileContext context, bool allowGrowOrShrink) {
            GridTrackSize[] retn = new GridTrackSize[nodes.Count - startIndex];

            for (int index = startIndex; index < nodes.Count; index++) {
                StyleASTNode argument = context.GetValueForReference(nodes[index]);
                GridTrackSize trackSize = MapGridTrackSize(argument, context);
                if (trackSize.type == GridTrackSizeType.Repeat) {
                    throw new CompileException(argument, "You cannot nest repeats.");
                }

                if (!allowGrowOrShrink && (trackSize.type == GridTrackSizeType.MinMax)) {
                    throw new CompileException(argument, "You cannot nest MinMaxes into each other.");
                }

                retn[index - startIndex] = trackSize;
            }

            return retn;
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

        private static void MapBorderColors(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            Color value1 = MapColor(property.children[0], context);

            if (property.children.Count == 1) {
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value1;
                targetStyle.BorderColorBottom = value1;
                targetStyle.BorderColorLeft = value1;
            }
            else if (property.children.Count == 2) {
                Color value2 = MapColor(property.children[1], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value1;
                targetStyle.BorderColorLeft = value2;
            }
            else if (property.children.Count == 3) {
                Color value2 = MapColor(property.children[1], context);
                Color value3 = MapColor(property.children[2], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value3;
                targetStyle.BorderColorLeft = value2;
            }
            else if (property.children.Count > 3) {
                Color value2 = MapColor(property.children[1], context);
                Color value3 = MapColor(property.children[2], context);
                Color value4 = MapColor(property.children[3], context);
                targetStyle.BorderColorTop = value1;
                targetStyle.BorderColorRight = value2;
                targetStyle.BorderColorBottom = value3;
                targetStyle.BorderColorLeft = value4;
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
                    UIMeasurementUnit unit = MapUnit(measurementNode.unit, context);
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        if (unit == UIMeasurementUnit.Percentage) {
                            measurementValue *= 0.01f;
                        }

                        return new UIMeasurement(measurementValue, unit);
                    }
                    else {
                        return new UIMeasurement(1f, unit);
                    }

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new UIMeasurement(literalValue);
                    }

                    break;
            }

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIFixedLength MapFixedLength(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        UIFixedUnit unit = MapFixedUnit(measurementNode.unit, context);
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

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static UIMeasurementUnit MapUnit(UnitNode unitNode, StyleCompileContext context) {
            if (unitNode == null) return UIMeasurementUnit.Pixel;

            switch (unitNode.value) {
                case "px":
                    return UIMeasurementUnit.Pixel;

                case "pca":
                    return UIMeasurementUnit.ParentContentArea;

                case "psz":
                    return UIMeasurementUnit.ParentSize;

                case "em":
                    return UIMeasurementUnit.Em;

                case "cnt":
                case "content":
                    return UIMeasurementUnit.Content;

                case "vw":
                    return UIMeasurementUnit.ViewportWidth;

                case "vh":
                    return UIMeasurementUnit.ViewportHeight;

                case "%":
                    return UIMeasurementUnit.Percentage;

                case "intrinsic":
                    return UIMeasurementUnit.IntrinsicPreferred;

                case "intrinsic-min":
                    return UIMeasurementUnit.IntrinsicMinimum;

                case "fit-content":
                    return UIMeasurementUnit.FitContent;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, pca, pcz, em, cnt, aw, ah, vw, vh or lh instead (see UIMeasurementUnit). Will fall back to px.");

            return UIMeasurementUnit.Pixel;
        }

        private static UIFixedUnit MapAlignmentUnit(UnitNode unitNode, StyleCompileContext context) {
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
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, em, vw, vh or lh instead (see UIFixedUnit). Will fall back to px.");

            return UIFixedUnit.Pixel;
        }

        private static UIFixedUnit MapFixedUnit(UnitNode unitNode, StyleCompileContext context) {
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
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, %, em, vw, vh or lh instead (see UIFixedUnit). Will fall back to px.");

            return UIFixedUnit.Pixel;
        }

        private static GridTemplateUnit MapGridTemplateUnit(UnitNode unitNode, StyleCompileContext context) {
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
                case "pca":
                    return GridTemplateUnit.ParentContentArea;
                case "psz":
                    return GridTemplateUnit.ParentSize;
            }

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
                             "Try px, mx, mn, em, vw, vh, fr, pca or psz instead (see GridTemplateUnit). Will fall back to px.");

            return GridTemplateUnit.Pixel;
        }

        private static TransformOffset MapTransformOffset(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (TryParseFloat(measurementNode.value.rawValue, out float measurementValue)) {
                        return new TransformOffset(measurementValue, MapTransformUnit(measurementNode.unit, context));
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (TryParseFloat(literalNode.rawValue, out float literalValue)) {
                        return new TransformOffset(literalValue);
                    }

                    break;
            }

            throw new CompileException(context.fileName, value, $"Cannot parse value, expected a numeric literal or measurement {value}.");
        }

        private static TransformUnit MapTransformUnit(UnitNode unitNode, StyleCompileContext context) {
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

            Debug.LogWarning($"You used a {unitNode.value} in line {unitNode.line} column {unitNode.column} in file {context.fileName} but this unit isn't supported. " +
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
                    AssetInfo assetInfo = TransformUrlNode(urlNode, context);
                    if (assetInfo.SpriteName != null) {
                        throw new CompileException(urlNode, "SpriteAtlas access is coming soon!");
                    }

                    return context.application.ResourceManager.GetTexture(assetInfo.Path);
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(context.fileName, node, $"Expected url(path/to/texture) but found {node}.");
        }

        private static FontAsset MapFont(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);
            switch (node) {
                case UrlNode urlNode:
                    AssetInfo assetInfo = TransformUrlNode(urlNode, context);
                    if (assetInfo.SpriteName != null) {
                        throw new CompileException(urlNode, "SpriteAtlas access is coming soon!");
                    }

                    return context.application.ResourceManager.GetFont(assetInfo.Path);
                // return ResourceManager.GetFont(TransformUrlNode(urlNode, context));
                case StyleLiteralNode literalNode:
                    string value = literalNode.rawValue;
                    if (value == "unset" || value == "default" || value == "null") {
                        return null;
                    }

                    break;
            }

            throw new CompileException(context.fileName, node, $"Expected url(path/to/font) but found {node}.");
        }

        private static AssetInfo TransformUrlNode(UrlNode urlNode, StyleCompileContext context) {
            StyleASTNode url = context.GetValueForReference(urlNode.url);
            StyleASTNode spriteNameNode = context.GetValueForReference(urlNode.spriteName);
            string spriteName = null;
            if (spriteNameNode != null && spriteNameNode is StyleLiteralNode stringNode) {
                spriteName = stringNode.rawValue;
            }

            if (url.type == StyleASTNodeType.Identifier) {
                return new AssetInfo {
                    Path = ((StyleIdentifierNode) url).name,
                    SpriteName = spriteName
                };
            }

            if (url.type == StyleASTNodeType.StringLiteral) {
                return new AssetInfo {
                    Path = ((StyleLiteralNode) url).rawValue,
                    SpriteName = spriteName
                };
            }

            throw new CompileException(url, "Invalid url value.");
        }


        private static Color MapColor(PropertyNode property, StyleCompileContext context) {
            AssertSingleValue(property.children, context);
            return MapColor(property.children[0], context);
        }

        private static Color MapColor(StyleASTNode colorStyleAstNode, StyleCompileContext context) {
            var styleAstNode = context.GetValueForReference(colorStyleAstNode);
            switch (styleAstNode) {
                case StyleIdentifierNode identifierNode:
                    Color color;
                    ColorUtility.TryParseHtmlString(identifierNode.name, out color);
                    return color;
                case ColorNode colorNode: return colorNode.color;
                case RgbaNode rgbaNode: return MapRbgaNodeToColor(rgbaNode, context);
                case RgbNode rgbNode: return MapRgbNodeToColor(rgbNode, context);
                default:
                    throw new CompileException(context.fileName, styleAstNode, "Unsupported color value.");
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

        internal static float MapNumber(StyleASTNode node, StyleCompileContext context) {
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

            throw new CompileException(context.fileName, node, $"Expected a numeric value but all I got was this lousy {node}");
        }

        private static string MapString(StyleASTNode node, StyleCompileContext context) {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                return identifierNode.name;
            }

            if (node is StyleLiteralNode literalNode && literalNode.type == StyleASTNodeType.StringLiteral) {
                return literalNode.rawValue;
            }

            throw new CompileException(context.fileName, node, $"Expected a string value but all I got was this lousy {node}");
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
            if (action == null) Debug.LogWarning($"{propertyKey} at column {node.column} line {node.line} in file {context.fileName} is an unknown style property.");
        }

        internal static T MapEnum<T>(StyleASTNode node, StyleCompileContext context) where T : struct {
            node = context.GetValueForReference(node);

            if (node is StyleIdentifierNode identifierNode) {
                if (Enum.TryParse(identifierNode.name, true, out T thing)) {
                    return thing;
                }
            }

            throw new CompileException(context.fileName, node, $"Expected a proper {typeof(T).Name} value, which must be one of " +
                                                               $"{EnumValues(typeof(T))} and your " +
                                                               $"value {node} does not match any of them.");
        }

        private static bool TryParseFloat(string input, out float result) {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static void AssertSingleValue(LightList<StyleASTNode> propertyValues, StyleCompileContext context) {
            if (propertyValues.Count > 1) {
                throw new CompileException(context.fileName, propertyValues[1], "Found too many values.");
            }
        }

        public struct AssetInfo {

            public string Path;
            public string SpriteName;

        }

    }

}