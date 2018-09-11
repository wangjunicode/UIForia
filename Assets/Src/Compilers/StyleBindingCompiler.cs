using System;
using JetBrains.Annotations;
using Rendering;
using Src.Compilers.AliasSource;
using Src.Layout;
using Src.Rendering;
using Src.StyleBindings;
using Src.StyleBindings.Src.StyleBindings;
using Src.StyleBindings.Text;
using UnityEngine;

namespace Src.Compilers {

    public class StyleBindingCompiler {

        private ContextDefinition context;
        private readonly ExpressionCompiler compiler;

        private static readonly MethodAliasSource rgbSource;
        private static readonly MethodAliasSource rgbaSource;
        private static readonly MethodAliasSource rect1Source;
        private static readonly MethodAliasSource rect2Source;
        private static readonly MethodAliasSource rect4Source;

        private static readonly MethodAliasSource borderRadiusRect1Source;
        private static readonly MethodAliasSource borderRadiusRect2Source;
        private static readonly MethodAliasSource borderRadiusRect4Source;

        private static readonly MethodAliasSource parentMeasurementSource;
        private static readonly MethodAliasSource contentMeasurementSource;
        private static readonly MethodAliasSource viewportMeasurementSource;
        private static readonly MethodAliasSource pixelMeasurementSource;

        private static readonly ColorAliasSource colorSource;
        private static readonly ValueAliasSource<UIMeasurement> autoKeywordSource;
        private static readonly EnumAliasSource<LayoutType> layoutTypeSource;
        private static readonly EnumAliasSource<LayoutDirection> layoutDirectionSource;
        private static readonly EnumAliasSource<LayoutFlowType> layoutFlowSource;
        private static readonly EnumAliasSource<LayoutWrap> layoutWrapSource;
        private static readonly EnumAliasSource<MainAxisAlignment> mainAxisAlignmentSource;
        private static readonly EnumAliasSource<CrossAxisAlignment> crossAxisAlignmentSource;
        private static readonly EnumAliasSource<WhitespaceMode> whiteSpaceSource;

