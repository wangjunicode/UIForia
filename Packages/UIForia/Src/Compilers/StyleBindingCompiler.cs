using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UIForia.Compilers.AliasSource;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.StyleBindings;
using UIForia.StyleBindings.Src.StyleBindings;
using UIForia.StyleBindings.Text;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using Object = System.Object;

namespace UIForia.Compilers {

    public class StyleBindingCompiler {

        private ContextDefinition context;
        private readonly ExpressionCompiler compiler;

        private static readonly MethodAliasSource rgbSource;
        private static readonly MethodAliasSource rgbaSource;
        private static readonly MethodAliasSource rect1Source;
        private static readonly MethodAliasSource rect2Source;
        private static readonly MethodAliasSource rect4Source;

        private static readonly MethodAliasSource sizeAliasSource;
        private static readonly MethodAliasSource vec2MeasurementSource;
        private static readonly MethodAliasSource vec2FixedLengthSource;

        private static readonly MethodAliasSource borderRadiusRect1Source;
        private static readonly MethodAliasSource borderRadiusRect2Source;
        private static readonly MethodAliasSource borderRadiusRect4Source;

        private static readonly MethodAliasSource parentMeasurementSource;
        private static readonly MethodAliasSource contentMeasurementSource;
        private static readonly MethodAliasSource viewportWidthMeasurementSource;
        private static readonly MethodAliasSource viewportHeightMeasurementSource;
        private static readonly MethodAliasSource pixelMeasurementSource;

        private static readonly ColorAliasSource colorSource;

        private static readonly MethodAliasSource textureUrlSource;
        private static readonly MethodAliasSource fontUrlSource;

        private static readonly EnumAliasSource<LayoutType> layoutTypeSource;
        private static readonly EnumAliasSource<LayoutDirection> layoutDirectionSource;
        private static readonly EnumAliasSource<LayoutFlowType> layoutFlowSource;
        private static readonly EnumAliasSource<LayoutWrap> layoutWrapSource;
        private static readonly EnumAliasSource<MainAxisAlignment> mainAxisAlignmentSource;
        private static readonly EnumAliasSource<CrossAxisAlignment> crossAxisAlignmentSource;
        private static readonly EnumAliasSource<WhitespaceMode> whiteSpaceSource;

        // todo 
        private static readonly ValueAliasSource<int> siblingIndexSource;
        private static readonly ValueAliasSource<int> activeSiblingIndexSource;
        private static readonly ValueAliasSource<int> childCountSource;
        private static readonly ValueAliasSource<int> activeChildCountSource;
        private static readonly ValueAliasSource<int> inactiveChildCountSource;
        private static readonly ValueAliasSource<int> templateChildChildCountSource;

        private static readonly IAliasSource[] fixedSources;
        private static readonly IAliasSource[] measurementSources;

        
        // todo implement these globally in bindings
        private static readonly string[] k_ElementProperties = {
            "$childIndex",
            "$childCount",
            "$activeChildIndex",
            "$inactiveChildCount",
            "$activeChildCount",
            "$hasActiveChildren",
            "$hasInactiveChildren",
            "$hasChildren"
        };

