using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

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

            context.rootElement = instance;
            
            return new UIElementCreationData(
                name,
                instance,
                styleDefinition,
                null, 
                scope.context
            );
        }

        public override bool Compile(ParsedTemplate template) {
            return true;
        }

    }

}