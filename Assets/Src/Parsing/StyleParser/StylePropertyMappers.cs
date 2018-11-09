using System;
using System.Diagnostics.CodeAnalysis;
using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Parsing.StyleParser {

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
                    FixedLengthRect rect = ParseUtil.ParseFixedLengthRect(context.variables, propertyValue);
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
                    FixedLengthVector length = ParseUtil.ParseFixedLengthPair(context.variables, propertyValue);
                    context.targetStyle.TransformPositionX = length.x;
                    context.targetStyle.TransformPositionY = length.y;
                    break;
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
                case "transformbehavior":
                    TransformBehavior behavior = ParseUtil.ParseTransformBehavior(context.variables, propertyValue);
                    context.targetStyle.TransformBehaviorX = behavior;
                    context.targetStyle.TransformBehaviorY = behavior;
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

        public static void ScrollMapper(StyleParserContext context, string propertyName, string propertyValue) {
            switch (propertyName.ToLower()) {
                // General
                case "scrollbarbuttonplacement":
                    context.targetStyle.ScrollbarVerticalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(context.variables, propertyValue);
                    break;
//                case "scrollbarattachment":
////                    context.targetStyle.ScrollbarVerticalAttachment = ParseUtil.ParseScrollbarVerticalAttachment(context.variables, propertyValue);
//                    break;

                // Track
                case "scrollbartracksize":
                    float size = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarVerticalTrackSize = size;
                    context.targetStyle.ScrollbarHorizontalTrackSize = size;
                    break;
                case "scrollbartrackcolor":
                    Color trackColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarVerticalTrackColor = trackColor;
                    context.targetStyle.ScrollbarHorizontalTrackColor = trackColor;
                    break;
                case "scrollbartrackborderradius":
                    context.targetStyle.ScrollbarVerticalTrackBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalTrackBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbartrackbordersize":
                    context.targetStyle.ScrollbarVerticalTrackBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalTrackBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbartrackbordercolor":
                    context.targetStyle.ScrollbarVerticalTrackBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalTrackBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbartrackimage":
                    context.targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Handle
                case "scrollbarhandlesize":
                    context.targetStyle.ScrollbarVerticalHandleSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalHandleSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhandlecolor":
                    context.targetStyle.ScrollbarVerticalHandleColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalHandleColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhandleborderradius":
                    context.targetStyle.ScrollbarVerticalHandleBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalHandleBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhandlebordersize":
                    context.targetStyle.ScrollbarVerticalHandleBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalHandleBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhandlebordercolor":
                    context.targetStyle.ScrollbarVerticalHandleBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalHandleBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhandleimage":
                    context.targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Increment
                case "scrollbarincrementsize":
                    context.targetStyle.ScrollbarVerticalIncrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarincrementcolor":
                    context.targetStyle.ScrollbarVerticalIncrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarincrementborderradius":
                    context.targetStyle.ScrollbarVerticalIncrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarincrementbordersize":
                    context.targetStyle.ScrollbarVerticalIncrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarincrementbordercolor":
                    context.targetStyle.ScrollbarVerticalIncrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarincrementimage":
                    context.targetStyle.ScrollbarVerticalIncrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalIncrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Decrement
                case "scrollbardecrementsize":
                    context.targetStyle.ScrollbarVerticalDecrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbardecrementcolor":
                    context.targetStyle.ScrollbarVerticalDecrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbardecrementborderradius":
                    context.targetStyle.ScrollbarVerticalDecrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbardecrementbordersize":
                    context.targetStyle.ScrollbarVerticalDecrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbardecrementbordercolor":
                    context.targetStyle.ScrollbarVerticalDecrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbardecrementimage":
                    context.targetStyle.ScrollbarVerticalDecrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    context.targetStyle.ScrollbarHorizontalDecrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // General
                case "scrollbarverticalbuttonplacement":
                    context.targetStyle.ScrollbarVerticalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(context.variables, propertyValue);
                    break;
                case "scrollbarverticalattachment":
                    context.targetStyle.ScrollbarVerticalAttachment = ParseUtil.ParseScrollbarVerticalAttachment(context.variables, propertyValue);
                    break;

                // Track
                case "scrollbarverticaltracksize":
                    context.targetStyle.ScrollbarVerticalTrackSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaltrackcolor":
                    context.targetStyle.ScrollbarVerticalTrackColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticaltrackborderradius":
                    context.targetStyle.ScrollbarVerticalTrackBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaltrackbordersize":
                    context.targetStyle.ScrollbarVerticalTrackBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaltrackbordercolor":
                    context.targetStyle.ScrollbarVerticalTrackBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticaltrackimage":
                    context.targetStyle.ScrollbarVerticalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Handle
                case "scrollbarverticalhandlesize":
                    context.targetStyle.ScrollbarVerticalHandleSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalhandlecolor":
                    context.targetStyle.ScrollbarVerticalHandleColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticalhandleborderradius":
                    context.targetStyle.ScrollbarVerticalHandleBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalhandlebordersize":
                    context.targetStyle.ScrollbarVerticalHandleBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalhandlebordercolor":
                    context.targetStyle.ScrollbarVerticalHandleBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticalhandleimage":
                    context.targetStyle.ScrollbarVerticalHandleImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Increment
                case "scrollbarverticalincrementsize":
                    context.targetStyle.ScrollbarVerticalIncrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalincrementcolor":
                    context.targetStyle.ScrollbarVerticalIncrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticalincrementborderradius":
                    context.targetStyle.ScrollbarVerticalIncrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalincrementbordersize":
                    context.targetStyle.ScrollbarVerticalIncrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticalincrementbordercolor":
                    context.targetStyle.ScrollbarVerticalIncrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticalincrementimage":
                    context.targetStyle.ScrollbarVerticalIncrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Decrement
                case "scrollbarverticaldecrementsize":
                    context.targetStyle.ScrollbarVerticalDecrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaldecrementcolor":
                    context.targetStyle.ScrollbarVerticalDecrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticaldecrementborderradius":
                    context.targetStyle.ScrollbarVerticalDecrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaldecrementbordersize":
                    context.targetStyle.ScrollbarVerticalDecrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarverticaldecrementbordercolor":
                    context.targetStyle.ScrollbarVerticalDecrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarverticaldecrementimage":
                    context.targetStyle.ScrollbarVerticalDecrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;


                // General
                case "scrollbarhorizontalbuttonplacement":
                    context.targetStyle.ScrollbarHorizontalButtonPlacement = ParseUtil.ParseScrollbarButtonPlacement(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalattachment":
                    context.targetStyle.ScrollbarHorizontalAttachment = ParseUtil.ParseScrollbarHorizontalAttachment(context.variables, propertyValue);
                    break;

                // Track
                case "scrollbarhorizontaltracksize":
                    context.targetStyle.ScrollbarHorizontalTrackSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaltrackcolor":
                    context.targetStyle.ScrollbarHorizontalTrackColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaltrackborderradius":
                    context.targetStyle.ScrollbarHorizontalTrackBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaltrackbordersize":
                    context.targetStyle.ScrollbarHorizontalTrackBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaltrackbordercolor":
                    context.targetStyle.ScrollbarHorizontalTrackBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaltrackimage":
                    context.targetStyle.ScrollbarHorizontalTrackImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Handle
                case "scrollbarhorizontalhandlesize":
                    context.targetStyle.ScrollbarHorizontalHandleSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalhandlecolor":
                    context.targetStyle.ScrollbarHorizontalHandleColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalhandleborderradius":
                    context.targetStyle.ScrollbarHorizontalHandleBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalhandlebordersize":
                    context.targetStyle.ScrollbarHorizontalHandleBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalhandlebordercolor":
                    context.targetStyle.ScrollbarHorizontalHandleBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalhandleimage":
                    context.targetStyle.ScrollbarHorizontalHandleImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Increment
                case "scrollbarhorizontalincrementsize":
                    context.targetStyle.ScrollbarHorizontalIncrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalincrementcolor":
                    context.targetStyle.ScrollbarHorizontalIncrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalincrementborderradius":
                    context.targetStyle.ScrollbarHorizontalIncrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalincrementbordersize":
                    context.targetStyle.ScrollbarHorizontalIncrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalincrementbordercolor":
                    context.targetStyle.ScrollbarHorizontalIncrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontalincrementimage":
                    context.targetStyle.ScrollbarHorizontalIncrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;

                // Decrement
                case "scrollbarhorizontaldecrementsize":
                    context.targetStyle.ScrollbarHorizontalDecrementSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaldecrementcolor":
                    context.targetStyle.ScrollbarHorizontalDecrementColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaldecrementborderradius":
                    context.targetStyle.ScrollbarHorizontalDecrementBorderRadius = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaldecrementbordersize":
                    context.targetStyle.ScrollbarHorizontalDecrementBorderSize = ParseUtil.ParseFloat(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaldecrementbordercolor":
                    context.targetStyle.ScrollbarHorizontalDecrementBorderColor = ParseUtil.ParseColor(context.variables, propertyValue);
                    break;
                case "scrollbarhorizontaldecrementimage":
                    context.targetStyle.ScrollbarHorizontalDecrementImage = ParseUtil.ParseTexture(context.variables, propertyValue);
                    break;
            }
        }

    }

}