        static StyleBindingCompiler() {
            Type type = typeof(StyleBindingCompiler);
            rgbSource = new MethodAliasSource("rgb", type.GetMethod("Rgb", ReflectionUtil.PublicStatic));
            rgbaSource = new MethodAliasSource("rgba", type.GetMethod("Rgba", ReflectionUtil.PublicStatic));

            rect1Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float)}));
            rect2Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float), typeof(float)}));
            rect4Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float), typeof(float), typeof(float), typeof(float)}));

            textureUrlSource = new MethodAliasSource("url", type.GetMethod(nameof(TextureUrl)));
            fontUrlSource = new MethodAliasSource("url", type.GetMethod(nameof(FontUrl)));

            sizeAliasSource = new MethodAliasSource("size", type.GetMethod(nameof(Size)));
            vec2MeasurementSource = new MethodAliasSource("vec2", type.GetMethod(nameof(Vec2Measurement)));
            vec2FixedLengthSource = new MethodAliasSource("vec2", type.GetMethod(nameof(Vec2FixedLength)));
            borderRadiusRect1Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float)}));
            borderRadiusRect2Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float), typeof(float)}));
            borderRadiusRect4Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float), typeof(float), typeof(float), typeof(float)}));

            pixelMeasurementSource = new MethodAliasSource("pixels", type.GetMethod(nameof(PixelMeasurement), new[] {typeof(float)}));
            parentMeasurementSource = new MethodAliasSource("parent", type.GetMethod(nameof(ParentMeasurement), new[] {typeof(float)}));
            viewportWidthMeasurementSource = new MethodAliasSource("viewWidth", type.GetMethod(nameof(ViewportWidthMeasurement), new[] {typeof(float)}));
            viewportHeightMeasurementSource = new MethodAliasSource("viewHeight", type.GetMethod(nameof(ViewportHeightMeasurement), new[] {typeof(float)}));
            contentMeasurementSource = new MethodAliasSource("content", type.GetMethod(nameof(ContentMeasurement), new[] {typeof(float)}));

            MethodAliasSource percentageLengthSource = new MethodAliasSource("percent", type.GetMethod(nameof(PercentageLength), new[] {typeof(float)}));
            MethodAliasSource emLengthSource = new MethodAliasSource("em", type.GetMethod(nameof(EmLength), new[] {typeof(float)}));
            MethodAliasSource viewportWidthLengthSource = new MethodAliasSource("vw", type.GetMethod(nameof(ViewportWidthLength)));
            MethodAliasSource viewportHeightLengthSource = new MethodAliasSource("vh", type.GetMethod(nameof(ViewportHeightLength)));
            MethodAliasSource pixelLengthSource = new MethodAliasSource("px", type.GetMethod(nameof(PixelLength)));

            measurementSources = new IAliasSource[] {
                pixelMeasurementSource,
                parentMeasurementSource,
                viewportWidthMeasurementSource,
                viewportHeightMeasurementSource,
                contentMeasurementSource,
            };
            
            fixedSources = new IAliasSource[] {
                pixelLengthSource,
                percentageLengthSource,
                emLengthSource,
                viewportWidthLengthSource,
                viewportHeightLengthSource
            };

            colorSource = new ColorAliasSource();
            layoutTypeSource = new EnumAliasSource<LayoutType>();
            layoutDirectionSource = new EnumAliasSource<LayoutDirection>();
            layoutFlowSource = new EnumAliasSource<LayoutFlowType>();
            layoutWrapSource = new EnumAliasSource<LayoutWrap>();
            mainAxisAlignmentSource = new EnumAliasSource<MainAxisAlignment>();
            crossAxisAlignmentSource = new EnumAliasSource<CrossAxisAlignment>();
            whiteSpaceSource = new EnumAliasSource<WhitespaceMode>();
        }

        public StyleBindingCompiler(ContextDefinition context) {
            this.context = context;
            this.compiler = new ExpressionCompiler(context);
        }

        public void SetContext(ContextDefinition context) {
            this.context = context;
            this.compiler.SetContext(context);
        }

        public StyleBinding Compile(AttributeDefinition attributeDefinition) {
            return Compile(context, attributeDefinition.key, attributeDefinition.value);
        }

        public StyleBinding Compile(ContextDefinition context, string key, string value) {
            SetContext(context);
            return Compile(key, value);
        }

        public StyleBinding Compile(string key, string value) {
            if (!key.StartsWith("style.")) return null;

            // todo -- drop this restriction if possible
            if (value[0] != '{') {
                value = '{' + value + '}';
            }

            Target targetState = GetTargetState(key);

            switch (targetState.property) {
                // Paint
                case RenderConstants.BackgroundImage:
                    return new StyleBinding_BackgroundImage(targetState.state, Compile<Texture2D>(value, textureUrlSource));

                case RenderConstants.BackgroundColor:
                    return new StyleBinding_BackgroundColor(targetState.state,
                        Compile<Color>(value, rgbSource, rgbaSource, colorSource));

                case RenderConstants.BorderColor:
                    return new StyleBinding_BorderColor(targetState.state, Compile<Color>(value, rgbSource, rgbaSource, colorSource));

                case RenderConstants.BorderRadius:
                    return new StyleBinding_BorderRadius(targetState.state, Compile<BorderRadius>(
                        value,
                        borderRadiusRect1Source,
                        borderRadiusRect2Source,
                        borderRadiusRect4Source
                    ));

                case RenderConstants.BorderRadiusTopLeft:
                    return new StyleBinding_BorderRadius_TopLeft(targetState.state, Compile<UIFixedLength>(value));

                case RenderConstants.BorderRadiusTopRight:
                    return new StyleBinding_BorderRadius_TopRight(targetState.state, Compile<UIFixedLength>(value));

                case RenderConstants.BorderRadiusBottomRight:
                    return new StyleBinding_BorderRadius_BottomRight(targetState.state, Compile<UIFixedLength>(value));

                case RenderConstants.BorderRadiusBottomLeft:
                    return new StyleBinding_BorderRadius_BottomLeft(targetState.state, Compile<UIFixedLength>(value));

                // Transform
                case RenderConstants.Translation:
                    return new StyleBinding_Translation(targetState.state, Compile<FixedLengthVector>(value, vec2FixedLengthSource));

                case RenderConstants.Rotation:
                    throw new NotImplementedException();

                case RenderConstants.Pivot:
                    throw new NotImplementedException();

                case RenderConstants.Scale:
                    throw new NotImplementedException();

                // Rect
                case RenderConstants.Size:
                    throw new NotImplementedException();
//                    return new StyleBinding_Dimensions(targetState.state, Compile<Dimensions>(
//                        value,
//                        sizeAliasSource
//                    ));
                    break;

                case RenderConstants.Width:
                    return new StyleBinding_Width(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                case RenderConstants.Height:
                    return new StyleBinding_Height(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                // Constraints

                case RenderConstants.MinWidth:
                    return new StyleBinding_MinWidth(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                case RenderConstants.MaxWidth:
                    return new StyleBinding_MaxWidth(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                case RenderConstants.MinHeight:
                    return new StyleBinding_MinHeight(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                case RenderConstants.MaxHeight:
                    return new StyleBinding_MaxHeight(targetState.state, Compile<UIMeasurement>(value,measurementSources));

                case RenderConstants.GrowthFactor:
                    return new StyleBinding_GrowthFactor(targetState.state, Compile<int>(value));

                case RenderConstants.ShrinkFactor:
                    return new StyleBinding_ShrinkFactor(targetState.state, Compile<int>(value));

                // Layout

//                case RenderConstants.OverflowX:
//                    return new StyleBinding_OverflowX(targetState.state);

                case RenderConstants.MainAxisAlignment:
                    return new StyleBinding_FlexLayoutMainAxisAlignment(targetState.state, Compile<MainAxisAlignment>(value, mainAxisAlignmentSource));

                case RenderConstants.CrossAxisAlignment:
                    return new StyleBinding_FlexLayoutCrossAxisAlignment(targetState.state, Compile<CrossAxisAlignment>(value, crossAxisAlignmentSource));

                case RenderConstants.LayoutDirection:
                    return new StyleBinding_FlexLayoutDirection(targetState.state, Compile<LayoutDirection>(value, layoutDirectionSource));

                case RenderConstants.LayoutType:
                    return new StyleBinding_LayoutType(targetState.state, Compile<LayoutType>(value, layoutTypeSource));

                case RenderConstants.LayoutWrap:
                    return new StyleBinding_FlexLayoutWrap(targetState.state, Compile<LayoutWrap>(value, layoutWrapSource));

                // Padding

                case RenderConstants.Padding:
                    return new StyleBinding_Padding(targetState.state, Compile<FixedLengthRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.PaddingTop:
                    return new StyleBinding_PaddingTop(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.PaddingRight:
                    return new StyleBinding_PaddingRight(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.PaddingBottom:
                    return new StyleBinding_PaddingBottom(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.PaddingLeft:
                    return new StyleBinding_PaddingLeft(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                // Border

                case RenderConstants.Border:
                    return new StyleBinding_Border(targetState.state, Compile<FixedLengthRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.BorderTop:
                    return new StyleBinding_BorderTop(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.BorderRight:
                    return new StyleBinding_BorderRight(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.BorderBottom:
                    return new StyleBinding_BorderBottom(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                case RenderConstants.BorderLeft:
                    return new StyleBinding_BorderLeft(targetState.state, Compile<UIFixedLength>(value, fixedSources));

                // Margin

                case RenderConstants.Margin:
                    return new StyleBinding_Margin(targetState.state, Compile<ContentBoxRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.MarginTop:
                    return new StyleBinding_MarginTop(targetState.state, Compile<UIMeasurement>(value, measurementSources));

                case RenderConstants.MarginRight:
                    return new StyleBinding_MarginRight(targetState.state, Compile<UIMeasurement>(value, measurementSources));

                case RenderConstants.MarginBottom:
                    return new StyleBinding_MarginBottom(targetState.state, Compile<UIMeasurement>(value, measurementSources));

                case RenderConstants.MarginLeft:
                    return new StyleBinding_MarginLeft(targetState.state, Compile<UIMeasurement>(value, measurementSources));
                
                // Text

                case RenderConstants.TextColor:
                    return new StyleBinding_TextColor(targetState.state, Compile<Color>(value, colorSource, rgbSource, rgbaSource));

                case RenderConstants.FontSize:
                    return new StyleBinding_FontSize(targetState.state, Compile<int>(value));

                case RenderConstants.Whitespace:
                    return new StyleBinding_Whitespace(targetState.state, Compile<WhitespaceMode>(value, whiteSpaceSource));

                case RenderConstants.Font:
                    return new StyleBinding_Font(targetState.state, Compile<TMP_FontAsset>(value, fontUrlSource));
                    
                default: return null;
            }
        }

        private static Target GetTargetState(string key) {
            if (key.StartsWith("style.hover.")) {
                return new Target(key.Substring("style.hover.".Length), StyleState.Hover);
            }

            if (key.StartsWith("style.disabled.")) {
                return new Target(key.Substring("style.disabled.".Length), StyleState.Inactive);
            }

            if (key.StartsWith("style.focused.")) {
                return new Target(key.Substring("style.focused.".Length), StyleState.Focused);
            }

            if (key.StartsWith("style.active.")) {
                return new Target(key.Substring("style.active.".Length), StyleState.Active);
            }

            if (key.StartsWith("style.")) {
                return new Target(key.Substring("style.".Length), StyleState.Normal);
            }

            throw new Exception("invalid target state");
        }

        private Expression<T> Compile<T>(string value, params IAliasSource[] sources) {
            for (int i = 0; i < sources.Length; i++) {
                context.AddConstAliasSource(sources[i]);
            }

            // for each intrinsic 
            Expression<T> expression = compiler.Compile<T>(value);

            for (int i = 0; i < sources.Length; i++) {
                context.RemoveConstAliasSource(sources[i]);
            }

            return expression;
        }

        [Pure]
        public static Color Rgb(float r, float g, float b) {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        [Pure]
        public static Color Rgba(float r, float g, float b, float a) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        [Pure]
        public static UIMeasurement PixelMeasurement(float value) {
            return new UIMeasurement(value, UIMeasurementUnit.Pixel);
        }

        [Pure]
        public static UIMeasurement ContentMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIMeasurementUnit.Content);
        }

        [Pure]
        public static UIMeasurement ParentMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIMeasurementUnit.ParentSize);
        }

        [Pure]
        public static UIMeasurement ViewportWidthMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIMeasurementUnit.ViewportWidth);
        }

        [Pure]
        public static UIMeasurement ViewportHeightMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIMeasurementUnit.ViewportHeight);
        }

        [Pure]
        public static FixedLengthRect Rect(float top, float right, float bottom, float left) {
            return new FixedLengthRect(top, right, bottom, left);
        }

        [Pure]
        public static FixedLengthRect Rect(float topBottom, float leftRight) {
            return new FixedLengthRect(topBottom, leftRight, topBottom, leftRight);
        }

        [Pure]
        public static FixedLengthRect Rect(float value) {
            return new FixedLengthRect(value, value, value, value);
        }

        [Pure]
        public static BorderRadius Radius(float value) {
            return new BorderRadius(value);
        }

        [Pure]
        public static BorderRadius Radius(float top, float bottom) {
            return new BorderRadius(top, bottom);
        }

        [Pure]
        public static BorderRadius Radius(float topLeft, float topRight, float bottomRight, float bottomLeft) {
            return new BorderRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

//        // todo -- support UIMeasurement as arguments here
//        [Pure]
//        public static Dimensions Size(float width, float height) {
//            return new Dimensions(new UIMeasurement(width), new UIMeasurement(height));
//        }

        // todo -- support UIMeasurement as arguments here
        [Pure]
        public static MeasurementPair Vec2Measurement(float x, float y) {
            return new MeasurementPair(new UIMeasurement(x), new UIMeasurement(y));
        }

        [Pure]
        public static FixedLengthVector Vec2FixedLength(float x, float y) {
            return new FixedLengthVector(new UIFixedLength(x), new UIFixedLength(y));
        }

        [Pure]
        public static TMP_FontAsset FontUrl(string url) {
            return UIForia.ResourceManager.GetFont(url);
        }
        
        [Pure]
        public static Texture2D TextureUrl(string url) {
            return UIForia.ResourceManager.GetTexture(url);
        }

        [Pure]
        public static UIFixedLength PercentageLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.Percent);
        }

        [Pure]
        public static UIFixedLength PixelLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.Pixel);
        }

        [Pure]
        public static UIFixedLength EmLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.Em);
        }

        [Pure]
        public static UIFixedLength ViewportWidthLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.ViewportWidth);
        }

        [Pure]
        public static UIFixedLength ViewportHeightLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.ViewportHeight);
        }

        private struct Target {

            public readonly string property;
            public readonly StyleState state;

            public Target(string property, StyleState state) {
                this.property = property;
                this.state = state;
            }

        }

    }

}