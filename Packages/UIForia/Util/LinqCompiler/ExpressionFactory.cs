using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace UIForia.Compilers {

    public static class ExpressionFactory {

        private static Func<Expression, Type, MethodInfo, UnaryExpression> s_ConversionFactory;
        private static Func<Expression, Expression, BinaryExpression> s_AssignmentFactory;
        private static Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression> s_InstanceCallFactory;
        private static Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression> s_StaticCallFactory;

        public static Expression Convert(Expression expression, Type type, MethodInfo methodInfo = null) {
            return GetConversionFactory().Invoke(expression, type, methodInfo);
        }

        public static Expression AssignUnchecked(Expression target, Expression src) {
            return GetAssignmentFactory().Invoke(target, src);
        }

        public static NewExpression New(ConstructorInfo constructorInfo) {
            return Expression.New(constructorInfo);
        }

        [ThreadStatic] private static Expression[] s_Array1;
        [ThreadStatic] private static Expression[] s_Array2;
        [ThreadStatic] private static Expression[] s_Array3;
        [ThreadStatic] private static Expression[] s_Array4;

        public static NewExpression New(ConstructorInfo constructorInfo, Expression p0) {
            s_Array1 = s_Array1 ?? new Expression[1];
            s_Array1[0] = p0;
            NewExpression retn = Expression.New(constructorInfo, s_Array1);
            s_Array1[0] = null;
            return retn;
        }

        public static NewExpression New(ConstructorInfo constructorInfo, Expression p0, Expression p1) {
            s_Array2 = s_Array2 ?? new Expression[2];
            s_Array2[0] = p0;
            s_Array2[1] = p1;
            NewExpression retn = Expression.New(constructorInfo, s_Array2);
            s_Array2[0] = null;
            s_Array2[1] = null;
            return retn;
        }

        public static NewExpression New(ConstructorInfo constructorInfo, Expression p0, Expression p1, Expression p2) {
            s_Array3 = s_Array3 ?? new Expression[3];
            s_Array3[0] = p0;
            s_Array3[1] = p1;
            s_Array3[2] = p2;
            NewExpression retn = Expression.New(constructorInfo, s_Array3);
            s_Array3[0] = null;
            s_Array3[1] = null;
            s_Array3[2] = null;
            return retn;
        }
        
        public static NewExpression New(ConstructorInfo constructorInfo, Expression p0, Expression p1, Expression p2, Expression p3) {
            s_Array4 = s_Array4 ?? new Expression[4];
            s_Array4[0] = p0;
            s_Array4[1] = p1;
            s_Array4[2] = p2;
            s_Array4[3] = p3;
            NewExpression retn = Expression.New(constructorInfo, s_Array4);
            s_Array4[0] = null;
            s_Array4[1] = null;
            s_Array4[2] = null;
            s_Array4[3] = null;
            return retn;
        }

        // todo -- use the non list allocating versions
        public static MethodCallExpression CallInstanceUnchecked(Expression target, MethodInfo method, params Expression[] arguments) {
            if (method == null) throw new NullReferenceException();
            return GetInstanceCall0Factory().Invoke(method, target, new ReadOnlyCollection<Expression>(arguments));
        }

        // todo -- better support for 
        public static Expression CallStaticUnchecked(MethodInfo method, params Expression[] arguments) {
            if (method == null) throw new NullReferenceException();
            return GetStaticCallFactory().Invoke(method, new ReadOnlyCollection<Expression>(arguments));
        }

        // todo -- reduce allocations
        public static Expression CallStaticUnchecked(MethodInfo method, Expression arg0) {
            if (method == null) throw new NullReferenceException();
            return GetStaticCallFactory().Invoke(method, new ReadOnlyCollection<Expression>(new[] {arg0}));
        }

        public static Expression CallStaticUnchecked(MethodInfo method, Expression arg0, Expression arg1) {
            if (method == null) throw new NullReferenceException();
            return GetStaticCallFactory().Invoke(method, new ReadOnlyCollection<Expression>(new[] {arg0, arg1}));
        }

        public static Expression CallStaticUnchecked(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) {
            if (method == null) throw new NullReferenceException();
            return GetStaticCallFactory().Invoke(method, new ReadOnlyCollection<Expression>(new[] {arg0, arg1, arg2}));
        }

        private static Func<Expression, Type, MethodInfo, UnaryExpression> GetConversionFactory() {
            if (s_ConversionFactory != null) return s_ConversionFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.UnaryExpression");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            ParameterExpression expressionParam = Expression.Parameter(typeof(Expression), "expression");
            ParameterExpression typeParam = Expression.Parameter(typeof(Type), "type");
            ParameterExpression methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");

            NewExpression newExpression = Expression.New(info, Expression.Constant(ExpressionType.Convert), expressionParam, typeParam, methodParam);
            BlockExpression block = Expression.Block(newExpression);
            s_ConversionFactory = Expression.Lambda<Func<Expression, Type, MethodInfo, UnaryExpression>>(block, expressionParam, typeParam, methodParam).Compile();
            return s_ConversionFactory;
        }

        private static Func<Expression, Expression, BinaryExpression> GetAssignmentFactory() {
            if (s_AssignmentFactory != null) return s_AssignmentFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            ParameterExpression dstParam = Expression.Parameter(typeof(Expression), "dst");
            ParameterExpression srcParam = Expression.Parameter(typeof(Expression), "src");
            NewExpression newExpression = Expression.New(info, dstParam, srcParam);
            BlockExpression block = Expression.Block(newExpression);
            s_AssignmentFactory = Expression.Lambda<Func<Expression, Expression, BinaryExpression>>(block, dstParam, srcParam).Compile();
            return s_AssignmentFactory;
        }

        private static Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression> GetInstanceCall0Factory() {
            if (s_InstanceCallFactory != null) return s_InstanceCallFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.InstanceMethodCallExpressionN");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            ParameterExpression targetParam = Expression.Parameter(typeof(Expression), "expression");
            ParameterExpression methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");
            ParameterExpression args = Expression.Parameter(typeof(IReadOnlyList<Expression>), "args");
            NewExpression newExpression = Expression.New(info, methodParam, targetParam, args);
            BlockExpression block = Expression.Block(newExpression);
            s_InstanceCallFactory = Expression.Lambda<Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression>>(block, methodParam, targetParam, args).Compile();
            return s_InstanceCallFactory;
        }

        private static Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression> GetStaticCallFactory() {
            if (s_StaticCallFactory != null) return s_StaticCallFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.MethodCallExpressionN");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            ParameterExpression methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");
            ParameterExpression args = Expression.Parameter(typeof(IReadOnlyList<Expression>), "args");
            NewExpression newExpression = Expression.New(info, methodParam, args);
            BlockExpression block = Expression.Block(newExpression);
            s_StaticCallFactory = Expression.Lambda<Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression>>(block, methodParam, args).Compile();
            return s_StaticCallFactory;
        }

    }

}