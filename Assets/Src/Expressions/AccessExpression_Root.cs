using System;
using System.Reflection;

namespace Src {

    public class AccessExpression_Root<T> : Expression<T> {

        private readonly FieldInfo cachedFieldInfo;
      //  private readonly Func<U, T> getter;
        
        public AccessExpression_Root(Type type, string fieldName) {
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

}