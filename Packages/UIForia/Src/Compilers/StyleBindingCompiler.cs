using System;
using JetBrains.Annotations;
using TMPro;
using UIForia.Compilers.AliasSource;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.StyleBindings;
using UnityEngine;

namespace UIForia.Compilers {

    public partial class StyleBindingCompiler {

        private ExpressionCompiler compiler;

        internal static readonly MethodAliasSource sizeAliasSource;
        internal static readonly MethodAliasSource vec2FixedLengthSource;

        internal static readonly MethodAliasSource parentMeasurementSource;
        internal static readonly MethodAliasSource contentMeasurementSource;
        internal static readonly MethodAliasSource viewportWidthMeasurementSource;
        internal static readonly MethodAliasSource viewportHeightMeasurementSource;
        internal static readonly MethodAliasSource pixelMeasurementSource;

        internal static readonly MethodAliasSource textureUrlSource;
        internal static readonly MethodAliasSource fontUrlSource;

        // todo 
        internal static readonly ValueAliasSource<int> siblingIndexSource;
        internal static readonly ValueAliasSource<int> activeSiblingIndexSource;
        internal static readonly ValueAliasSource<int> childCountSource;
        internal static readonly ValueAliasSource<int> activeChildCountSource;
        internal static readonly ValueAliasSource<int> inactiveChildCountSource;
        internal static readonly ValueAliasSource<int> templateChildChildCountSource;

        internal static readonly IAliasSource[] fixedSources;
        internal static readonly IAliasSource[] measurementSources;
        internal static readonly IAliasSource[] colorSources;


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

        private Type rootType;
        private Type elementType;

        static StyleBindingCompiler() {
            Type type = typeof(StyleBindingCompiler);

            textureUrlSource = new MethodAliasSource("url", type.GetMethod(nameof(TextureUrl)));
            fontUrlSource = new MethodAliasSource("url", type.GetMethod(nameof(FontUrl)));

            vec2FixedLengthSource = new MethodAliasSource("vec2", type.GetMethod(nameof(Vec2FixedLength)));
          
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

            colorSources = new IAliasSource[] {
                new ColorAliasSource(),
                new MethodAliasSource("rgb", type.GetMethod("Rgb", ReflectionUtil.PublicStatic)),
                new MethodAliasSource("rgba", type.GetMethod("Rgba", ReflectionUtil.PublicStatic))
            };

            sizeAliasSource = new MethodAliasSource("size", type.GetMethod(nameof(Vec2Measurement)));
        }

        public StyleBindingCompiler() {
            this.compiler = new ExpressionCompiler();
        }

        public void SetCompiler(ExpressionCompiler compiler) {
            this.compiler = compiler;
        }
        
        public StyleBinding Compile(Type rootType, Type elementType, AttributeDefinition attributeDefinition) {
            return Compile(rootType, elementType, attributeDefinition.key, attributeDefinition.value);
        }

        public StyleBinding Compile(Type rootType, Type elementType, string key, string value) {
            if (!key.StartsWith("style.")) return null;
            this.rootType = rootType;
            this.elementType = elementType;

            // todo -- drop this restriction if possible
            if (value[0] != '{') {
                value = '{' + value + '}';
            }

            Target targetState = GetTargetState(key);

            StyleBinding retn = DoCompile(key, value, targetState);
            if (retn != null) {
                return retn;
            }

            switch (targetState.property.ToLower()) {
                case "translation":
                    return new StyleBinding_Translation("Translation", targetState.state, Compile<FixedLengthVector>(value, vec2FixedLengthSource));

                case "preferredsize":
                    return new StyleBinding_PreferredSize("PreferredSize", targetState.state, Compile<MeasurementPair>(value, sizeAliasSource));
            }

            return null;
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

            Expression<T> expression = compiler.Compile<T>(rootType, elementType, value);
            
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
            return ResourceManager.GetFont(url);
        }

        [Pure]
        public static Texture2D TextureUrl(string url) {
            return ResourceManager.GetTexture(url);
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