        static StyleBindingCompiler() {
            Type type = typeof(StyleBindingCompiler);
            rgbSource = new MethodAliasSource("rgb", type.GetMethod("Rgb", ReflectionUtil.PublicStatic));
            rgbaSource = new MethodAliasSource("rgba", type.GetMethod("Rgba", ReflectionUtil.PublicStatic));

            rect1Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float)}));
            rect2Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float), typeof(float)}));
            rect4Source = new MethodAliasSource("rect", type.GetMethod(nameof(Rect), new[] {typeof(float), typeof(float), typeof(float), typeof(float)}));

            borderRadiusRect1Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float)}));
            borderRadiusRect2Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float), typeof(float)}));
            borderRadiusRect4Source = new MethodAliasSource("radius", type.GetMethod(nameof(Radius), new[] {typeof(float), typeof(float), typeof(float), typeof(float)}));

            pixelMeasurementSource = new MethodAliasSource("pixels", type.GetMethod(nameof(PixelMeasurement), new[] {typeof(float)}));
            parentMeasurementSource = new MethodAliasSource("parent", type.GetMethod(nameof(ParentMeasurement), new[] {typeof(float)}));
            viewportMeasurementSource = new MethodAliasSource("view", type.GetMethod(nameof(ViewportMeasurement), new[] {typeof(float)}));
            contentMeasurementSource = new MethodAliasSource("content", type.GetMethod(nameof(ContentMeasurement), new[] {typeof(float)}));

            colorSource = new ColorAliasSource();
            layoutTypeSource = new EnumAliasSource<LayoutType>();
            layoutDirectionSource = new EnumAliasSource<LayoutDirection>();
            layoutFlowSource = new EnumAliasSource<LayoutFlowType>();
            layoutWrapSource = new EnumAliasSource<LayoutWrap>();
            mainAxisAlignmentSource = new EnumAliasSource<MainAxisAlignment>();
            crossAxisAlignmentSource = new EnumAliasSource<CrossAxisAlignment>();
            autoKeywordSource = new ValueAliasSource<UIMeasurement>("auto", UIMeasurement.Auto);
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
                    return new StyleBinding_BackgroundImage(targetState.state, Compile<Texture2D>(value));

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
                    return new StyleBinding_BorderRadius_TopLeft(targetState.state, Compile<float>(value));

                case RenderConstants.BorderRadiusTopRight:
                    return new StyleBinding_BorderRadius_TopRight(targetState.state, Compile<float>(value));

                case RenderConstants.BorderRadiusBottomRight:
                    return new StyleBinding_BorderRadius_BottomRight(targetState.state, Compile<float>(value));

                case RenderConstants.BorderRadiusBottomLeft:
                    return new StyleBinding_BorderRadius_BottomLeft(targetState.state, Compile<float>(value));

                // Rect
                case RenderConstants.RectWidth:
                    return new StyleBinding_RectWidth(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                case RenderConstants.RectHeight:
                    return new StyleBinding_RectHeight(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                // Constraints

                case RenderConstants.MinWidth:
                    return new StyleBinding_MinWidth(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                case RenderConstants.MaxWidth:
                    return new StyleBinding_MaxWidth(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                case RenderConstants.MinHeight:
                    return new StyleBinding_MinHeight(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                case RenderConstants.MaxHeight:
                    return new StyleBinding_MaxHeight(targetState.state, Compile<UIMeasurement>(
                        value,
                        autoKeywordSource,
                        pixelMeasurementSource,
                        viewportMeasurementSource,
                        parentMeasurementSource,
                        contentMeasurementSource
                    ));

                case RenderConstants.GrowthFactor:
                    return new StyleBinding_GrowthFactor(targetState.state, Compile<int>(value));

                case RenderConstants.ShrinkFactor:
                    return new StyleBinding_ShrinkFactor(targetState.state, Compile<int>(value));

                // Layout

                case RenderConstants.MainAxisAlignment:
                    return new StyleBinding_MainAxisAlignment(targetState.state, Compile<MainAxisAlignment>(value, mainAxisAlignmentSource));

                case RenderConstants.CrossAxisAlignment:
                    return new StyleBinding_CrossAxisAlignment(targetState.state, Compile<CrossAxisAlignment>(value, crossAxisAlignmentSource));

                case RenderConstants.LayoutDirection:
                    return new StyleBinding_LayoutDirection(targetState.state, Compile<LayoutDirection>(value, layoutDirectionSource));

                case RenderConstants.LayoutFlow:
                    return new StyleBinding_LayoutFlowType(targetState.state, Compile<LayoutFlowType>(value, layoutFlowSource));

                case RenderConstants.LayoutType:
                    return new StyleBinding_LayoutType(targetState.state, Compile<LayoutType>(value, layoutTypeSource));

                case RenderConstants.LayoutWrap:
                    return new StyleBinding_LayoutWrap(targetState.state, Compile<LayoutWrap>(value, layoutWrapSource));

                // Padding

                case RenderConstants.Padding:
                    return new StyleBinding_Padding(targetState.state, Compile<ContentBoxRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.PaddingTop:
                    return new StyleBinding_PaddingTop(targetState.state, Compile<float>(value));

                case RenderConstants.PaddingRight:
                    return new StyleBinding_PaddingRight(targetState.state, Compile<float>(value));

                case RenderConstants.PaddingBottom:
                    return new StyleBinding_PaddingBottom(targetState.state, Compile<float>(value));

                case RenderConstants.PaddingLeft:
                    return new StyleBinding_PaddingLeft(targetState.state, Compile<float>(value));

                // Border

                case RenderConstants.Border:
                    return new StyleBinding_Border(targetState.state, Compile<ContentBoxRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.BorderTop:
                    return new StyleBinding_BorderTop(targetState.state, Compile<float>(value));

                case RenderConstants.BorderRight:
                    return new StyleBinding_BorderRight(targetState.state, Compile<float>(value));

                case RenderConstants.BorderBottom:
                    return new StyleBinding_BorderBottom(targetState.state, Compile<float>(value));

                case RenderConstants.BorderLeft:
                    return new StyleBinding_PaddingLeft(targetState.state, Compile<float>(value));

                // Margin

                case RenderConstants.Margin:
                    return new StyleBinding_Margin(targetState.state, Compile<ContentBoxRect>(value, rect1Source, rect2Source, rect4Source));

                case RenderConstants.MarginTop:
                    return new StyleBinding_MarginTop(targetState.state, Compile<float>(value));

                case RenderConstants.MarginRight:
                    return new StyleBinding_MarginRight(targetState.state, Compile<float>(value));

                case RenderConstants.MarginBottom:
                    return new StyleBinding_MarginBottom(targetState.state, Compile<float>(value));

                case RenderConstants.MarginLeft:
                    return new StyleBinding_MarginLeft(targetState.state, Compile<float>(value));

                // Text

                case RenderConstants.TextColor:
                    return new StyleBinding_TextColor(targetState.state, Compile<Color>(value, colorSource, rgbSource, rgbaSource));

                case RenderConstants.FontSize:
                    return new StyleBinding_FontSize(targetState.state, Compile<int>(value));

                case RenderConstants.Whitespace:
                    return new StyleBinding_Whitespace(targetState.state, Compile<WhitespaceMode>(value, whiteSpaceSource));

                default: return null;
            }
        }

        private static Target GetTargetState(string key) {
            if (key.StartsWith("style.hover.")) {
                return new Target(key.Substring("style.hover.".Length), StyleState.Hover);
            }

            if (key.StartsWith("style.disabled.")) {
                return new Target(key.Substring("style.disabled.".Length), StyleState.Disabled);
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
            return new UIMeasurement(value, UIUnit.Pixel);
        }

        [Pure]
        public static UIMeasurement ContentMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIUnit.Content);
        }

        [Pure]
        public static UIMeasurement ParentMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIUnit.Parent);
        }

        [Pure]
        public static UIMeasurement ViewportMeasurement(float value) {
            return new UIMeasurement(value * 0.01, UIUnit.View);
        }

        [Pure]
        public static ContentBoxRect Rect(float top, float right, float bottom, float left) {
            return new ContentBoxRect(top, right, bottom, left);
        }

        [Pure]
        public static ContentBoxRect Rect(float topBottom, float leftRight) {
            return new ContentBoxRect(topBottom, leftRight, topBottom, leftRight);
        }

        [Pure]
        public static ContentBoxRect Rect(float value) {
            return new ContentBoxRect(value, value, value, value);
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

        [Pure]
        public static AssetPointer Url(string url) {
            return new AssetPointer(url);
        }

        public struct AssetPointer {

            public readonly string path;

            public AssetPointer(string path) {
                this.path = path;
            }

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