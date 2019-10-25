using System;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Generated {

    public partial class UIForiaGeneratedTemplates_TestApp {
        
        public Func<UIElement, TemplateScope2, UIElement> Template_dbc412ec15ef3684fb97d17debe86768 = (UIForia.Elements.UIElement root, UIForia.Compilers.TemplateScope2 scope) =>
        {
            UIForia.Elements.UIElement targetElement_1;

            if (root == null)
            {
                root = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Test.TestData.LoadTemplateHydrate), default(UIForia.Elements.UIElement), 1, 0);
            }

            // <Text />    'Hydrate me'
            targetElement_1 = scope.application.CreateElementFromPoolWithType(typeof(UIForia.Elements.UITextElement), root, 0, 0);
            ((UIForia.Elements.UITextElement)targetElement_1).text = "Hydrate me";
            root.children.array[0] = targetElement_1;
            return root;
        }; 

        

    }

}