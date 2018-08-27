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
            
            return true;
        }

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

            outputScope.context.rootElement = instanceData.element;
            
            return instanceData;
        }

    }

}