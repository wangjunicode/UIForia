using System;
using System.Reflection;

namespace UIForia {

    public class AccessExpression_RootField<T> : Expression<T> {

        private readonly FieldInfo cachedFieldInfo;

        public AccessExpression_RootField(Type type, string fieldName) {
            cachedFieldInfo = ReflectionUtil.GetFieldInfoOrThrow(type, fieldName);
        }

        public override T Evaluate(ExpressionContext context) {
            return (T) cachedFieldInfo.GetValue(context.rootObject);
        }

        public override Type YieldedType => cachedFieldInfo.FieldType;

        public override bool IsConstant() {
            return false;
        }

    }

    public class AccessExpression_RootField_Linq<T, U> : Expression<T> {

        private readonly Func<U, T> getter;

        public AccessExpression_RootField_Linq(Type type, string fieldName) {
            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfoOrThrow(type, fieldName);
            getter = (Func<U, T>) ReflectionUtil.GetLinqFieldAccessors(type, fieldInfo.FieldType, fieldName).getter;
        }

        public override T Evaluate(ExpressionContext context) {
            return getter((U) context.rootObject);
        }

        public override Type YieldedType => typeof(T);

        public override bool IsConstant() {
            return false;
        }

    }

    public class AccessExpression_RootProperty<T> : Expression<T> {

        private readonly PropertyInfo cachedPropertyInfo;

        public AccessExpression_RootProperty(Type type, string propertyName) {
            cachedPropertyInfo = ReflectionUtil.GetPropertyInfoOrThrow(type, propertyName);
        }

        public override T Evaluate(ExpressionContext context) {
            return (T) cachedPropertyInfo.GetValue(context.rootObject);
        }

        public override Type YieldedType => cachedPropertyInfo.PropertyType;

        public override bool IsConstant() {
            return false;
        }

    }

    public class AccessExpression_RootProperty_Linq<T, U> : Expression<T> {

        private readonly Func<U, T> getter;

        public AccessExpression_RootProperty_Linq(Type type, string fieldName) {
            PropertyInfo propertyInfo = ReflectionUtil.GetPropertyInfoOrThrow(type, fieldName);
            getter = (Func<U, T>) ReflectionUtil.GetLinqPropertyAccessors(type, propertyInfo.PropertyType, fieldName).getter;
        }

        public override T Evaluate(ExpressionContext context) {
            return getter((U) context.rootObject);
        }

        public override Type YieldedType => typeof(T);

        public override bool IsConstant() {
            return false;
        }

    }

}