using System.Collections.Generic;
using System.Reflection;
using UIForia.Expressions;
using UIForia.Layout;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.ExpressionResolvers {

    public class ContentSizeResolver : ExpressionAliasResolver {

        public ContentSizeResolver() : base("$content") { }

        public override Expression CompileAsMethodExpression(CompilerContext context, LightList<ASTNode> parameters) {
            if (parameters.Count == 1) {
                if (context.targetType == typeof(UIMeasurement)) {
                    Expression arg = context.Visit(typeof(float), parameters[0]);
                    MethodInfo info = ReflectionUtil.GetMethodInfo(typeof(ContentSizeResolver), nameof(ContentSize));
                    return new MethodCallExpression_Static<float, UIMeasurement>(info, new[] {arg});
                }

                if (context.targetType == typeof(MeasurementPair)) {
                    MethodInfo info = ReflectionUtil.GetMethodInfo(typeof(ContentSizeResolver), nameof(ContentSizePair1));
                    Expression arg0 = context.Visit(typeof(float), parameters[0]);
                    return new MethodCallExpression_Static<float, UIMeasurement>(info, new[] {arg0});
                }
            }
            else if (parameters.Count == 2) {
                if (context.targetType == typeof(MeasurementPair)) {
                    Expression arg0 = context.Visit(typeof(float), parameters[0]);
                    Expression arg1 = context.Visit(typeof(float), parameters[1]);
                    MethodInfo info = ReflectionUtil.GetMethodInfo(typeof(ContentSizeResolver), nameof(ContentSizePair2));
                    return new MethodCallExpression_Static<float, UIMeasurement>(info, new[] {arg0, arg1});
                }
            }

            return null;
        }

        private static UIMeasurement ContentSize(float size) {
            return new UIMeasurement(size * 0.01f, UIMeasurementUnit.Content);
        }

        private static MeasurementPair ContentSizePair1(float size) {
            return new MeasurementPair(
                new UIMeasurement(size * 0.01f, UIMeasurementUnit.Content),
                new UIMeasurement(size * 0.01f, UIMeasurementUnit.Content)
            );
        }

        private static MeasurementPair ContentSizePair2(float x, float y) {
            return new MeasurementPair(
                new UIMeasurement(x * 0.01f, UIMeasurementUnit.Content),
                new UIMeasurement(y * 0.01f, UIMeasurementUnit.Content)
            );
        }

    }

}