using System;
using System.Reflection;

namespace Src {

    public class PropertyAccessorEvaluator : Expression {

        private readonly string fieldName;
        private Type cachedType;
        private FieldInfo cachedFieldInfo;

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public PropertyAccessorEvaluator(string fieldName) {
            this.fieldName = fieldName;
        }

        public object Evaluate(object target) {
            if (target == null) return null;
            if (target.GetType() == cachedType) {
                return cachedFieldInfo.GetValue(target);
            }

            cachedType = target.GetType();
            cachedFieldInfo = cachedType.GetField(fieldName, Flags);

            if (cachedFieldInfo == null) return null;

            return cachedFieldInfo.GetValue(target);
        }

        public override object Evaluate(TemplateContext context) {
            throw new NotImplementedException();
        }

    }

}
