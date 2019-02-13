using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Style.Parsing;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers.Style {
    public struct StylePropertyMappers {

        private static readonly Dictionary<string, Action<UIStyle, LightList<StyleASTNode>>> mappers = new Dictionary<string, Action<UIStyle, LightList<StyleASTNode>>> {
            { "backgroundcolor", (targetStyle, valueParts) => targetStyle.BackgroundColor = MapColor(valueParts) },
            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
            { "opacity", (targetStyle, valueParts) => targetStyle.Opacity = MapNumber(valueParts) },
            { "cursor", (targetStyle, valueParts) => {

                    // TODO implement this
                    
                    CursorStyle cursor = null; // new CursorStyle(texturePath, texture, new Vector2(hotSpotX, hotSpotY));
                    targetStyle.Cursor = cursor;
                }
            },
//            { "backgroundimage", (targetStyle, valueParts) => targetStyle.BackgroundImage = 
//                ResourceManager.GetTexture(valueParts[0]) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
//            { "bordercolor", (targetStyle, valueParts) => targetStyle.BorderColor = MapColor(valueParts) },
        };

        private static float MapNumber(LightList<StyleASTNode> valueParts) {
            throw new NotImplementedException();
        }

        private static Color MapColor(LightList<StyleASTNode> colorValueParts) {
            if (colorValueParts.Count == 0) {
                return Color.clear;
            }

            if (colorValueParts.Count == 1) {
                switch (colorValueParts[0]) {
                    case RgbaNode rgbaNode: return MapRbgaNodeToColor(rgbaNode);
                    case RgbNode rgbNode: return MapRgbNodeToColor(rgbNode);
                    default: throw new ParseException($"Unsupported color value: {colorValueParts[0]}");
                }
            }

            // this must be a color in # notation
            throw new ParseException("TODO implement #color mapper");
        }

        private static Color MapRbgaNodeToColor(RgbaNode rgbaNode) {
            byte red = (byte) CompileToNumber(rgbaNode.red);
            byte green = (byte) CompileToNumber(rgbaNode.green);
            byte blue = (byte) CompileToNumber(rgbaNode.blue);
            byte alpha = (byte) CompileToNumber(rgbaNode.alpha);

            return new Color32(red, green, blue, alpha);
        }

        private static Color MapRgbNodeToColor(RgbNode rgbaNode) {
            byte red = (byte) CompileToNumber(rgbaNode.red);
            byte green = (byte) CompileToNumber(rgbaNode.green);
            byte blue = (byte) CompileToNumber(rgbaNode.blue);

            return new Color32(red, green, blue, 255);
        }

        private static StyleASTNode ResolveReference(ReferenceNode reference) {
//            foreach (var constant in constants) {
//                if (constant.name == reference.referenceName) {
//                    // todo this should resolve a ref and figure out type and all that jazz
//                }
//            }
            
            throw new ParseException($"Could not resolve reference {reference}");
        }

        private static int CompileToNumber(StyleASTNode node) {

            if (node is ReferenceNode) {
                node = ResolveReference((ReferenceNode) node);
            }
            
            if (node.type == StyleASTNodeType.NumericLiteral) {
                return int.Parse(((LiteralNode) node).rawValue);
            }

            throw new ParseException($"Expected a numeric value but all I got was this lousy {node}");
        }

        public static void MapProperty(UIStyle targetStyle, string propertyName, LightList<StyleASTNode> propertyValueParts) {
            mappers[propertyName].Invoke(targetStyle, propertyValueParts);
        }

//            
//            
//            switch (propertyName) {
//               
//                case "backgroundimage":
//                    targetStyle.BackgroundImage = ResourceManager.GetTexture(propertyValue);
//                    break;
//                case "overflow":
//                    Overflow overflow = ParseUtil.ParseOverflow(propertyValue);
//                    targetStyle.OverflowX = overflow;
//                    targetStyle.OverflowY = overflow;
//                    break;
//                case "overflowx":
//                    targetStyle.OverflowX = ParseUtil.ParseOverflow(propertyValue);
//                    break;
//                case "overflowy":
//                    targetStyle.OverflowY = ParseUtil.ParseOverflow(propertyValue);
//                    break;
//                case "visibility":
//                    targetStyle.Visibility = ParseUtil.ParseVisibility(propertyValue);
//                    break;
//                case "margin":
//                    ContentBoxRect rect = ParseUtil.ParseMeasurementRect(propertyValue);
//                    targetStyle.MarginTop = rect.top;
//                    targetStyle.MarginRight = rect.right;
//                    targetStyle.MarginBottom = rect.bottom;
//                    targetStyle.MarginLeft = rect.left;
//                    break;
//                case "margintop":
//                    targetStyle.MarginTop = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "marginright":
//                    targetStyle.MarginRight = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "marginbottom":
//                    targetStyle.MarginBottom = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "marginleft":
//                    targetStyle.MarginLeft = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown margin property: " + propertyName);
//            }
//        }
//
//        public static void PaddingBorderMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "padding":
//                    FixedLengthRect rect = ParseUtil.ParseFixedLengthRect(propertyValue);
//                    targetStyle.PaddingTop = rect.top;
//                    targetStyle.PaddingRight = rect.right;
//                    targetStyle.PaddingBottom = rect.bottom;
//                    targetStyle.PaddingLeft = rect.left;
//                    break;
//                case "paddingtop":
//                    targetStyle.PaddingTop = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "paddingright":
//                    targetStyle.PaddingRight = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "paddingbottom":
//                    targetStyle.PaddingBottom = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "paddingleft":
//                    targetStyle.PaddingLeft = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "border":
//                    rect = ParseUtil.ParseFixedLengthRect(propertyValue);
//                    targetStyle.BorderTop = rect.top;
//                    targetStyle.BorderRight = rect.right;
//                    targetStyle.BorderBottom = rect.bottom;
//                    targetStyle.BorderLeft = rect.left;
//                    break;
//                case "bordertop":
//                    targetStyle.BorderTop = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderright":
//                    targetStyle.BorderRight = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderbottom":
//                    targetStyle.BorderBottom = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderleft":
//                    targetStyle.BorderLeft = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown grid padding or border property: " + propertyName);
//            }
//        }
//  
//        public static void GridItemMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "griditemcolstart":
//                    targetStyle.GridItemColStart = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "griditemcolspan":
//                    targetStyle.GridItemColSpan = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "griditemrowstart":
//                    targetStyle.GridItemRowStart = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "griditemrowspan":
//                    targetStyle.GridItemRowSpan = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "griditemcolselfalignment":
//                    targetStyle.GridItemColSelfAlignment = ParseUtil.ParseGridAxisAlignment(propertyValue);
//                    break;
//                case "griditemrowselfalignment":
//                    targetStyle.GridItemRowSelfAlignment = ParseUtil.ParseGridAxisAlignment(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown grid item property: " + propertyName);
//            }
//        }
//
//        public static void GridLayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "gridlayoutdirection":
//                    targetStyle.GridLayoutDirection = ParseUtil.ParseLayoutDirection(propertyValue);
//                    break;
//                case "gridlayoutdensity":
//                    targetStyle.GridLayoutDensity = ParseUtil.ParseDensity(propertyValue);
//                    break;
//                case "gridlayoutcoltemplate":
//                    targetStyle.GridLayoutColTemplate = ParseUtil.ParseGridTemplate(propertyValue);
//                    break;
//                case "gridlayoutrowtemplate":
//                    targetStyle.GridLayoutRowTemplate = ParseUtil.ParseGridTemplate(propertyValue);
//                    break;
//                case "gridlayoutmainaxisautosize":
//                    targetStyle.GridLayoutMainAxisAutoSize = ParseUtil.ParseGridTrackSize(propertyValue);
//                    break;
//                case "gridlayoutcrossaxisautosize":
//                    targetStyle.GridLayoutCrossAxisAutoSize = ParseUtil.ParseGridTrackSize(propertyValue);
//                    break;
//                case "gridlayoutcolgap": // todo -- support fixed length
//                    targetStyle.GridLayoutColGap = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "gridlayoutrowgap": // todo -- support fixed length
//                    targetStyle.GridLayoutRowGap = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "gridlayoutcolalignment":
//                    targetStyle.GridLayoutColAlignment = ParseUtil.ParseGridAxisAlignment(propertyValue);
//                    break;
//                case "gridlayoutrowalignment":
//                    targetStyle.GridLayoutRowAlignment = ParseUtil.ParseGridAxisAlignment(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown grid layout property: " + propertyName);
//            }
//        }
//
//        public static void FlexItemMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "flexitemselfalignment":
//                    targetStyle.FlexItemSelfAlignment = ParseUtil.ParseCrossAxisAlignment(propertyValue);
//                    break;
//                case "flexitemorder":
//                    targetStyle.FlexItemOrder = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "flexitemgrow":
//                    targetStyle.FlexItemGrow = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "flexitemshrink":
//                    targetStyle.FlexItemShrink = ParseUtil.ParseInt(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown flex item property: " + propertyName);
//            }
//        }
//
//        public static void FlexLayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "flexlayoutwrap":
//                    targetStyle.FlexLayoutWrap = ParseUtil.ParseLayoutWrap(propertyValue);
//                    break;
//                case "flexlayoutdirection":
//                    targetStyle.FlexLayoutDirection = ParseUtil.ParseLayoutDirection(propertyValue);
//                    break;
//                case "flexlayoutmainaxisalignment":
//                    targetStyle.FlexLayoutMainAxisAlignment = ParseUtil.ParseMainAxisAlignment(propertyValue);
//                    break;
//                case "flexlayoutcrossaxisalignment":
//                    targetStyle.FlexLayoutCrossAxisAlignment = ParseUtil.ParseCrossAxisAlignment(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown flex item property: " + propertyName);
//            }
//        }
//
//        public static void BorderRadiusMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "borderradius":
//                    FixedLengthRect rect = ParseUtil.ParseFixedLengthRect(propertyValue);
//                    BorderRadius radius = new BorderRadius(rect.top, rect.right, rect.bottom, rect.left);
//                    targetStyle.BorderRadius = radius;
//                    break;
//                case "borderradiustopleft":
//                    targetStyle.BorderRadiusTopLeft = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderradiustopright":
//                    targetStyle.BorderRadiusTopRight = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderradiusbottomright":
//                    targetStyle.BorderRadiusBottomRight = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "borderradiusbottomleft":
//                    targetStyle.BorderRadiusBottomLeft = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown border radius property: " + propertyName);
//            }
//        }
//
//        public static void TransformMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "transformposition":
//                    TransformOffsetPair length = ParseUtil.ParseTransformPair(propertyValue);
//                    targetStyle.TransformPositionX = length.x;
//                    targetStyle.TransformPositionY = length.y;
//                    break;
//                case "transformpositionx":
//                    targetStyle.TransformPositionX = ParseUtil.ParseTransform(propertyValue);
//                    break;
//                case "transformpositiony":
//                    targetStyle.TransformPositionY = ParseUtil.ParseTransform(propertyValue);
//                    break;
//                case "transformscale":
//                    float scale = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.TransformScaleX = scale;
//                    targetStyle.TransformScaleY = scale;
//                    break;
//                case "transformscalex":
//                    targetStyle.TransformScaleX = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "transformscaley":
//                    targetStyle.TransformScaleY = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "transformpivotx":
//                    targetStyle.TransformPivotX = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "transformpivoty":
//                    targetStyle.TransformPivotY = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "transformrotation":
//                    // todo -- handle deg / rad / and maybe %
//                    targetStyle.TransformRotation = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "transformbehavior":
//                    TransformBehavior behavior = ParseUtil.ParseTransformBehavior(propertyValue);
//                    targetStyle.TransformBehaviorX = behavior;
//                    targetStyle.TransformBehaviorY = behavior;
//                    break;
//
//                case "transformbehaviorx":
//                    targetStyle.TransformBehaviorX = ParseUtil.ParseTransformBehavior(propertyValue);
//                    break;
//                case "transformbehaviory":
//                    targetStyle.TransformBehaviorY = ParseUtil.ParseTransformBehavior(propertyValue);
//                    break;
//                default:
//                    throw new ParseException("Unknown border radius property: " + propertyName);
//            }
//        }
//
//        public static void SizeMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            MeasurementPair pair;
//            switch (propertyName.ToLower()) {
//                case "minwidth":
//                    targetStyle.MinWidth = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "minheight":
//                    targetStyle.MinHeight = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "preferredwidth":
//                    targetStyle.PreferredWidth = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "preferredheight":
//                    targetStyle.PreferredHeight = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "maxwidth":
//                    targetStyle.MaxWidth = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "maxheight":
//                    targetStyle.MaxHeight = ParseUtil.ParseMeasurement(propertyValue);
//                    break;
//                case "preferredsize":
//                    pair = ParseUtil.ParseMeasurementPair(propertyValue);
//                    targetStyle.PreferredWidth = pair.x;
//                    targetStyle.PreferredHeight = pair.y;
//                    break;
//                case "minsize":
//                    pair = ParseUtil.ParseMeasurementPair(propertyValue);
//                    targetStyle.MinWidth = pair.x;
//                    targetStyle.MinHeight = pair.y;
//                    break;
//                case "maxsize":
//                    pair = ParseUtil.ParseMeasurementPair(propertyValue);
//                    targetStyle.MaxWidth = pair.x;
//                    targetStyle.MaxHeight = pair.y;
//                    break;
//                default:
//                    throw new ParseException("Unknown size property: " + propertyName);
//            }
//        }
//
//        public static void LayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "layouttype":
//                    targetStyle.LayoutType = ParseUtil.ParseLayoutType(propertyValue);
//                    break;
//                case "layoutbehavior":
//                    targetStyle.LayoutBehavior = ParseUtil.ParseLayoutBehavior(propertyValue);
//                    break;
//                case "anchortarget":
//                    targetStyle.AnchorTarget = ParseUtil.ParseAnchorTarget(propertyValue);
//                    break;
//                case "anchortop":
//                    targetStyle.AnchorTop = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "anchorright":
//                    targetStyle.AnchorRight = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "anchorbottom":
//                    targetStyle.AnchorBottom = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "anchorleft":
//                    targetStyle.AnchorLeft = ParseUtil.ParseFixedLength(propertyValue);
//                    break;
//                case "zindex":
//                    targetStyle.ZIndex = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "renderlayer":
//                    targetStyle.RenderLayer = ParseUtil.ParseRenderLayer(propertyValue);
//                    break;
//                case "renderlayeroffset":
//                    targetStyle.RenderLayerOffset = ParseUtil.ParseInt(propertyValue);
//                    break;
//            }
//        }
//
//        public static void TextMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                case "textcolor":
//                    targetStyle.TextColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "textfontasset":
//                    targetStyle.TextFontAsset = ParseUtil.ParseFont(propertyValue);
//                    break;
//                case "textfontstyle":
//                    targetStyle.TextFontStyle = ParseUtil.ParseFontStyle(propertyValue);
//                    break;
//                case "textfontsize":
//                    targetStyle.TextFontSize = ParseUtil.ParseInt(propertyValue);
//                    break;
//                case "textalignment":
//                    targetStyle.TextAlignment = ParseUtil.ParseTextAlignment(propertyValue);
//                    break;
//                default:
//                    throw new NotImplementedException();
//            }
//        }
//
//        public static void ScrollMapper(StyleParserContext context, string propertyName, string propertyValue) {
//            switch (propertyName.ToLower()) {
//                // General
//                case "scrollbarbuttonplacement":
//                    targetStyle.ScrollbarVerticalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(propertyValue);
//                    targetStyle.ScrollbarHorizontalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(propertyValue);
//                    break;
////                case "scrollbarattachment":
//////                    targetStyle.ScrollbarVerticalAttachment = ParseUtil.ParseScrollbarVerticalAttachment(propertyValue);
////                    break;
//
//                // Track
//                case "scrollbartracksize":
//                    float size = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarVerticalTrackSize = size;
//                    targetStyle.ScrollbarHorizontalTrackSize = size;
//                    break;
//                case "scrollbartrackcolor":
//                    Color trackColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarVerticalTrackColor = trackColor;
//                    targetStyle.ScrollbarHorizontalTrackColor = trackColor;
//                    break;
//                case "scrollbartrackborderradius":
//                    targetStyle.ScrollbarVerticalTrackBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalTrackBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbartrackbordersize":
//                    targetStyle.ScrollbarVerticalTrackBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalTrackBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbartrackbordercolor":
//                    targetStyle.ScrollbarVerticalTrackBorderColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalTrackBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbartrackimage":
//                    targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Handle
//                case "scrollbarhandlesize":
//                    targetStyle.ScrollbarVerticalHandleSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalHandleSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhandlecolor":
//                    targetStyle.ScrollbarVerticalHandleColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalHandleColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhandleborderradius":
//                    targetStyle.ScrollbarVerticalHandleBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalHandleBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhandlebordersize":
//                    targetStyle.ScrollbarVerticalHandleBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalHandleBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhandlebordercolor":
//                    targetStyle.ScrollbarVerticalHandleBorderColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalHandleBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhandleimage":
//                    targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Increment
//                case "scrollbarincrementsize":
//                    targetStyle.ScrollbarVerticalIncrementSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarincrementcolor":
//                    targetStyle.ScrollbarVerticalIncrementColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarincrementborderradius":
//                    targetStyle.ScrollbarVerticalIncrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarincrementbordersize":
//                    targetStyle.ScrollbarVerticalIncrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarincrementbordercolor":
//                    targetStyle.ScrollbarVerticalIncrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarincrementimage":
//                    targetStyle.ScrollbarVerticalIncrementImage = ParseUtil.ParseTexture(propertyValue);
//                    targetStyle.ScrollbarHorizontalIncrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Decrement
//                case "scrollbardecrementsize":
//                    targetStyle.ScrollbarVerticalDecrementSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbardecrementcolor":
//                    targetStyle.ScrollbarVerticalDecrementColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbardecrementborderradius":
//                    targetStyle.ScrollbarVerticalDecrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbardecrementbordersize":
//                    targetStyle.ScrollbarVerticalDecrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbardecrementbordercolor":
//                    targetStyle.ScrollbarVerticalDecrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbardecrementimage":
//                    targetStyle.ScrollbarVerticalDecrementImage = ParseUtil.ParseTexture(propertyValue);
//                    targetStyle.ScrollbarHorizontalDecrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // General
//                case "scrollbarverticalbuttonplacement":
//                    targetStyle.ScrollbarVerticalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(propertyValue);
//                    break;
//                case "scrollbarverticalattachment":
//                    targetStyle.ScrollbarVerticalAttachment = ParseUtil.ParseScrollbarVerticalAttachment(propertyValue);
//                    break;
//
//                // Track
//                case "scrollbarverticaltracksize":
//                    targetStyle.ScrollbarVerticalTrackSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaltrackcolor":
//                    targetStyle.ScrollbarVerticalTrackColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticaltrackborderradius":
//                    targetStyle.ScrollbarVerticalTrackBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaltrackbordersize":
//                    targetStyle.ScrollbarVerticalTrackBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaltrackbordercolor":
//                    targetStyle.ScrollbarVerticalTrackBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticaltrackimage":
//                    targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Handle
//                case "scrollbarverticalhandlesize":
//                    targetStyle.ScrollbarVerticalHandleSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalhandlecolor":
//                    targetStyle.ScrollbarVerticalHandleColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticalhandleborderradius":
//                    targetStyle.ScrollbarVerticalHandleBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalhandlebordersize":
//                    targetStyle.ScrollbarVerticalHandleBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalhandlebordercolor":
//                    targetStyle.ScrollbarVerticalHandleBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticalhandleimage":
//                    targetStyle.ScrollbarVerticalHandleImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Increment
//                case "scrollbarverticalincrementsize":
//                    targetStyle.ScrollbarVerticalIncrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalincrementcolor":
//                    targetStyle.ScrollbarVerticalIncrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticalincrementborderradius":
//                    targetStyle.ScrollbarVerticalIncrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalincrementbordersize":
//                    targetStyle.ScrollbarVerticalIncrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticalincrementbordercolor":
//                    targetStyle.ScrollbarVerticalIncrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticalincrementimage":
//                    targetStyle.ScrollbarVerticalIncrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Decrement
//                case "scrollbarverticaldecrementsize":
//                    targetStyle.ScrollbarVerticalDecrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaldecrementcolor":
//                    targetStyle.ScrollbarVerticalDecrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticaldecrementborderradius":
//                    targetStyle.ScrollbarVerticalDecrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaldecrementbordersize":
//                    targetStyle.ScrollbarVerticalDecrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarverticaldecrementbordercolor":
//                    targetStyle.ScrollbarVerticalDecrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarverticaldecrementimage":
//                    targetStyle.ScrollbarVerticalDecrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//
//                // General
//                case "scrollbarhorizontalbuttonplacement":
//                    targetStyle.ScrollbarHorizontalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(propertyValue);
//                    break;
//                case "scrollbarhorizontalattachment":
//                    targetStyle.ScrollbarHorizontalAttachment = ParseUtil.ParseScrollbarHorizontalAttachment(propertyValue);
//                    break;
//
//                // Track
//                case "scrollbarhorizontaltracksize":
//                    targetStyle.ScrollbarHorizontalTrackSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaltrackcolor":
//                    targetStyle.ScrollbarHorizontalTrackColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontaltrackborderradius":
//                    targetStyle.ScrollbarHorizontalTrackBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaltrackbordersize":
//                    targetStyle.ScrollbarHorizontalTrackBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaltrackbordercolor":
//                    targetStyle.ScrollbarHorizontalTrackBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontaltrackimage":
//                    targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Handle
//                case "scrollbarhorizontalhandlesize":
//                    targetStyle.ScrollbarHorizontalHandleSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalhandlecolor":
//                    targetStyle.ScrollbarHorizontalHandleColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontalhandleborderradius":
//                    targetStyle.ScrollbarHorizontalHandleBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalhandlebordersize":
//                    targetStyle.ScrollbarHorizontalHandleBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalhandlebordercolor":
//                    targetStyle.ScrollbarHorizontalHandleBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontalhandleimage":
//                    targetStyle.ScrollbarHorizontalHandleImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Increment
//                case "scrollbarhorizontalincrementsize":
//                    targetStyle.ScrollbarHorizontalIncrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalincrementcolor":
//                    targetStyle.ScrollbarHorizontalIncrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontalincrementborderradius":
//                    targetStyle.ScrollbarHorizontalIncrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalincrementbordersize":
//                    targetStyle.ScrollbarHorizontalIncrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontalincrementbordercolor":
//                    targetStyle.ScrollbarHorizontalIncrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontalincrementimage":
//                    targetStyle.ScrollbarHorizontalIncrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//
//                // Decrement
//                case "scrollbarhorizontaldecrementsize":
//                    targetStyle.ScrollbarHorizontalDecrementSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaldecrementcolor":
//                    targetStyle.ScrollbarHorizontalDecrementColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontaldecrementborderradius":
//                    targetStyle.ScrollbarHorizontalDecrementBorderRadius = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaldecrementbordersize":
//                    targetStyle.ScrollbarHorizontalDecrementBorderSize = ParseUtil.ParseFloat(propertyValue);
//                    break;
//                case "scrollbarhorizontaldecrementbordercolor":
//                    targetStyle.ScrollbarHorizontalDecrementBorderColor = ParseUtil.ParseColor(propertyValue);
//                    break;
//                case "scrollbarhorizontaldecrementimage":
//                    targetStyle.ScrollbarHorizontalDecrementImage = ParseUtil.ParseTexture(propertyValue);
//                    break;
//            }
//        }

    }
}
