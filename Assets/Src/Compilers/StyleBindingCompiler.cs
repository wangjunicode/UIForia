using System;
using System.Reflection;
using Rendering;
using Src.Layout;
using Src.StyleBindings;
using Src.StyleBindings.Src.StyleBindings;
using UnityEngine;

namespace Src.Compilers {

    public static class StyleTemplateConstants {

        public const string RectX = "rectX";
        public const string RectY = "rectY";
        public const string RectWidth = "rectWidth";
        public const string RectHeight = "rectHeight";

        public const string MinWidth = "minWidth";
        public const string MaxWidth = "maxWidth";
        public const string MinHeight = "minHeight";
        public const string MaxHeight = "maxHeight";

        public const string GrowthFactor = "growthFactor";
        public const string ShrinkFactor = "shrinkFactor";

        public const string BorderColor = "borderColor";
        public const string BackgroundImage = "backgroundImage";
        public const string BackgroundColor = "backgroundColor";

        public const string Padding = "padding";
        public const string PaddingTop = "paddingTop";
        public const string PaddingRight = "paddingRight";
        public const string PaddingBottom = "paddingBottom";
        public const string PaddingLeft = "paddingLeft";

        public const string Border = "border";
        public const string BorderTop = "borderTop";
        public const string BorderRight = "borderRight";
        public const string BorderBottom = "borderBottom";
        public const string BorderLeft = "borderLeft";

        public const string Margin = "margin";
        public const string MarginTop = "marginTop";
        public const string MarginRight = "marginRight";
        public const string MarginBottom = "marginBottom";
        public const string MarginLeft = "marginLeft";

        public const string MainAxisAlignment = "mainAxisAlignment";
        public const string CrossAxisAlignment = "crossAxisAlignment";
        public const string LayoutDirection = "layoutDirection";
        public const string LayoutType = "layoutType";
        public const string LayoutFlow = "layoutFlow";
        public const string LayoutWrap = "layoutWrap";

    }

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
            
            // todo -- dont' compile until we know the type of attribute we are parsing
            // then set contexts to alias constants, get constant returns from url and rgb functions, etc
//            context.SetAliasToConstant("");
            
            compiler = new ExpressionCompiler(context);
            Target targetState = GetTargetState(key);
            ExpressionNode expressionNode = parser.Parse(value);
            Expression expression = compiler.Compile(expressionNode);

