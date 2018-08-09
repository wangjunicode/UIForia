using System.Collections.Generic;

namespace Src {

    public class AccessorBinding : ExpressionBinding {

        private string contextName;
        private List<PropertyAcessorBinding> parts;

        public AccessorBinding(string contextName, List<PropertyAcessorBinding> parts) {
            this.parts = parts;
            this.contextName = contextName;
        }

        public override object Evaluate(TemplateContext context) {
            object target = null;//context.GetContext(contextName);
            object instance = target;

            if (target == null) return null;

            for (int i = 0; i < parts.Count; i++) {
                ExpressionBinding part = parts[i];
                if (part is ArrayAccessExpressionPart) { }
                else {
                    PropertyAcessorBinding propertyPart = (PropertyAcessorBinding) part;
                    instance = propertyPart.Evaluate(instance);
                    if (instance == null) return null;
                }
            }

            return null;
        }

    }

}