using System;
using System.Collections.Generic;
using System.Linq;
using Src.Elements;
using Src.Util;
using UnityEngine;

namespace Src {

    // wraps a ParsedTemplate, this is what is created by parent templates
    // properties need to be merged from the parsed template <Content/> tag (root)
    public class UIElementTemplate : UITemplate {

        private Type rootType;
        private readonly string typeName;
        private ParsedTemplate templateToExpand;
        private TemplateType templateType;

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
            
            if (typeof(UIPrimitiveElement).IsAssignableFrom(rootType)) {
                // assert no children
                // assert no template
                templateType = TemplateType.Primitive;
                base.Compile(template);
                return true;
            }

            if (typeof(UIContainerElement).IsAssignableFrom(rootType)) {
                templateType = TemplateType.Container;
                base.Compile(template);
                return true;
            }

            templateType = TemplateType.Template;

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

            MetaData instanceData = MetaData.GetFromPool();

            switch (templateType) {
                case TemplateType.Primitive:
                    if (scopedChildren.Count > 0) {
                        throw new Exception("Primitive elements cannot have children. Children were passed to " + rootType.Name);
                    }

                    instanceData.element = (UIElement) Activator.CreateInstance(rootType);
                    instanceData.element.ownChildren = ArrayPool<UIElement>.Empty;
                    instanceData.element.templateChildren = ArrayPool<UIElement>.Empty;
                    
                    break;
                case TemplateType.Container:
                    instanceData.element = (UIElement) Activator.CreateInstance(rootType);
                    
                    if (scopedChildren.Count == 0) {
                        instanceData.element.ownChildren = ArrayPool<UIElement>.Empty;
                        instanceData.element.templateChildren = ArrayPool<UIElement>.Empty;
                        break;
                    }

                    instanceData.element.templateChildren = ArrayPool<UIElement>.GetExactSize(scopedChildren.Count);
                    instanceData.element.ownChildren = instanceData.element.templateChildren;
                    for (int i = 0; i < scopedChildren.Count; i++) {
                        MetaData child = scopedChildren[i];
                        instanceData.element.templateChildren[i] = child.element;
                        instanceData.AddChild(child);
                    }

                    outputScope.inputChildren = null;
                    break;
                case TemplateType.Template:
                    instanceData = templateToExpand.CreateWithScope(outputScope);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (templateType == TemplateType.Template) {
                // todo -- merge bindings here
            }
            else {
                // todo -- used un-merged bindings
            }

            // todo -- not sure this is safe to overwrite bindings here probably need to merge
            // actually the only bindings allowed on <Contents> tag should be styles
            // which would make this ok. need to merge styles though, maybe input handlers though?
            instanceData.bindings = bindings;
            instanceData.context = inputScope.context;
            instanceData.constantBindings = constantBindings;
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
        
        private enum TemplateType {

            Primitive,
            Container,
            Template

        }

    }

}