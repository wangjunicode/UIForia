using System;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        private Binding[] bindings;

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public void Compile(List<ContextDefinition> contextDefinitions) {

            // {this.someobject.list}
            // Getter("someobject", Getter("list"));
            ExpressionEvaluator listGetter = BindingGenerator.Generate(contextDefinitions[0],
                attributes.Find((a) => a.name == "list").bindingExpression);

            bindings = new Binding[] {
               // new RepeatEnterBinding()
            };
            
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            UIRepeatElement element = new UIRepeatElement();


            scope.view.RegisterBindings(element, bindings, scope.context);

            return element;
        }

    }

}