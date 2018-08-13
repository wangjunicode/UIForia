using System;

namespace Src {

    public class UITextTemplate : UITemplate {

        private Binding[] bindings;

        public readonly string text;

        public UITextTemplate(string text) {
            this.text = text;
        }

        public override Type ElementType => typeof(UITextElement);

        public override bool TypeCheck() {
            throw new NotImplementedException();
        }

        public void Compile() {
            bindings = new Binding[attributes.Count];

        }

        public override UIElement CreateScoped(TemplateScope scope) {
            // todo -- find solution for constant expressions so they are not registered

            UITextElement instance = new UITextElement();
            
            scope.view.CreateTextPrimitive(instance, text);
            
//            scope.view.RegisterBindings(instance, bindings, scope.context);

            return instance;
        }

    }

    public class SetUITextBinding : Binding {

        public override void Execute(UIElement element, TemplateContext context) {
//            if (element._textChanged) {
//                element._textChanged = false;
//            }
//            context.view.SetElementText(element, ((UITextElement) element).label);
        }

    }

}


/*
 *  Built in elements can have hand written binding handlers (but probably don't need to other than control flow)
 * 
 * Special bindings for all elements
 *    if (visibility)
 *    style
 *    style.*
 *    layout.*
 *    
 *    event handlers
 * 
*/