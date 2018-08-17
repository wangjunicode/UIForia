using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

        private Binding[] bindings;

        public override UIElement CreateScoped(TemplateScope scope) {
            List<UIElement> scopedChildren = new List<UIElement>(childTemplates.Count);

            for (int i = 0; i < childTemplates.Count; i++) {
                UIElement child = childTemplates[i].CreateScoped(scope);
                if (child != null) {
                    scopedChildren.Add(child);
                }
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(processedElementType);

            UITemplateContext context = new UITemplateContext(scope.view);

            TemplateScope outputScope = new TemplateScope();

            outputScope.context = context;
            outputScope.inputChildren = scopedChildren;
            outputScope.view = scope.view;

            UIElement instance = templateToExpand.CreateWithScope(outputScope);

            ApplyStyles(instance, scope);

            context.rootElement = instance;

            if (bindings != null && bindings.Length > 0) {
                scope.view.RegisterBindings(instance, bindings, scope.context);
            }

            return instance;
        }

        public override bool Compile(ParsedTemplate template) {
            if (!base.Compile(template)) {
                return false;
            }

            if (bindings != null) return true;

            if (attributes == null) {
                return true;
            }

            List<Binding> bindingList = new List<Binding>();

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition attr = attributes[i];
                
                if(attr.key.StartsWith("style")) continue;
                
                //Binding binding = TryGeneratingBinding(attr, template);
                
                //bindingList.Add(ExpressionCompiler.GenerateFromAttribute(template.contextDefinition, attr));
            }

            bindings = bindingList.ToArray();
            
            return true;
        }

        public override bool TypeCheck() {
            return true;
        }

    }

}