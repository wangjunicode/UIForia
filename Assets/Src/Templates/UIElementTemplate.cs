using System;
using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

        private Type rootType;
        private readonly string typeName;
        
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
            
            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(rootType);
            templateToExpand.Compile();
            // todo -- make this not suck
            bindingList = bindingList ?? new List<Binding>();
            bindingList.AddRange(templateToExpand.rootElementTemplate.bindings);
            bindingList.AddRange(templateToExpand.rootElementTemplate.constantBindings);
            // todo -- remove duplicate bindings
            base.Compile(template);

            return true;
        }

        public override Type elementType => rootType;

        public override InitData CreateScoped(TemplateScope inputScope) {
            List<InitData> scopedChildren = new List<InitData>(childTemplates.Count);

            for (int i = 0; i < childTemplates.Count; i++) {
                scopedChildren.Add(childTemplates[i].CreateScoped(inputScope));
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(rootType);
            TemplateScope outputScope = new TemplateScope();

            // todo -- some templates don't need their own scope
            outputScope.context = new UITemplateContext(inputScope.context.view);
            outputScope.inputChildren = scopedChildren;

            InitData instanceData = templateToExpand.CreateWithScope(outputScope);

            // todo -- not sure this is safe to overwrite bindings here
            // actually the only bindings allowed on <Contents> tag should be styles
            // which would make this ok. need to merge styles though
            instanceData.constantBindings = constantBindings;
            instanceData.conditionalBindings = conditionalBindings;
            instanceData.bindings = bindings;
            instanceData.context = inputScope.context;
            instanceData.inputBindings = inputBindings;
            instanceData.constantStyleBindings = constantStyleBindings;
            outputScope.context.rootElement = instanceData.element;
            
            return instanceData;
        }

    }

}