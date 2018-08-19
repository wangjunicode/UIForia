using System;
using System.Reflection;

namespace Src {

    public class AccessExpression_Root : Expression {

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly string fieldName;
        private readonly FieldInfo cachedFieldInfo;

        public AccessExpression_Root(Type type, string fieldName) {
            cachedFieldInfo = type.GetField(fieldName, Flags);
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