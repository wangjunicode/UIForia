using System;
using System.Reflection;

namespace Src {

    public class PropertyAcessorBinding : ExpressionBinding {

        private readonly string fieldName;
        private Type cachedType;
        private FieldInfo cachedFieldInfo;

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public PropertyAcessorBinding(string fieldName) {
            this.fieldName = fieldName;
        }

        public object Evaluate(object target) {
            if (target == null) return null;
            if (target.GetType() == cachedType) {
                return cachedFieldInfo.GetValue(target);
            }
            else {
                cachedType = target.GetType();
                cachedFieldInfo = cachedType.GetField(fieldName, Flags);

                if (cachedFieldInfo == null) return null;

                return cachedFieldInfo.GetValue(target);
            }
        }

    }

}