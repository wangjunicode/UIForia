using System;
using System.Collections.Generic;
using Debugger;

namespace Src {

    public class UIInputFieldTemplate : UITemplate {

        public UIInputFieldTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UIInputFieldElement);

        public override bool Compile(ParsedTemplate template) {
            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(typeof(UIInputFieldElement));
            templateToExpand.Compile();
            // todo -- make this not suck
            bindingList = bindingList ?? new List<Binding>();
            bindingList.AddRange(templateToExpand.rootElementTemplate.bindings);
            bindingList.AddRange(templateToExpand.rootElementTemplate.constantBindings);
            return base.Compile(template);
        }

        public override InitData CreateScoped(TemplateScope inputScope) {
            
            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(typeof(UIInputFieldElement));
            TemplateScope outputScope = new TemplateScope();
            
            outputScope.context = new UITemplateContext(inputScope.context.view);
            
            InitData instanceData = templateToExpand.CreateWithScope(outputScope);
            
            outputScope.context.rootElement = instanceData.element;

            instanceData.constantBindings = constantBindings;
            instanceData.conditionalBindings = conditionalBindings;
            instanceData.bindings = bindings;
            instanceData.context = inputScope.context;
            instanceData.inputBindings = inputBindings;
            instanceData.constantStyleBindings = constantStyleBindings;
            return instanceData;
        }

    }

}