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
            throw new System.NotImplementedException();
        }

        public void Compile() {
            bindings = new Binding[attributes.Count];

            if (attributes.Find((definition => definition.name == "if")) != null) { }
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            // todo -- find solution for constant expressions so they are not registered

            UITextElement instance = new UITextElement();

            scope.view.RegisterBindings(instance, bindings, scope.context);

            return instance;
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