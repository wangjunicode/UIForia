using System;
using System.Reflection;

namespace Src {

    public class AccessExpression_RootField<T> : Expression<T> {

        private readonly FieldInfo cachedFieldInfo;
      //  private readonly Func<U, T> getter;
        
        public AccessExpression_RootField(Type type, string fieldName) {
            cachedFieldInfo = ReflectionUtil.GetFieldInfoOrThrow(type, fieldName);
        }

        public override T EvaluateTyped(ExpressionContext context) {
            // return ReflectionUtil.GetValue<U,T>(accessor, context.rootContext);
            return (T)cachedFieldInfo.GetValue(context.rootContext);
        }

        public override Type YieldedType => cachedFieldInfo.FieldType;

        public override object Evaluate(ExpressionContext context) {
            return cachedFieldInfo.GetValue(context.rootContext);
        }

        public override bool IsConstant() {
            return false;
        }

    }

    public class AccessExpression_RootProperty<T> : Expression<T> {

        private readonly PropertyInfo cachedPropertyInfo;
        //  private readonly Func<U, T> getter;
        
        public AccessExpression_RootProperty(Type type, string propertyName) {
            cachedPropertyInfo = ReflectionUtil.GetPropertyInfoOrThrow(type, propertyName);
        }

        public override T EvaluateTyped(ExpressionContext context) {
            // return ReflectionUtil.GetValue<U,T>(accessor, context.rootContext);
            return (T)cachedPropertyInfo.GetValue(context.rootContext);
        }

        public override Type YieldedType => cachedPropertyInfo.PropertyType;

        public override object Evaluate(ExpressionContext context) {
            return cachedPropertyInfo.GetValue(context.rootContext);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}