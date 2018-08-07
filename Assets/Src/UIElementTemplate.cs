using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Src {

    public enum UIElementTemplateType {

        Slot,
        Text,
        Primitive,
        Template,
        Repeat

    }

    [DebuggerDisplay("{processedType.type.Name}")]
    public class UIElementTemplate {

        public string text;
        public UIElementTemplate parent;
        public readonly ProcessedType processedType;
        public readonly List<UIElementTemplate> children;

        public readonly List<LiteralBinding> literalBindings;
        public readonly List<ExpressionBinding> expressionBindings;
        public readonly UIElementTemplateType templateType;

        public UIElementTemplate(UIElementTemplateType templateType, ProcessedType processedType) {
            this.templateType = templateType;
            this.processedType = processedType;
            literalBindings = new List<LiteralBinding>();
            expressionBindings = new List<ExpressionBinding>();
            children = new List<UIElementTemplate>();
        }

        public bool IsSlot => templateType      == UIElementTemplateType.Slot;
        public bool IsTemplate => templateType  == UIElementTemplateType.Template;
        public bool IsPrimitive => templateType == UIElementTemplateType.Primitive;
        public bool IsRepeat => templateType == UIElementTemplateType.Repeat;

        private void ExpandTemplateElement(UIElement toExpand) {
            UIElementTemplate template = TemplateParser.GetParsedTemplate(toExpand.GetType());
            UIElement[] scopedChildren = toExpand.children;
            toExpand.children = null;
            template.Hydrate(toExpand, scopedChildren);
        }

        private UIElement Hydrate(UIElement root, UIElement[] scopedChildren) {
            TemplateContext context = new TemplateContext(root);
            root.providedContext = context;

            Stack<UIElementWithTemplate> stack = new Stack<UIElementWithTemplate>();

            stack.Push(new UIElementWithTemplate(root, this));

            List<UIElement> toExpand = new List<UIElement>();
            List<UIElement> actualChildList = new List<UIElement>();

            while (stack.Count > 0) {
                actualChildList.Clear();
                UIElementWithTemplate pair = stack.Pop();
                UIElement currentElement = pair.element;
                UIElementTemplate currentTemplate = pair.template;

                for (int i = 0; i < currentTemplate.children.Count; i++) {
                    UIElementTemplate template = currentTemplate.children[i];

                    if (template.IsSlot) {
                        Debug.Assert(scopedChildren != null, "scopedChildren != null");
                        actualChildList.AddRange(scopedChildren);
                        continue;
                    }

                    if (template.IsRepeat) {
                            
                    }

                    UIElement child = Activator.CreateInstance(template.processedType.type) as UIElement;
                    Debug.Assert(child != null, nameof(child) + " != null");
                    child.referenceContext = context;
                    
                    // for each attribute in template 
                    // generate binding
                    // if binding is constant or contains only constants then don't bind
                    // if binding has a once modifier dont' bind
                    
                    // binding -> target + expression(context) + change notification?
                    // context.AddBinding(element, propName, binding(static?));

                    child.originTemplate = template;
                    template.processedType.AssignObservedProperties(child);
                    template.AssignLiteralBindings(child);
                    template.CreateContextBindings(child, context);

                    stack.Push(new UIElementWithTemplate(child, template));

                    actualChildList.Add(child);

                    if (template.IsTemplate) {
                        toExpand.Add(child);
                    }

                }

                currentElement.children = actualChildList.ToArray();

            }

            for (int i = 0; i < toExpand.Count; i++) {
                ExpandTemplateElement(toExpand[i]);
            }

            return root;
        }

        public UIElement CreateElement(UIElement[] scopedChildren = null) {
            UIElement root = Activator.CreateInstance(processedType.type) as UIElement;
            processedType.AssignObservedProperties(root);
            Hydrate(root, scopedChildren);
            InitializeTree(root);
            return root;
        }

        private void InitializeTree(UIElement root) {
            root.Initialize();
            for (int i = 0; i < root.children.Length; i++) {
                UIElement child = root.children[i];
                List<ExpressionBinding> bindings = child.originTemplate?.expressionBindings;

                if (bindings == null) continue;

                for (int j = 0; j < bindings.Count; j++) {
                    //child.referenceContexts[binding.contextId]
                    ExpressionBinding binding = bindings[j];
                }
            }
        }

        private void AssignLiteralBindings(UIElement child) {
            for (int i = 0; i < literalBindings.Count; i++) {
                FieldInfo fieldInfo = processedType.GetField(literalBindings[i].propName);
                fieldInfo.SetValue(child, literalBindings[i].value);
            }
        }

        private void CreateContextBindings(UIElement element, TemplateContext context) {
            for (int i = 0; i < expressionBindings.Count; i++) {
                context.CreateBinding(expressionBindings[i], element);
            }
        }

        private struct UIElementWithTemplate {

            public readonly UIElement element;
            public readonly UIElementTemplate template;

            public UIElementWithTemplate(UIElement element, UIElementTemplate template) {
                this.element = element;
                this.template = template;
            }

        }

        public void AddLiteralBinding(string attrName, object literalValue) {
            literalBindings.Add(new LiteralBinding(attrName, literalValue));
        }

        public void AddExpressionBinding(ExpressionBinding binding) {
          
        }
    }

}