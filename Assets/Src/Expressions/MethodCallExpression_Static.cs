using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Src {

    public class MethodCallExpression_Static<T> : Expression<T> {

        private readonly Func<T> method;
        private readonly bool isConstant;

        public MethodCallExpression_Static(MethodInfo methodInfo, Expression[] argumentExpressions) {
            method = (Func<T>) ReflectionUtil.GetDelegate(typeof(Func<T>), methodInfo);
            isConstant = ReflectionUtil.HasAnyAttribute(methodInfo,
                typeof(PureAttribute),
                typeof(System.Diagnostics.Contracts.PureAttribute)
            );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method();
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method();
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    // used for methods with 0 arguments, U = context type, T = return type
    public class MethodCallExpression_Static<U, T> : Expression<T> {

        private readonly Func<U, T> method;
        private readonly bool isConstant;
        private readonly Expression<U> argument0;

        public MethodCallExpression_Static(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Func<U, T>) ReflectionUtil.GetDelegate(typeof(Func<U, T>), methodInfo);
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.isConstant = ReflectionUtil.HasAnyAttribute(methodInfo,
                typeof(PureAttribute),
                typeof(System.Diagnostics.Contracts.PureAttribute)
            );
        }

        public override Type YieldedType => typeof(T);

        public override object Evaluate(ExpressionContext context) {
            return method(argument0.EvaluateTyped(context));
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(argument0.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    // used for methods with 1 arguments, U = context type, V = arg0 type, T = return type
    public class MethodCallExpression_Static<U, V, T> : Expression<T> {

        private readonly Func<U, V, T> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;
        private readonly bool isConstant;

        public MethodCallExpression_Static(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
            this.method = (Func<U, V, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, T>), methodInfo);
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
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    public class MethodCallExpression_Static<U, V, W, T> : Expression<T> {

        private readonly Func<U, V, W, T> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;
        private readonly Expression<W> argument2;
        private readonly bool isConstant;

        public MethodCallExpression_Static(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
            this.argument2 = (Expression<W>) argumentExpressions[2];
            this.method = (Func<U, V, W, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, W, T>), methodInfo);
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
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

    public class MethodCallExpression_Static<U, V, W, X, T> : Expression<T> {

        private readonly Func<U, V, W, X, T> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;
        private readonly Expression<W> argument2;
        private readonly Expression<X> argument3;
        private readonly bool isConstant;

        public MethodCallExpression_Static(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
            this.argument2 = (Expression<W>) argumentExpressions[2];
            this.argument3 = (Expression<X>) argumentExpressions[3];
            this.method = (Func<U, V, W, X, T>) ReflectionUtil.GetDelegate(typeof(Func<U, V, W, X, T>), methodInfo);
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
                argument0.EvaluateTyped(context),
                argument1.EvaluateTyped(context),
                argument2.EvaluateTyped(context),
                argument3.EvaluateTyped(context)
            );
        }

        public override T EvaluateTyped(ExpressionContext context) {
            return method(
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