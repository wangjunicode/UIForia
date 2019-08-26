using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.Exceptions;
using UIForia.Expressions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;

namespace UIForia.Compilers.ExpressionResolvers {

    public class LengthResolver : ExpressionAliasResolver {

        private static readonly MethodInfo s_LengthVector1;
        private static readonly MethodInfo s_LengthVector2;

        static LengthResolver() {
            s_LengthVector1 = ReflectionUtil.GetMethodInfo(typeof(LengthResolver), nameof(FixedVec1));
            s_LengthVector2 = ReflectionUtil.GetMethodInfo(typeof(LengthResolver), nameof(FixedVec2));
        }

        public LengthResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, LightList<ASTNode> parameters) {
            if (context.targetType == typeof(UIFixedLengthPair)) {
                if (parameters.Count == 1) {
                    Expression expr = context.Visit(typeof(UIFixedLength), parameters[0]);
                    if (expr == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}");
                    }

                    return new MethodCallExpression_Static<UIFixedLength, UIFixedLengthPair>(s_LengthVector1, new[] {expr});
                }

                if (parameters.Count == 2) {
                    Expression expr0 = context.Visit(typeof(UIFixedLength), parameters[0]);
                    Expression expr1 = context.Visit(typeof(UIFixedLength), parameters[1]);
                    if (expr0 == null || expr1 == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}");
                    }

                    return new MethodCallExpression_Static<UIFixedLength, UIFixedLength, UIFixedLengthPair>(s_LengthVector2, new[] {expr0, expr1});
                }
            }

            if (context.targetType == typeof(UIFixedLength)) { }

            return null;
        }

        [Pure]
        public static UIFixedLengthPair FixedVec1(UIFixedLength x) {
            return new UIFixedLengthPair(x, x);
        }

        [Pure]
        public static UIFixedLengthPair FixedVec2(UIFixedLength x, UIFixedLength y) {
            return new UIFixedLengthPair(x, y);
        }

    }

}