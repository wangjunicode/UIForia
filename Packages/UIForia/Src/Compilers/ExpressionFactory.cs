using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UIForia.Compilers {

    internal static class ExpressionFactory {

        private static Func<Expression, Type, MethodInfo, UnaryExpression> s_ConversionFactory;

        public static Expression Convert(Expression expression, Type type, MethodInfo methodInfo) {
            return GetConversionFactory().Invoke(expression, type, methodInfo);
        }

        private static Func<Expression, Type, MethodInfo, UnaryExpression> GetConversionFactory() {
            if (s_ConversionFactory != null) return s_ConversionFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.UnaryExpression");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            var expressionParam = Expression.Parameter(typeof(Expression), "expression");
            var typeParam = Expression.Parameter(typeof(Type), "type");
            var methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");

            var newExpression = Expression.New(info, Expression.Constant(ExpressionType.Convert), expressionParam, typeParam, methodParam);
            BlockExpression block = Expression.Block(newExpression);
            s_ConversionFactory = Expression.Lambda<Func<Expression, Type, MethodInfo, UnaryExpression>>(block, expressionParam, typeParam, methodParam).Compile();
            return s_ConversionFactory;
        }

    }

}