using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.Exceptions;
using UIForia.Expressions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers.ExpressionResolvers {

    public class ColorResolver : ExpressionAliasResolver {

        private static readonly MethodInfo s_RGB;

        static ColorResolver() {
            s_RGB = ReflectionUtil.GetMethodInfo(typeof(ColorResolver), nameof(RGB));
        }
        
        public ColorResolver(string aliasName) : base(aliasName) { }

        public override Expression CompileAsMethodExpression(CompilerContext context, List<ASTNode> parameters) {
            if (context.targetType == typeof(Color)) {
                if (parameters.Count == 3) {
                    Expression exprRed = context.Visit(typeof(int), parameters[0]);
                    Expression exprGreen = context.Visit(typeof(int), parameters[1]);
                    Expression exprBlue = context.Visit(typeof(int), parameters[2]);

                    if (exprRed == null || exprGreen == null || exprBlue == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}, all arguments must of type int");
                    }

                    return new MethodCallExpression_Static<int, int, int, int, Color>(s_RGB, new[] {
                        exprRed,
                        exprGreen, 
                        exprBlue, 
                        new ConstantExpression<int>(1)
                    });
                }
                else if (parameters.Count == 4) {
                    Expression exprRed = context.Visit(typeof(int), parameters[0]);
                    Expression exprGreen = context.Visit(typeof(int), parameters[1]);
                    Expression exprBlue = context.Visit(typeof(int), parameters[2]);
                    Expression exprAlpha = context.Visit(typeof(int), parameters[3]);

                    if (exprRed == null || exprGreen == null || exprBlue == null) {
                        throw new CompileException($"Invalid arguments for {aliasName}, all arguments must of type int");
                    }
                    return new MethodCallExpression_Static<int, int, int, int, Color>(s_RGB, new[] {exprRed, exprGreen, exprBlue, exprAlpha});
                }
            }
            else if (context.targetType == typeof(Color32)) { }

            return null;
        }

        [Pure]
        public static Color RGB(int r, int g, int b, int a) {
            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

    }

}