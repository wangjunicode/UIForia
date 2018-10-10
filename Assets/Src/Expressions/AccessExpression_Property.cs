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
        private readonly string propertyName;

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