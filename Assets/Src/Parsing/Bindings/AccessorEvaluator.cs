using System.Collections.Generic;

namespace Src {

    public class AccessorEvaluator : ExpressionEvaluator {

        private string contextName;
        private List<PropertyAccessorEvaluator> parts;

        public AccessorEvaluator(string contextName, List<PropertyAccessorEvaluator> parts) {
            this.parts = parts;
            this.contextName = contextName;
        }

        public override object Evaluate(TemplateContext context) {
            object target = null;//context.GetContext(contextName);
            object instance = target;

            if (target == null) return null;

            for (int i = 0; i < parts.Count; i++) {
                ExpressionEvaluator part = parts[i];
                if (part is ArrayAccessExpressionPart) { }
                else {
                    PropertyAccessorEvaluator propertyPart = (PropertyAccessorEvaluator) part;
                    instance = propertyPart.Evaluate(instance);
                    if (instance == null) return null;
                }
            }

            return null;
        }

    }

}