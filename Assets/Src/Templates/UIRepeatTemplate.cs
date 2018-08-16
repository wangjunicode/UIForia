using System;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        private Binding[] bindings;

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public void Compile(List<ContextDefinition> contextDefinitions) {

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