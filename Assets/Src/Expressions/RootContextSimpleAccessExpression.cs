using System;
using System.Reflection;

namespace Src {

    public class RootContextSimpleAccessExpression : Expression {

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly string fieldName;
        private readonly FieldInfo cachedFieldInfo;

        public RootContextSimpleAccessExpression(Type type, string fieldName) {
            cachedFieldInfo = type.GetField(fieldName, Flags);
        }
        
        public override object Evaluate(TemplateContext context) {
            return cachedFieldInfo.GetValue(context.target);
        }

    }

}