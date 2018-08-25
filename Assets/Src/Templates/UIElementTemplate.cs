using System;
using System.Collections.Generic;
using Src.Compilers;

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
            base.Compile(template);
            
            if (rootType == null) {
                rootType = TypeProcessor.GetType(typeName, template.imports).rawType;
            }
            BindExpressionCompiler bindingCompiler = new BindExpressionCompiler(template.contextDefinition);
            List<AttributeDefinition> userAttrs = GetUserAttributes();

            for (int i = 0; i < userAttrs.Count; i++) {
                bindingCompiler.Compile(rootType, userAttrs[i].key, userAttrs[i].value);
            }
            
            return true;
        }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            List<UIElementCreationData> scopedChildren = new List<UIElementCreationData>(childTemplates.Count);

            for (int i = 0; i < childTemplates.Count; i++) {
                scopedChildren.Add(childTemplates[i].CreateScoped(scope));
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(rootType);
            UITemplateContext context = new UITemplateContext(scope.view);
            TemplateScope outputScope = new TemplateScope(scope.outputList);

            outputScope.context = context;
            outputScope.inputChildren = scopedChildren;
            outputScope.view = scope.view;

            UIElement instance = templateToExpand.CreateWithScope(outputScope);

            context.rootElement = instance;

            return GetCreationData(instance, context);
        }

    }

}