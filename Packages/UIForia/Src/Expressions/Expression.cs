using System;
using System.Collections;
using System.Reflection;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Expressions {

    public abstract class WriteTargetExpression { }

    public abstract class WriteTargetExpression<T> : WriteTargetExpression {

        public abstract void Assign(ExpressionContext ctx, T value);

    }

    public class FieldPropertyTargetExpression<T, U> : WriteTargetExpression<U> {

        private readonly Func<T, U, U> setter;

        public FieldPropertyTargetExpression(FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            if (fieldInfo != null) {
                setter = (Func<T, U, U>) ReflectionUtil.GetLinqFieldAccessors(typeof(T), fieldInfo).setter;
            }
            else if (propertyInfo != null) {
                setter = (Func<T, U, U>) ReflectionUtil.GetLinqPropertyAccessors(typeof(T), propertyInfo).setter;
            }
        }

        public override void Assign(ExpressionContext context, U value) {
            setter.Invoke((T) context.rootObject, value);
        }

    }

    public class MemberWriteTargetExpression<T, U> : WriteTargetExpression<U> where T : class {

        private readonly Func<T, U, U> setter;
        public readonly Expression<T> getTargetBaseExpression;

        public MemberWriteTargetExpression(Expression<T> getTargetBaseExpression, FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            this.getTargetBaseExpression = getTargetBaseExpression;
            if (fieldInfo != null) {
                setter = (Func<T, U, U>) ReflectionUtil.GetLinqFieldAccessors(typeof(T), fieldInfo).setter;
            }
            else if (propertyInfo != null) {
                setter = (Func<T, U, U>) ReflectionUtil.GetLinqPropertyAccessors(typeof(T), propertyInfo).setter;
            }
        }

        public override void Assign(ExpressionContext ctx, U value) {
            T target = getTargetBaseExpression.Evaluate(ctx);

            if (target == null) {
                return;
            }
            
            setter(getTargetBaseExpression.Evaluate(ctx), value);
        }

    }

    public class IndexedWriteTargetExpression<T, V, W> : WriteTargetExpression<T> where V : class {

        public readonly Expression<W> indexExpression;
        public readonly Expression<V> getTargetBaseExpression;

        public readonly Func<V, W, T, T> setter;

        public IndexedWriteTargetExpression(Expression<V> getTargetBaseExpression, Expression<W> indexExpression) {
            this.getTargetBaseExpression = getTargetBaseExpression;
            this.indexExpression = indexExpression;
            if (typeof(V).IsArray) {
                setter = (Func<V, W, T, T>) ReflectionUtil.CreateArraySetter(typeof(V));
            }
            else {
                setter = (Func<V, W, T, T>) ReflectionUtil.CreateIndexSetter(typeof(V));
            }
        }

        public override void Assign(ExpressionContext ctx, T value) {
            V target = getTargetBaseExpression.Evaluate(ctx);
            // should not actually box since we only allow class types
            if (target == null) {
                return;
            }
            W index = indexExpression.Evaluate(ctx);
            // todo -- not sure what happens for out of bounds access
            setter.Invoke(target, index, value);
        }

    }

    public abstract class Expression {

        public abstract Type YieldedType { get; }

        public abstract bool IsConstant();

    }

    public abstract class Expression<T> : Expression {

        public abstract T Evaluate(ExpressionContext context);

    }

}