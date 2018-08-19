using System;
using System.Reflection;
using Rendering;
using Src.StyleBindings;
using UnityEngine;

namespace Src.Compilers {

    public class StyleBindingCompiler {

        private ContextDefinition context;
        private ExpressionCompiler compiler;
        private ExpressionParser parser;

        public StyleBindingCompiler() {
            parser = new ExpressionParser();
        }

        private static MethodInfo rgbInfo;
        private static MethodInfo rgbaInfo;

        static StyleBindingCompiler() {
            rgbInfo = typeof(StyleBindingCompiler).GetMethod("Rgb", BindingFlags.Public | BindingFlags.Static);
            if (rgbInfo == null) {
                Debug.Log("rgb null");
            }
            rgbaInfo = typeof(StyleBindingCompiler).GetMethod("Rgba", BindingFlags.Public | BindingFlags.Static);
            if (rgbaInfo == null) {
                Debug.Log("rgba null");
            }
        }

        public StyleBinding Compile(ContextDefinition context, string key, string value) {

            context.SetMethodAlias("rgb", rgbInfo);
            context.SetMethodAlias("rgba", rgbaInfo);

            compiler = new ExpressionCompiler(context);
            Target targetState = GetTargetState(key);
            ExpressionNode expressionNode = parser.Parse(value);
            Expression expression = compiler.Compile(expressionNode);

            switch (targetState.property) {
                case "backgroundColor":
                    return new StyleBinding_BackgroundColor(targetState.state, CastToColor(expression));

                case "rectW":
                    return new StyleBinding_RectW(targetState.state, CastToMeasurement(expression));

                case "rectH":
                    return new StyleBinding_RectH(targetState.state, CastToMeasurement(expression));
            }

            context.RemoveMethodAlias("rgb");
            context.RemoveMethodAlias("rgba");

            return null;
        }

        private static Expression<UIMeasurement> CastToMeasurement(Expression expression) {

            Expression<UIMeasurement> expression1 = expression as Expression<UIMeasurement>;
            if (expression1 != null) {
                return expression1;
            }

            if (expression is Expression<int>) {
                return expression.Cast(CastFn);
            }

            return null;
        }

        private static Expression<Color> CastToColor(Expression expression) {
            return expression.Cast(ColorCastFn);
        }

        private static Color ColorCastFn(Expression expression, ExpressionContext context) {
            return (Color) expression.Evaluate(context);
        }

        private static UIMeasurement CastFn(Expression expression, ExpressionContext context) {
            return new UIMeasurement(((Expression<int>) expression).EvaluateTyped(context), UIUnit.Pixel);
        }

        private static Target GetTargetState(string key) {
            if (key.StartsWith("style.hover.")) {
                return new Target(key.Substring("style.hover.".Length), StyleStateType.Hover);
            }
            else if (key.StartsWith("style.disabled.")) {
                return new Target(key.Substring("style.disabled.".Length), StyleStateType.Disabled);
            }
            else if (key.StartsWith("style.focused.")) {
                return new Target(key.Substring("style.focused.".Length), StyleStateType.Focused);
            }
            else if (key.StartsWith("style.active.")) {
                return new Target(key.Substring("style.active.".Length), StyleStateType.Active);
            }
            else if (key.StartsWith("style.")) {
                return new Target(key.Substring("style.".Length), StyleStateType.Normal);
            }
            throw new Exception("invalid target state");
        }

        public static Color Rgb(float r, float g, float b) {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Color Rgba(float r, float g, float b, float a) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private struct Target {

            public readonly string property;
            public readonly StyleStateType state;

            public Target(string property, StyleStateType state) {
                this.property = property;
                this.state = state;
            }

        }

    }

}