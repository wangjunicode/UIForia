using System;
using System.Collections.Generic;

namespace Src {

    public class UISwitchTemplate : UITemplate {

        public UISwitchTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            UISwitchCaseElement instance = new UISwitchCaseElement();
            return GetCreationData(instance, scope.context);
        }

        public override bool Compile(ParsedTemplate template) {
            AttributeDefinition valueAttr = GetAttribute("value");

            Expression valueExpression = template.compiler.Compile(valueAttr.value);

            Type yieldedType = valueExpression.YieldedType;
            
            // int, string, (enum -> later)
            
            
            
            return base.Compile(template);
        }

    }

    // todo -- int / enum / string versions
    public class SwitchBinding<T> : Binding {

        private readonly Expression<T> expression;
        private readonly UISwitchCaseElement[] cases;
        private readonly UISwitchDefaultElement defaultCase;
        
        public SwitchBinding(Expression<T> expression) {
            this.expression = expression;
        }
        
        public override void Execute(UIElement element, UITemplateContext context) {
            UISwitchElement<T> switchElement = (UISwitchElement<T>) element;
            T currentValue = switchElement.currentValue;
            T result = expression.EvaluateTyped(context);
            if (!currentValue.Equals(result)) {
                // find case -- enable
            }
        }

        public override bool IsConstant() {
            return false;
        }

    }

}