using System;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.Util;

namespace UIForia.Expressions {

    public class MethodCallExpression_InstanceVoid<T> : Expression<Terminal> {

        public override Type YieldedType => typeof(void);
        private readonly Action<T> method;

        public MethodCallExpression_InstanceVoid(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Action<T>) ReflectionUtil.GetDelegate(typeof(Action<T>), methodInfo);
        }

        public override Terminal Evaluate(ExpressionContext context) {
            method((T) context.aux);
            return null;
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class MethodCallExpression_InstanceVoid<T, U> : Expression<Terminal> {

        public override Type YieldedType => typeof(void);
        private readonly Action<T, U> method;
        private readonly Expression<U> argument0;

        public MethodCallExpression_InstanceVoid(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Action<T, U>) ReflectionUtil.GetDelegate(typeof(Action<T, U>), methodInfo);
            this.argument0 = (Expression<U>) argumentExpressions[0];
        }

        public override Terminal Evaluate(ExpressionContext context) {
            method((T) context.aux, argument0.Evaluate(context));
            return null;
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class MethodCallExpression_InstanceVoid<T, U, V> : Expression<Terminal> {

        public override Type YieldedType => typeof(void);
        private readonly Action<T, U, V> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;

        public MethodCallExpression_InstanceVoid(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Action<T, U, V>) ReflectionUtil.GetDelegate(typeof(Action<T, U, V>), methodInfo);
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
        }

        public override Terminal Evaluate(ExpressionContext context) {
            method((T) context.aux, argument0.Evaluate(context), argument1.Evaluate(context));
            return null;
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class MethodCallExpression_InstanceVoid<T, U, V, W> : Expression<Terminal> {

        public override Type YieldedType => typeof(void);
        private readonly Action<T, U, V, W> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;
        private readonly Expression<W> argument2;

        public MethodCallExpression_InstanceVoid(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Action<T, U, V, W>) ReflectionUtil.GetDelegate(typeof(Action<T, U, V, W>), methodInfo);
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
            this.argument2 = (Expression<W>) argumentExpressions[2];
        }

        public override Terminal Evaluate(ExpressionContext context) {
            method((T) context.aux,
                argument0.Evaluate(context),
                argument1.Evaluate(context),
                argument2.Evaluate(context)
            );
            return null;
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class MethodCallExpression_InstanceVoid<T, U, V, W, X> : Expression<Terminal> {

        public override Type YieldedType => typeof(void);
        private readonly Action<T, U, V, W, X> method;
        private readonly Expression<U> argument0;
        private readonly Expression<V> argument1;
        private readonly Expression<W> argument2;
        private readonly Expression<X> argument3;

        public MethodCallExpression_InstanceVoid(MethodInfo methodInfo, Expression[] argumentExpressions) {
            this.method = (Action<T, U, V, W, X>) ReflectionUtil.GetDelegate(typeof(Action<T, U, V, W, X>), methodInfo);
            this.argument0 = (Expression<U>) argumentExpressions[0];
            this.argument1 = (Expression<V>) argumentExpressions[1];
            this.argument2 = (Expression<W>) argumentExpressions[2];
            this.argument3 = (Expression<X>) argumentExpressions[3];
        }

        public override Terminal Evaluate(ExpressionContext context) {
            method((T) context.aux,
                argument0.Evaluate(context),
                argument1.Evaluate(context),
                argument2.Evaluate(context),
                argument3.Evaluate(context)
            );
            return null;
        }

        public override bool IsConstant() {
            return false;
        }

    }

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

        public override T Evaluate(ExpressionContext context) {
            return method((U) context.aux);
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

        public override T Evaluate(ExpressionContext context) {
            return method(
                (U) context.aux,
                argument0.Evaluate(context)
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

        public override T Evaluate(ExpressionContext context) {
            return method(
                (U) context.aux,
                argument0.Evaluate(context),
                argument1.Evaluate(context)
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

        public override T Evaluate(ExpressionContext context) {
            return method(
                (U) context.aux,
                argument0.Evaluate(context),
                argument1.Evaluate(context),
                argument2.Evaluate(context)
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

        public override T Evaluate(ExpressionContext context) {
            return method(
                (U) context.aux,
                argument0.Evaluate(context),
                argument1.Evaluate(context),
                argument2.Evaluate(context),
                argument3.Evaluate(context)
            );
        }

        public override bool IsConstant() {
            return isConstant;
        }

    }

}