using System;
using System.Collections.Generic;
using System.Linq;

namespace Src {

    // wraps a ParsedTemplate, this is what is created by parent templates
    // properties need to be merged from the parsed template <Content/> tag (root)
    public class UIElementTemplate : UITemplate {

        private Type rootType;
        private readonly string typeName;
        private ParsedTemplate templateToExpand;
        
        public UIElementTemplate(string typeName, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) {
            this.typeName = typeName;
        }

        public UIElementTemplate(Type rootType, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) {
            this.rootType = rootType;
        }

        public Type RootType => rootType;

        public override bool Compile(ParsedTemplate template) {
            if (rootType == null) {
                rootType = TypeProcessor.GetType(typeName, template.imports).rawType;
            }

            templateToExpand = TemplateParser.GetParsedTemplate(rootType);
            templateToExpand.Compile();
            // todo -- make this not suck
            bindingList = bindingList ?? new List<Binding>();
            bindingList.AddRange(templateToExpand.rootElementTemplate.bindings);
            bindingList.AddRange(templateToExpand.rootElementTemplate.constantBindings);
            constantStyleBindings.AddRange(templateToExpand.rootElementTemplate.constantStyleBindings);
            baseStyles.AddRange(templateToExpand.rootElementTemplate.baseStyles);
            // todo -- remove duplicate bindings
            base.Compile(template);
            return true;
        }

        public override Type elementType => rootType;

        public override MetaData CreateScoped(TemplateScope inputScope) {
            List<MetaData> scopedChildren = new List<MetaData>(childTemplates.Count);

            for (int i = 0; i < childTemplates.Count; i++) {
                scopedChildren.Add(childTemplates[i].CreateScoped(inputScope));
            }

            TemplateScope outputScope = new TemplateScope();

            // todo -- some templates don't need their own scope
            outputScope.context = new UITemplateContext(inputScope.context.view);
            outputScope.inputChildren = scopedChildren;

            MetaData instanceData = templateToExpand.CreateWithScope(outputScope);

            // todo -- not sure this is safe to overwrite bindings here probably need to merge
            // actually the only bindings allowed on <Contents> tag should be styles
            // which would make this ok. need to merge styles though
            instanceData.constantBindings = constantBindings;
            instanceData.conditionalBindings = conditionalBindings;
            instanceData.bindings = bindings;
            instanceData.context = inputScope.context;
            instanceData.inputBindings = inputBindings;
            instanceData.constantStyleBindings = constantStyleBindings;
            instanceData.element.templateAttributes = templateAttributes;
            instanceData.baseStyles = baseStyles;
            instanceData.mouseEventHandlers = mouseEventHandlers;
            instanceData.dragEventCreators = dragEventCreators;
            instanceData.dragEventHandlers = dragEventHandlers;
            instanceData.keyboardEventHandlers = keyboardEventHandlers;
            
            outputScope.context.rootElement = instanceData.element;

            AssignContext(instanceData.element, outputScope.context);
            
            return instanceData;
        }

        private void AssignContext(UIElement element, UITemplateContext context) {
            element.templateContext = context;
            
            if (element.ownChildren == null) return;
            
            for (int i = 0; i < element.ownChildren.Length; i++) {
                AssignContext(element.ownChildren[i], context);
            }
        }

    }

}