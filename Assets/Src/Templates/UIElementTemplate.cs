using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

        private Binding[] bindings;

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            List<UIElementCreationData> scopedChildren = new List<UIElementCreationData>(childTemplates.Count);

            for (int i = 0; i < childTemplates.Count; i++) {
                scopedChildren.Add(childTemplates[i].CreateScoped(scope));
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(processedElementType);

            UITemplateContext context = new UITemplateContext(scope.view);

            TemplateScope outputScope = new TemplateScope(scope.outputList);

            outputScope.context = context;
            outputScope.inputChildren = scopedChildren;
            outputScope.view = scope.view;

            UIElement instance = templateToExpand.CreateWithScope(outputScope);
            instance.name = name;

            ApplyConstantStyles(instance, scope);

            context.rootElement = instance;
            
            return new UIElementCreationData(instance, bindings, scope.context);
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

            bindingList.AddRange(dynamicStyleBindings);
            
            bindings = bindingList.ToArray();
            
            return true;
        }

    }

}