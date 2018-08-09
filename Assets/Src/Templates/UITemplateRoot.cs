using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class UITemplateRoot : UITemplate {

        public UIElement Instantiate(UIView view, List<object> props, List<UIElement> inputChildren) {
            UIElement root = (UIElement) Activator.CreateInstance(processedElementType.type);
            TemplateContext context = new TemplateContext(null);
            
            TemplateScope scope = new TemplateScope();
            
            scope.view = view;
            scope.context = context;
            scope.inputChildren = inputChildren;
            scope.rootElement = root;
            scope.props = new List<object>();

            for (int i = 0; i < childTemplates.Count; i++) {
                childTemplates[i].CreateScoped(scope);
            }

            return root;
        }

        public override bool TypeCheck() {
            throw new NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();
        }

    }

}