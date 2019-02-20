using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers.Style {
    public struct StylePropertyMappers {

        private static readonly Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> mappers
            = new Dictionary<string, Action<UIStyle, PropertyNode, StyleCompileContext>> {
                {"backgroundcolor", (targetStyle, property, context) => targetStyle.BackgroundColor = MapColor(property, context)},
                {"bordercolor", (targetStyle, property, context) => targetStyle.BorderColor = MapColor(property, context)},
                {"opacity", (targetStyle, property, context) => targetStyle.Opacity = MapNumber(property.children[0], context)}, {
                    "cursor", (targetStyle, property, context) => {
                        // first value must be the reference

                        CursorStyle cursor = null; // new CursorStyle(texturePath, texture, new Vector2(hotSpotX, hotSpotY));
                        targetStyle.Cursor = cursor;
                    }
                },
                {"backgroundimage", (targetStyle, property, context) => targetStyle.BackgroundImage = MapTexture(property)},
                {"overflow", (targetStyle, property, context) => MapOverflows(targetStyle, property, context)},
                {"overflowx", (targetStyle, property, context) => targetStyle.OverflowX = MapOverflow(property.children[0], context)},
                {"overflowy", (targetStyle, property, context) => targetStyle.OverflowY = MapOverflow(property.children[0], context)},
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
                {"visibility", (targetStyle, property, context) => targetStyle.Visibility = MapVisibility(property, context)},
                {"border", (targetStyle, property, context) => MapBorders(targetStyle, property, context)},
                {"bordertop", (targetStyle, property, context) => targetStyle.BorderTop = MapFixedLength(property, context)},
                {"borderright", (targetStyle, property, context) => targetStyle.BorderRight = MapFixedLength(property, context)},
                {"borderbottom", (targetStyle, property, context) => targetStyle.BorderBottom = MapFixedLength(property, context)},
                {"borderleft", (targetStyle, property, context) => targetStyle.BorderLeft = MapFixedLength(property, context)},
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
                {"borderradius", (targetStyle, property, context) => MapBorderRadius(targetStyle, property, context)},                
                {"borderradiustopleft", (targetStyle, property, context) => targetStyle.BorderRadiusTopLeft = MapFixedLength(property.children[0], context)},
                {"borderradiustopright", (targetStyle, property, context) => targetStyle.BorderRadiusTopRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomright", (targetStyle, property, context) => targetStyle.BorderRadiusBottomRight = MapFixedLength(property.children[0], context)},
                {"borderradiusbottomleft", (targetStyle, property, context) => targetStyle.BorderRadiusBottomLeft = MapFixedLength(property.children[0], context)},
            };

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
                    else if (literalNode.type == StyleASTNodeType.NumericLiteral && float.TryParse(literalNode.rawValue, out float number)) {
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

        private static Visibility MapVisibility(PropertyNode property, StyleCompileContext context) {
            StyleASTNode visibilityValueNode = context.GetValueForReference(property.children[0]);
            string visibilityValue = "";
            switch (visibilityValueNode) {
                case StyleIdentifierNode identifierNode:
                    visibilityValue = identifierNode.name;
                    break;
                case StyleLiteralNode literalNode:
                    visibilityValue = literalNode.rawValue;
                    break;
            }

            if (Enum.TryParse(visibilityValue, true, out Visibility visibility)) {
                return visibility;
            }

            throw new CompileException(property.children[0], $"Unexpected value for visibility. Please choose one of {EnumValues(typeof(Visibility))}.");
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
                    if (float.TryParse(measurementNode.value.rawValue, out float measurementValue)) {
                        return new UIMeasurement(measurementValue, MapUnit(measurementNode.unit));
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (float.TryParse(literalNode.rawValue, out float literalValue)) {
                        return new UIMeasurement(literalValue);
                    }

                    break;
            }

            throw new CompileException(value, "Cannot parse value, expected a numeric literal or measurement.");
        }

        private static UIFixedLength MapFixedLength(StyleASTNode value, StyleCompileContext context) {
            value = context.GetValueForReference(value);
            switch (value) {
                case MeasurementNode measurementNode:
                    if (float.TryParse(measurementNode.value.rawValue, out float measurementValue)) {
                        return new UIFixedLength(measurementValue, MapFixedUnit(measurementNode.unit));
                    }

                    break;

                case StyleLiteralNode literalNode:
                    if (float.TryParse(literalNode.rawValue, out float literalValue)) {
                        return new UIFixedLength(literalValue);
                    }

                    break;
            }

            throw new CompileException(value, "Cannot parse value, expected a numeric literal or measurement.");
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
                case "min":
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
                             "Try px, mx, min, em, vw, vh, cca, fr or cnt instead (see GridTemplateUnit). Will fall back to px.");

            return GridTemplateUnit.Pixel;
        }

        private static void MapOverflows(UIStyle targetStyle, PropertyNode property, StyleCompileContext context) {
            Overflow overflowX = MapOverflow(property.children[0], context);
            Overflow overflowY = overflowX;

            if (property.children.Count == 2) {
                overflowY = MapOverflow(property.children[1], context);
            }

            // should we check for more than 2 values and log a warning?

            targetStyle.OverflowX = overflowX;
            targetStyle.OverflowY = overflowY;
        }

        private static Overflow MapOverflow(StyleASTNode valueNode, StyleCompileContext context) {
            StyleASTNode value = context.GetValueForReference(valueNode);
            if (value is StyleLiteralNode literalNode) {
                return ParseOverflowFromLiteralNode(literalNode);
            }

            throw new CompileException(value, "Couldn't parse overflow value.");
        }

        private static Overflow ParseOverflowFromLiteralNode(StyleLiteralNode node) {
            if (!Enum.TryParse(node.rawValue, true, out Overflow overflow)) {
                throw new CompileException(node, $"Unknown value overflow. Possible values: {EnumValues(typeof(Overflow))}");
            }

            return overflow;
        }

        private static Texture2D MapTexture(PropertyNode property) {
            LightList<StyleASTNode> propertyValues = property.children;
            AssertSingleValue(propertyValues);
            switch (propertyValues[0]) {
                case UrlNode urlNode:
                    return ResourceManager.GetTexture(TransformUrlNode(urlNode));
                default:
                    throw new CompileException(propertyValues[0], "Expected url(path/to/texture).");
            }
        }

        private static string TransformUrlNode(UrlNode urlNode) {
            StyleASTNode url = urlNode.url;

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
                if (float.TryParse(identifierNode.name, out float number)) {
                    return number;
                }
            }

            if (node.type == StyleASTNodeType.NumericLiteral) {
                if (float.TryParse(((StyleLiteralNode) node).rawValue, out float number)) {
                    return number;
                }
            }

            throw new CompileException(node, $"Expected a numeric value but all I got was this lousy {node}");
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

        private static void AssertSingleValue(LightList<StyleASTNode> propertyValues) {
            if (propertyValues.Count > 1) {
                throw new CompileException(propertyValues[1], "Found too many values.");
            }
        }
    }
}
