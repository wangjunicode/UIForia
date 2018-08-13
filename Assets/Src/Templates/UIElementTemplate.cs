using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

        private Binding[] bindings;

        public override UIElement CreateScoped(TemplateScope scope) {

            List<UIElement> scopedChildren = new List<UIElement>(childTemplates.Count);
            
            for (int i = 0; i < childTemplates.Count; i++) {
                scopedChildren.Add(childTemplates[i].CreateScoped(scope));
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(processedElementType);
      
            TemplateContext context = new TemplateContext(scope.view);

            TemplateScope outputScope = new TemplateScope();
            
            outputScope.context = context;
            outputScope.inputChildren = scopedChildren;
            outputScope.view = scope.view;
            outputScope.styleTemplates = templateToExpand.styles;
            
            UIElement instance = templateToExpand.CreateWithScope(outputScope);
            
            ApplyStyles(instance, scope);
            
            context.rootElement = instance;

            if (bindings != null && bindings.Length > 0) {
                scope.view.RegisterBindings(instance, bindings, scope.context);
            }

            return instance;
        }
        
        public bool Compile(ContextDefinition context) {
            if (bindings != null) return true;

            if (attributes == null) {
                return true;
            }
            
            bindings = new Binding[attributes.Count];

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];
                bindings[i] = BindingGenerator.GenerateFromAttribute(context, attr);
            }

            return true;
        }

        public override bool TypeCheck() {
            return true;
        }

    }

}