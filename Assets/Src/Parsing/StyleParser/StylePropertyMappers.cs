using System;
using System.Diagnostics.CodeAnalysis;
using Src.Rendering;
using UIForia;
using UnityEngine;

namespace Src.Parsing.StyleParser {

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static class StylePropertyMappers {

        public static void DisplayMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "backgroundcolor":
                    context.targetStyle.BackgroundColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "bordercolor":
                    context.targetStyle.BorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "opacity":
                    throw new NotImplementedException();
                case "cursor":
                    throw new NotImplementedException();
                case "backgroundimage":
                    context.targetStyle.BackgroundImage = UIForia.ResourceManager.GetTexture(propertyValue);
                    break;
                case "overflow":
                    Overflow overflow = ParseUtil.ParseOverflow(context.variables, propertyValue);
                    context.targetStyle.OverflowX = overflow;
                    context.targetStyle.OverflowY = overflow;
                    break;
                case "overflowx":
                    context.targetStyle.OverflowX = ParseUtil.ParseOverflow(context.variables, propertyValue);
                    break;
                case "overflowy":
                    context.targetStyle.OverflowY = ParseUtil.ParseOverflow(context.variables, propertyValue);
                    break;
                case "backgroundfilltype":
                    throw new INeedToGoToBedException();
//                    context.targetStyle.BackgroundFillType = ParseUtil.ParseBackgroundFillType(context.variables, propertyValue);
                    break;
                
                default:
                    throw new ParseException("Unknown display property: " + propertyName);
            }
        }

        public class INeedToGoToBedException : Exception { }

        public static void MarginMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "margin":
                    ContentBoxRect rect = ParseUtil.ParseMeasurementRect(context.variables, propertyValue);
                    context.targetStyle.MarginTop = rect.top;
                    context.targetStyle.MarginRight = rect.right;
                    context.targetStyle.MarginBottom = rect.bottom;
                    context.targetStyle.MarginLeft = rect.left;
                    break;
                case "margintop":
                    context.targetStyle.MarginTop = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "marginright":
                    context.targetStyle.MarginRight = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "marginbottom":
                    context.targetStyle.MarginBottom = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "marginleft":
                    context.targetStyle.MarginLeft = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown margin property: " + propertyName);
            }
        }

        public static void PaddingBorderMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "padding":
                    FixedLengthRect rect  = ParseUtil.ParseFixedLengthRect(context.variables, propertyValue);
                    context.targetStyle.PaddingTop = rect.top;
                    context.targetStyle.PaddingRight = rect.right;
                    context.targetStyle.PaddingBottom = rect.bottom;
                    context.targetStyle.PaddingLeft = rect.left;
                    break;
                case "paddingtop":
                    context.targetStyle.PaddingTop = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "paddingright":
                    context.targetStyle.PaddingRight = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "paddingbottom":
                    context.targetStyle.PaddingBottom = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "paddingleft":
                    context.targetStyle.PaddingLeft = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "border":
                    rect = ParseUtil.ParseFixedLengthRect(context.variables, propertyValue);
                    context.targetStyle.BorderTop = rect.top;
                    context.targetStyle.BorderRight = rect.right;
                    context.targetStyle.BorderBottom = rect.bottom;
                    context.targetStyle.BorderLeft = rect.left;
                    break;
                case "bordertop":
                    context.targetStyle.BorderTop = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderright":
                    context.targetStyle.BorderRight = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderbottom":
                    context.targetStyle.BorderBottom = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderleft":
                    context.targetStyle.BorderLeft = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown grid padding or border property: " + propertyName);
            }
        }

        public static void GridItemMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "griditemcolstart":
                    context.targetStyle.GridItemColStart = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "griditemcolspan":
                    context.targetStyle.GridItemColSpan = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "griditemrowstart":
                    context.targetStyle.GridItemRowStart = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "griditemrowspan":
                    context.targetStyle.GridItemRowSpan = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "griditemcolselfalign":
                    context.targetStyle.GridItemColSelfAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                case "griditemrowselfalign":
                    context.targetStyle.GridItemRowSelfAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown grid item property: " + propertyName);
            }
        }

        public static void GridLayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "gridlayoutdirection":
                    context.targetStyle.GridLayoutDirection = ParseUtil.ParseLayoutDirection(context.variables, propertyValue);
                    break;
                case "gridlayoutdensity":
                    context.targetStyle.GridLayoutDensity = ParseUtil.ParseDensity(context.variables, propertyValue);
                    break;
                case "gridlayoutcoltemplate":
                    context.targetStyle.GridLayoutColTemplate = ParseUtil.ParseGridTemplate(context.variables, propertyValue);
                    break;
                case "gridlayoutrowtemplate":
                    context.targetStyle.GridLayoutRowTemplate = ParseUtil.ParseGridTemplate(context.variables, propertyValue);
                    break;
                case "gridlayoutcolautosize":
                    context.targetStyle.GridLayoutColAutoSize = ParseUtil.ParseGridTrackSize(context.variables, propertyValue);
                    break;
                case "gridlayoutrowautosize":
                    context.targetStyle.GridLayoutRowAutoSize = ParseUtil.ParseGridTrackSize(context.variables, propertyValue);
                    break;
                case "gridlayoutcolgap":
                    context.targetStyle.GridLayoutColGap = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "gridlayoutrowgap":
                    context.targetStyle.GridLayoutColGap = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "gridlayoutcolalignment":
                    context.targetStyle.GridLayoutColAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                case "gridlayoutrowalignment":
                    context.targetStyle.GridLayoutRowAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown grid layout property: " + propertyName);
            }
        }

        public static void FlexItemMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "flexitemselfalignment":
                    context.targetStyle.FlexItemSelfAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                case "flexitemorder":
                    context.targetStyle.FlexItemOrder = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "flexitemgrow":
                    context.targetStyle.FlexItemGrow = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "flexitemshrink":
                    context.targetStyle.FlexItemShrink = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown flex item property: " + propertyName);
            }
        }

        public static void FlexLayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "flexlayoutwrap":
                    context.targetStyle.FlexLayoutWrap = ParseUtil.ParseLayoutWrap(context.variables, propertyValue);
                    break;
                case "flexlayoutdirection":
                    context.targetStyle.FlexLayoutDirection = ParseUtil.ParseLayoutDirection(context.variables, propertyValue);
                    break;
                case "flexlayoutmainaxisalignment":
                    context.targetStyle.FlexLayoutMainAxisAlignment = ParseUtil.ParseMainAxisAlignment(context.variables, propertyValue);
                    break;
                case "flexlayoutcrossaxisalignment":
                    context.targetStyle.FlexLayoutCrossAxisAlignment = ParseUtil.ParseCrossAxisAlignment(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown flex item property: " + propertyName);
            }
        }

        public static void BorderRadiusMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "borderradius":
                    FixedLengthRect rect = ParseUtil.ParseFixedLengthRect(context.variables, propertyValue);
                    BorderRadius radius = new BorderRadius(rect.top, rect.right, rect.bottom, rect.left);
                    context.targetStyle.BorderRadius = radius;
                    break;
                case "borderradiustopleft":
                    context.targetStyle.BorderRadiusTopLeft = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderradiustopright":
                    context.targetStyle.BorderRadiusTopRight = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderradiusbottomright":
                    context.targetStyle.BorderRadiusBottomRight = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "borderradiusbottomleft":
                    context.targetStyle.BorderRadiusBottomLeft = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown border radius property: " + propertyName);
            }
        }

        public static void TransformMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "transformposition":
                    throw new NotImplementedException();
                case "transformpositionx":
                    context.targetStyle.TransformPositionX = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "transformpositiony":
                    context.targetStyle.TransformPositionY = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "transformscale":
                    float scale = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.TransformScaleX = scale;
                    context.targetStyle.TransformScaleY = scale;
                    break;
                case "transformscalex":
                    context.targetStyle.TransformScaleX = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "transformscaley":
                    context.targetStyle.TransformScaleY = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "transformpivotx":
                    context.targetStyle.TransformPivotX = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "transformpivoty":
                    context.targetStyle.TransformPivotY = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "transformrotation":
                    // todo -- handle deg / rad / and maybe %
                    context.targetStyle.TransformRotation = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "transformbehaviorx":
                    context.targetStyle.TransformBehaviorX = ParseUtil.ParseTransformBehavior(context.variables, propertyValue);
                    break;
                case "transformbehaviory":
                    context.targetStyle.TransformBehaviorY = ParseUtil.ParseTransformBehavior(context.variables, propertyValue);
                    break;
                default:
                    throw new ParseException("Unknown border radius property: " + propertyName);
            }
        }

        public static void SizeMapper(StyleParserContext context, string propertyName, string propertyValue) {
            MeasurementPair pair;
            switch (propertyName.ToLower()) {
                case "minwidth":
                    context.targetStyle.MinWidth = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "minheight":
                    context.targetStyle.MinHeight = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "preferredwidth":
                    context.targetStyle.PreferredWidth = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "preferredheight":
                    context.targetStyle.PreferredHeight = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "maxwidth":
                    context.targetStyle.MaxWidth = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "maxheight":
                    context.targetStyle.MaxHeight = ParseUtil.ParseMeasurement(context.variables, propertyValue);
                    break;
                case "preferredsize":
                    pair = ParseUtil.ParseMeasurementPair(context.variables, propertyValue);
                    context.targetStyle.PreferredWidth = pair.x;
                    context.targetStyle.PreferredHeight = pair.y;
                    break;
                case "minsize":
                    pair = ParseUtil.ParseMeasurementPair(context.variables, propertyValue);
                    context.targetStyle.MinWidth = pair.x;
                    context.targetStyle.MinHeight = pair.y;
                    break;
                case "maxsize":
                    pair = ParseUtil.ParseMeasurementPair(context.variables, propertyValue);
                    context.targetStyle.MaxWidth = pair.x;
                    context.targetStyle.MaxHeight = pair.y;
                    break;
                default:
                    throw new ParseException("Unknown size property: " + propertyName);
            }
        }

        public static void LayoutMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "layouttype":
                    context.targetStyle.LayoutType = ParseUtil.ParseLayoutType(context.variables, propertyValue);
                    break;
                case "layoutbehavior":
                    context.targetStyle.LayoutBehavior = ParseUtil.ParseLayoutBehavior(context.variables, propertyValue);
                    break;
                case "anchortarget":
                    context.targetStyle.AnchorTarget = ParseUtil.ParseAnchorTarget(context.variables, propertyValue);
                    break;
                case "anchortop":
                    context.targetStyle.AnchorTop = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "anchorright":
                    context.targetStyle.AnchorRight = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "anchorbottom":
                    context.targetStyle.AnchorBottom = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "anchorleft":
                    context.targetStyle.AnchorLeft = ParseUtil.ParseFixedLength(context.variables, propertyValue);
                    break;
                case "zindex":
                    context.targetStyle.ZIndex = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                case "renderlayer":
                    context.targetStyle.RenderLayer = ParseUtil.ParseRenderLayer(context.variables, propertyValue);
                    break;
                case "renderlayeroffset":
                    context.targetStyle.RenderLayerOffset = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
            }
        }

        public static void TextMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                case "textcolor":
                    context.targetStyle.TextColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "textfontasset":
                    context.targetStyle.TextFontAsset = ParseUtil.ParseFont(context.variables, propertyValue);
                    break;
                case "textfontstyle":
                    context.targetStyle.TextFontStyle = ParseUtil.ParseFontStyle(context.variables, propertyValue);
                    break;
                case "textfontsize":
                    context.targetStyle.TextFontSize = ParseUtil.ParseInt(context.variables, propertyValue);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

    }

}