            switch (targetState.property) {
                // Paint
                case StyleTemplateConstants.BackgroundImage:
                    return new StyleBinding_BackgroundImage(targetState.state, CastToTexture(expression));

                case StyleTemplateConstants.BackgroundColor:
                    return new StyleBinding_BackgroundColor(targetState.state, CastToColor(expression));

                case StyleTemplateConstants.BorderColor:
                    return new StyleBinding_BorderColor(targetState.state, CastToColor(expression));

                // Rect
                case StyleTemplateConstants.RectX:
                    return new StyleBinding_RectX(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.RectY:
                    return new StyleBinding_RectY(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.RectWidth:
                    return new StyleBinding_RectWidth(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.RectHeight:
                    return new StyleBinding_RectHeight(targetState.state, CastToMeasurement(expression));

                // Constraints
                case StyleTemplateConstants.MinWidth:
                    return new StyleBinding_MinWidth(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.MaxWidth:
                    return new StyleBinding_MaxWidth(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.MinHeight:
                    return new StyleBinding_MinHeight(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.MaxHeight:
                    return new StyleBinding_MaxHeight(targetState.state, CastToMeasurement(expression));

                case StyleTemplateConstants.GrowthFactor:
                    return new StyleBinding_GrowthFactor(targetState.state, (Expression<int>) expression);

                case StyleTemplateConstants.ShrinkFactor:
                    return new StyleBinding_ShrinkFactor(targetState.state, (Expression<int>) expression);

                // Layout

                case StyleTemplateConstants.MainAxisAlignment:
                    return new StyleBinding_MainAxisAlignment(targetState.state, (Expression<MainAxisAlignment>)expression);
                
                case StyleTemplateConstants.CrossAxisAlignment:
                    return new StyleBinding_CrossAxisAlignment(targetState.state, (Expression<CrossAxisAlignment>)expression);

                case StyleTemplateConstants.LayoutDirection:
                    return new StyleBinding_LayoutDirection(targetState.state, (Expression<LayoutDirection>)expression);

                case StyleTemplateConstants.LayoutFlow:
                    return new StyleBinding_LayoutFlowType(targetState.state, (Expression<LayoutFlowType>)expression);

                case StyleTemplateConstants.LayoutType:
                    return new StyleBinding_LayoutType(targetState.state, (Expression<LayoutType>)expression);

                case StyleTemplateConstants.LayoutWrap:
                    return new StyleBinding_LayoutWrap(targetState.state, (Expression<LayoutWrap>)expression);
                    
                // Padding
                
                case StyleTemplateConstants.Padding:
                    return new StyleBinding_Padding(targetState.state, CastToContentRect(expression));

                case StyleTemplateConstants.PaddingTop:
                    return new StyleBinding_PaddingTop(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.PaddingRight:
                    return new StyleBinding_PaddingRight(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.PaddingBottom:
                    return new StyleBinding_PaddingBottom(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.PaddingLeft:
                    return new StyleBinding_PaddingLeft(targetState.state, (Expression<float>) expression);

                // Border

                case StyleTemplateConstants.Border:
                    return new StyleBinding_Border(targetState.state, CastToContentRect(expression));

                case StyleTemplateConstants.BorderTop:
                    return new StyleBinding_BorderTop(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.BorderRight:
                    return new StyleBinding_BorderRight(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.BorderBottom:
                    return new StyleBinding_BorderBottom(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.BorderLeft:
                    return new StyleBinding_PaddingLeft(targetState.state, (Expression<float>) expression);
                
                // Margin
                
                case StyleTemplateConstants.Margin:
                    return new StyleBinding_Margin(targetState.state, CastToContentRect(expression));

                case StyleTemplateConstants.MarginTop:
                    return new StyleBinding_MarginTop(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.MarginRight:
                    return new StyleBinding_MarginRight(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.MarginBottom:
                    return new StyleBinding_MarginBottom(targetState.state, (Expression<float>) expression);

                case StyleTemplateConstants.MarginLeft:
                    return new StyleBinding_MarginLeft(targetState.state, (Expression<float>) expression);
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

                return expression.Cast((exp, ctx) => {
                    Expression<int> expInt = (Expression<int>) exp;
                    return new UIMeasurement(expInt.EvaluateTyped(ctx), UIUnit.Pixel);
                });
            }

            if (expression is Expression<float>) {

                return expression.Cast((exp, ctx) => {
                    Expression<float> expInt = (Expression<float>) exp;
                    return new UIMeasurement(expInt.EvaluateTyped(ctx), UIUnit.Pixel);
                });

            }

            if (expression is Expression<double>) {

                return expression.Cast((exp, ctx) => {
                    Expression<double> expInt = (Expression<double>) exp;
                    return new UIMeasurement(Convert.ToSingle(expInt.EvaluateTyped(ctx)), UIUnit.Pixel);
                });

            }

            return null;
        }

        private static Expression<ContentBoxRect> CastToContentRect(Expression expression) {

            Expression<ContentBoxRect> expression1 = expression as Expression<ContentBoxRect>;
            if (expression1 != null) {
                return expression1;
            }
            return expression.Cast((exp, context) => (ContentBoxRect) exp.Evaluate(context));
        }

        private static Expression<Texture2D> CastToTexture(Expression expression) {

            Expression<Texture2D> expression1 = expression as Expression<Texture2D>;
            if (expression1 != null) {
                return expression1;
            }

            return expression.Cast((exp, context) => (Texture2D) exp.Evaluate(context));

        }

        private static Expression<Color> CastToColor(Expression expression) {
            Expression<Color> expression1 = expression as Expression<Color>;
            if (expression1 != null) return expression1;
            return expression.Cast((exp, context) => (Color) exp.Evaluate(context));
        }

        private static UIMeasurement CastFn(Expression expression, ExpressionContext context) {
            return new UIMeasurement(((Expression<int>) expression).EvaluateTyped(context), UIUnit.Pixel);
        }

        private static Target GetTargetState(string key) {
            if (key.StartsWith("style.hover.")) {
                return new Target(key.Substring("style.hover.".Length), StyleState.Hover);
            }
            else if (key.StartsWith("style.disabled.")) {
                return new Target(key.Substring("style.disabled.".Length), StyleState.Disabled);
            }
            else if (key.StartsWith("style.focused.")) {
                return new Target(key.Substring("style.focused.".Length), StyleState.Focused);
            }
            else if (key.StartsWith("style.active.")) {
                return new Target(key.Substring("style.active.".Length), StyleState.Active);
            }
            else if (key.StartsWith("style.")) {
                return new Target(key.Substring("style.".Length), StyleState.Normal);
            }
            throw new Exception("invalid target state");
        }

        public static Color Rgb(float r, float g, float b) {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Color Rgba(float r, float g, float b, float a) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

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