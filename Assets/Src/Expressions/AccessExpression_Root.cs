using System;
using System.Reflection;

namespace Src {

    public class AccessExpression_Root<T> : Expression<T> {

        private readonly FieldInfo cachedFieldInfo;

        public AccessExpression_Root(Type type, string fieldName) {
            cachedFieldInfo = ReflectionUtil.GetFieldInfoOrThrow(type, fieldName);
        }

        public override T EvaluateTyped(ExpressionContext context) {
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