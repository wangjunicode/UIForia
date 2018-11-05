using System;
using System.Collections;
using System.Reflection;

namespace Src {

    // todo more generic version for part types?
    public class AccessExpression<T, U> : Expression<T> {

        protected readonly string contextName;
        protected readonly AccessExpressionPart[] parts;

        public AccessExpression(string contextName, AccessExpressionPart[] parts) {
            this.contextName = contextName;
            this.parts = parts;
        }

        public override Type YieldedType => typeof(T);

        public override bool IsConstant() {
            return false;
        }

        public override object Evaluate(ExpressionContext context) {
            U contextHead;
            if (contextName[0] == '$') {
                context.GetContextValue(context.current, contextName, out contextHead);
            }
            else {
                contextHead = (U) context.rootContext;
            }

            object last = contextHead;
            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return null;
                }
            }

            return last;
        }

        public override T EvaluateTyped(ExpressionContext context) {
            U contextHead;
            if (contextName[0] == '$') {
                context.GetContextValue(context.current, contextName, out contextHead);
            }
            else if (contextName[0] == '@') {
                contextHead = (U) context.rootContext;
            }
            else {
                contextHead = (U) context.rootContext;
            }

            object last = contextHead;

            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return default(T);
                }
            }

            return (T) last;
        }

    }

    public class AccessExpressionStaticProperty<T> : Expression<T> {

        protected readonly PropertyInfo propertyInfo;
        protected readonly AccessExpressionPart[] parts;

        public AccessExpressionStaticProperty(PropertyInfo propertyInfo, AccessExpressionPart[] parts) {
            this.propertyInfo = propertyInfo;
            this.parts = parts;
        }

        public override Type YieldedType => typeof(T);

        public override bool IsConstant() {
            return false;
        }

        public override object Evaluate(ExpressionContext context) {
            object last = propertyInfo.GetValue(null);
            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return null;
                }
            }

            return last;
        }

        public override T EvaluateTyped(ExpressionContext context) {
            object last = propertyInfo.GetValue(null);
            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return default(T);
                }
            }

            return (T) last;
        }

    }

    public class AccessExpressionStaticField<T> : Expression<T> {

        protected readonly FieldInfo fieldInfo;
        protected readonly AccessExpressionPart[] parts;

        public AccessExpressionStaticField(FieldInfo fieldInfo, AccessExpressionPart[] parts) {
            this.fieldInfo = fieldInfo;
            this.parts = parts;
        }

        public override Type YieldedType => typeof(T);

        public override bool IsConstant() {
            return false;
        }

        public override object Evaluate(ExpressionContext context) {
            object last = fieldInfo.GetValue(null);
            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return null;
                }
            }

            return last;
        }

        public override T EvaluateTyped(ExpressionContext context) {
            object last = fieldInfo.GetValue(null);
            for (int i = 0; i < parts.Length; i++) {
                last = parts[i].Evaluate(last, context);
                if (last == null) {
                    return default(T);
                }
            }

            return (T) last;
        }

    }

    public abstract class AccessExpressionPart {

        public abstract object Evaluate(object target, ExpressionContext context);

    }

    public class AccessExpressionPart_List : AccessExpressionPart {

        public readonly Expression<int> indexExpression;

        public AccessExpressionPart_List(Expression<int> indexExpression) {
            this.indexExpression = indexExpression;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            IList targetList = (IList) target;
            int index = indexExpression.EvaluateTyped(context);
            return targetList[index];
        }

    }

    public abstract class AccessExpressionPart_Func : AccessExpressionPart {

        public virtual Type RetnType => null;

    }

    public class AccessExpressionPart_Func<T> : AccessExpressionPart_Func {

        public override object Evaluate(object target, ExpressionContext context) {
            Func<T> fn = (Func<T>) target;
            return fn == null ? null : (object) fn.Invoke();
        }
        
        public override Type RetnType => typeof(T);

    }

    public class AccessExpressionPart_Func<T, U> : AccessExpressionPart_Func {

        private readonly Expression<U> arg0;

        public AccessExpressionPart_Func(Expression<U> arg0) {
            this.arg0 = arg0;
        }

        public override Type RetnType => typeof(T);

        public override object Evaluate(object target, ExpressionContext context) {
            Func<U, T> fn = (Func<U, T>) target;
            return fn == null ? null : (object) fn.Invoke(arg0.EvaluateTyped(context));
        }

    }
    
    public class AccessExpressionPart_Func<T, U, V> : AccessExpressionPart_Func {

        private readonly Expression<U> arg0;
        private readonly Expression<V> arg1;

        public AccessExpressionPart_Func(Expression<U> arg0, Expression<V> arg1) {
            this.arg0 = arg0;
            this.arg1 = arg1;
        }

        public override Type RetnType => typeof(T);

        public override object Evaluate(object target, ExpressionContext context) {
            Func<U, V, T> fn = (Func<U, V, T>) target;
            return fn == null ? null : (object) fn.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context)
            );
        }

    }

    public class AccessExpressionPart_Func<T, U, V, W> : AccessExpressionPart_Func {

        private readonly Expression<U> arg0;
        private readonly Expression<V> arg1;
        private readonly Expression<W> arg2;

        public AccessExpressionPart_Func(Expression<U> arg0, Expression<V> arg1, Expression<W> arg2) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public override Type RetnType => typeof(T);

        public override object Evaluate(object target, ExpressionContext context) {
            Func<U, V, W, T> fn = (Func<U, V, W, T>) target;
            return fn == null ? null : (object) fn.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context),
                arg2.EvaluateTyped(context)
            );
        }

    }
    
    public class AccessExpressionPart_Func<T, U, V, W, X> : AccessExpressionPart_Func {

        private readonly Expression<U> arg0;
        private readonly Expression<V> arg1;
        private readonly Expression<W> arg2;
        private readonly Expression<X> arg3;

        public AccessExpressionPart_Func(Expression<U> arg0, Expression<V> arg1, Expression<W> arg2, Expression<X> arg3) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
        }
        
        public override Type RetnType => typeof(T);

        public override object Evaluate(object target, ExpressionContext context) {
            Func<U, V, W, X, T> fn = (Func<U, V, W, X, T>) target;
            return fn == null ? null : (object) fn.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context),
                arg2.EvaluateTyped(context),
                arg3.EvaluateTyped(context)
            );
        }

    }
    
    public class AccessExpressionPart_Method : AccessExpressionPart {

        public override object Evaluate(object target, ExpressionContext context) {
            Action action = (Action) target;
            action?.Invoke();
            return null;
        }

    }

    public class AccessExpressionPart_Method<T> : AccessExpressionPart_Method {

        private readonly Expression<T> arg0;

        public AccessExpressionPart_Method(Expression<T> arg0) {
            this.arg0 = arg0;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            Action<T> action = (Action<T>) target;
            action?.Invoke(arg0.EvaluateTyped(context));
            return null;
        }

    }

    public class AccessExpressionPart_Method<T, U> : AccessExpressionPart_Method {

        private readonly Expression<T> arg0;
        private readonly Expression<U> arg1;

        public AccessExpressionPart_Method(Expression<T> arg0, Expression<U> arg1) {
            this.arg0 = arg0;
            this.arg1 = arg1;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            Action<T, U> action = (Action<T, U>) target;
            action?.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context)
            );
            return null;
        }

    }

    public class AccessExpressionPart_Method<T, U, V> : AccessExpressionPart_Method {

        private readonly Expression<T> arg0;
        private readonly Expression<U> arg1;
        private readonly Expression<V> arg2;

        public AccessExpressionPart_Method(Expression<T> arg0, Expression<U> arg1, Expression<V> arg2) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            Action<T, U, V> action = (Action<T, U, V>) target;
            action?.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context),
                arg2.EvaluateTyped(context)
            );
            return null;
        }

    }

    public class AccessExpressionPart_Method<T, U, V, W> : AccessExpressionPart_Method {

        private readonly Expression<T> arg0;
        private readonly Expression<U> arg1;
        private readonly Expression<V> arg2;
        private readonly Expression<W> arg3;

        public AccessExpressionPart_Method(Expression<T> arg0, Expression<U> arg1, Expression<V> arg2, Expression<W> arg3) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            Action<T, U, V, W> action = (Action<T, U, V, W>) target;
            action?.Invoke(
                arg0.EvaluateTyped(context),
                arg1.EvaluateTyped(context),
                arg2.EvaluateTyped(context),
                arg3.EvaluateTyped(context)
            );
            return null;
        }

    }

    public class AccessExpressionPart_Field : AccessExpressionPart {

        private Type cachedType;
        private FieldInfo cachedFieldInfo;
        public readonly string fieldName;

        public AccessExpressionPart_Field(string fieldName) {
            this.fieldName = fieldName;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            if (target == null) return null;
            Type targetType = target.GetType();
            if (targetType != cachedType || cachedFieldInfo == null) {
                cachedType = targetType;
                cachedFieldInfo = targetType.GetField(fieldName, ReflectionUtil.InstanceBindFlags);
                if (cachedFieldInfo == null) {
                    throw new Exception($"Field {fieldName} does not exist on type {targetType}");
                }
            }

            return cachedFieldInfo.GetValue(target);
        }

    }

    public class AccessExpressionPart_Property : AccessExpressionPart {

        private Type cachedType;
        private PropertyInfo cachedPropertyInfo;
        public readonly string propertyName;

        public AccessExpressionPart_Property(string propertyName) {
            this.propertyName = propertyName;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            if (target == null) return null;
            Type targetType = target.GetType();
            if (targetType != cachedType || cachedPropertyInfo == null) {
                cachedType = targetType;
                cachedPropertyInfo = targetType.GetProperty(propertyName, ReflectionUtil.InstanceBindFlags);
                if (cachedPropertyInfo == null) {
                    throw new Exception($"Property {propertyName} does not exist on type {targetType}");
                }
            }

            return cachedPropertyInfo.GetValue(target);
        }

    }

    public class AccessExpressionPart_StaticProperty : AccessExpressionPart {

        private readonly PropertyInfo cachedPropertyInfo;

        public AccessExpressionPart_StaticProperty(PropertyInfo cachedPropertyInfo) {
            this.cachedPropertyInfo = cachedPropertyInfo;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            return cachedPropertyInfo.GetValue(null);
        }

    }

    public class AccessExpressionPart_StaticField : AccessExpressionPart {

        private readonly FieldInfo cachedFieldInfo;

        public AccessExpressionPart_StaticField(FieldInfo cachedFieldInfo) {
            this.cachedFieldInfo = cachedFieldInfo;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            return cachedFieldInfo.GetValue(null);
        }

    }

    public class AccessExpressionPart_Constant : AccessExpressionPart {

        private readonly object value;

        public AccessExpressionPart_Constant(object value) {
            this.value = value;
        }

        public override object Evaluate(object target, ExpressionContext context) {
            return value;
        }

    }

}