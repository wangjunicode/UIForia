using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Src {

    // used for methods with 0 arguments, U = context type, T = return type
    public class MethodCallExpression_Instance<U, T> : Expression<T> {

        private readonly Func<U, T> method;
        private readonly bool isConstant;

        public MethodCallExpression_Instance(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Func<U, T>) ReflectionUtil.GetDelegate(typeof(Func<U, T>), methodInfo);
            this.isConstant = ReflectionUtil.HasAnyAttribute(methodInfo,
                typeof(PureAttribute),
                typeof(System.Diagnostics.Contracts.PureAttribute)
            );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method((U) context.rootContext);
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method((U) context.rootContext);
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    // used for methods with 1 arguments, U = context type, V = arg0 type, T = return type
    public class MethodCallExpression_Instance<U, V, T> : Expression<T> {

        private readonly Func<U, V, T> method;
        private readonly Expression<V> argument0;
        private readonly bool isConstant;

        public MethodCallExpression_Instance(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<V>) argumentExpressions[0];
            this.method = (Func<U, V, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, T>), methodInfo);
            this.isConstant = argument0.IsConstant() &&
                              ReflectionUtil.HasAnyAttribute(methodInfo,
                                  typeof(PureAttribute),
                                  typeof(System.Diagnostics.Contracts.PureAttribute)
                              );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    public class MethodCallExpression_Instance<U, V, W, T> : Expression<T> {

        private readonly Func<U, V, W, T> method;
        private readonly Expression<V> argument0;
        private readonly Expression<W> argument1;
        private readonly bool isConstant;

        public MethodCallExpression_Instance(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<V>) argumentExpressions[0];
            this.argument1 = (Expression<W>) argumentExpressions[1];
            this.method = (Func<U, V, W, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, W, T>), methodInfo);
            this.isConstant = argument0.IsConstant() &&
                              argument1.IsConstant() &&
                              ReflectionUtil.HasAnyAttribute(methodInfo,
                                  typeof(PureAttribute),
                                  typeof(System.Diagnostics.Contracts.PureAttribute)
                              );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    public class MethodCallExpression_Instance<U, V, W, X, T> : Expression<T> {

        private readonly Func<U, V, W, X, T> method;
        private readonly Expression<V> argument0;
        private readonly Expression<W> argument1;
        private readonly Expression<X> argument2;
        private readonly bool isConstant;

        public MethodCallExpression_Instance(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<V>) argumentExpressions[0];
            this.argument1 = (Expression<W>) argumentExpressions[1];
            this.argument2 = (Expression<X>) argumentExpressions[2];
            this.method = (Func<U, V, W, X, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, W, X, T>), methodInfo);
            this.isConstant = argument0.IsConstant() &&
                              argument1.IsConstant() &&
                              argument2.IsConstant() &&
                              ReflectionUtil.HasAnyAttribute(methodInfo,
                                  typeof(PureAttribute),
                                  typeof(System.Diagnostics.Contracts.PureAttribute)
                              );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    public class MethodCallExpression_Instance<U, V, W, X, Y, T> : Expression<T> {

        private readonly Func<U, V, W, X, Y, T> method;
        private readonly Expression<V> argument0;
        private readonly Expression<W> argument1;
        private readonly Expression<X> argument2;
        private readonly Expression<Y> argument3;
        private readonly bool isConstant;

        public MethodCallExpression_Instance(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<V>) argumentExpressions[0];
            this.argument1 = (Expression<W>) argumentExpressions[1];
            this.argument2 = (Expression<X>) argumentExpressions[2];
            this.argument3 = (Expression<Y>) argumentExpressions[3];
            this.method =
                (Func<U, V, W, X, Y, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, W, X, Y, T>), methodInfo);
            this.isConstant = argument0.IsConstant() &&
                              argument1.IsConstant() &&
                              argument2.IsConstant() &&
                              argument3.IsConstant() &&
                              ReflectionUtil.HasAnyAttribute(methodInfo,
                                  typeof(PureAttribute),
                                  typeof(System.Diagnostics.Contracts.PureAttribute)
                              );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context),
                argument3.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                (U) context.rootContext,
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context),
                argument3.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

}