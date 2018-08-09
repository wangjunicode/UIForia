using System;
using System.Collections.Generic;

namespace Src {

    public class UIElementTemplate : UITemplate {

        public override UIElement CreateScoped(TemplateScope scope) {
            UIElement instance = (UIElement) Activator.CreateInstance(processedElementType.type);

            for (int i = 0; i < childTemplates.Count; i++) {
                childTemplates[i].CreateScoped(scope);
            }

            ParsedTemplate templateToExpand = TemplateParser.GetParsedTemplate(processedElementType);

            List<UIElement> inputChildren = instance.children;
            instance.children = new List<UIElement>();

            TemplateContext context = new TemplateContext(instance);

            scope = new TemplateScope();
            scope.view = instance.view;
            scope.context = context;
            scope.inputChildren = inputChildren;
            scope.rootElement = instance;
            scope.props = new List<object>();

            templateToExpand.Hydrate(instance, scope);

            for (int i = 0; i < childTemplates.Count; i++) {
                childTemplates[i].CreateScoped(scope);
            }

            return instance;
        }

        public void Compile(ContextDefinition context) {
            for (int i = 0; i < attributes.Count; i++) {
                ExpressionBinding binding = BindingGenerator.Generate(context, attributes[i].bindingExpression);
                // an expression binding is just a targetProperty linked to a function
                // that function is called every frame
                // when the output of the function changes, callbacks are invoked on the element if needed
                // targetProperty -> function()
                
            }
        }

        public override bool TypeCheck() {
            return true;
        }

    }

//    [DebuggerDisplay("{processedType.type.Name}")]
//    public class UIElementTemplate : UITemplate {
//
//        public UIElementTemplate parent;
//        public readonly ProcessedType processedType;
//        public readonly List<UIElementTemplate> children;
//
//        public readonly List<LiteralBinding> literalBindings;
//        public readonly List<ExpressionBinding> expressionBindings;
//        public readonly UIElementTemplateType templateType;
//
//        public UIElementTemplate() {
//          
//        }
//
//        public bool IsSlot => templateType == UIElementTemplateType.Slot;
//        public bool IsTemplate => templateType == UIElementTemplateType.Template;
//        public bool IsRepeat => templateType == UIElementTemplateType.Repeat;
//
//        private void ExpandTemplateElement(UIElement toExpand) {
//            UIElementTemplate template = TemplateParser.GetParsedTemplate(toExpand.GetType());
//            UIElement[] scopedChildren = toExpand.children;
//            toExpand.children = null;
//            template.Hydrate(toExpand, scopedChildren);
//        }
//
//        private UIElement Hydrate(UIElement root, UIElement[] scopedChildren) {
//            TemplateContext context = new TemplateContext(root);
//            root.providedContext = context;
//
//            Stack<UIElementWithTemplate> stack = new Stack<UIElementWithTemplate>();
//
//            stack.Push(new UIElementWithTemplate(root, this));
//
//            List<UIElement> toExpand = new List<UIElement>();
//            List<UIElement> actualChildList = new List<UIElement>();
//
//            while (stack.Count > 0) {
//                actualChildList.Clear();
//                UIElementWithTemplate pair = stack.Pop();
//                UIElement currentElement = pair.element;
//                UIElementTemplate currentTemplate = pair.template;
//
//                for (int i = 0; i < currentTemplate.children.Count; i++) {
//                    UIElementTemplate template = currentTemplate.children[i];
//
//                    if (template.IsSlot) {
//                        Debug.Assert(scopedChildren != null, "scopedChildren != null");
//                        actualChildList.AddRange(scopedChildren);
//                        continue;
//                    }
//
//                    if (template.IsRepeat) { }
//
//                    UIElement child = Activator.CreateInstance(template.processedType.type) as UIElement;
//                    Debug.Assert(child != null, nameof(child) + " != null");
//                    child.referenceContext = context;
////                    child.style.LoadFromTree(root);
//
//                    // for each attribute in template 
//                    // generate binding
//                    // if binding is constant or contains only constants then don't bind
//                    // if binding has a once modifier dont' bind
//
//                    // binding -> target + expression(context) + change notification?
//                    // context.AddBinding(element, propName, binding(static?));
//
//                    child.originTemplate = template;
//                    template.processedType.AssignObservedProperties(child);
//                    template.AssignLiteralBindings(child);
//                    template.CreateContextBindings(child, context);
//
//                    stack.Push(new UIElementWithTemplate(child, template));
//
//                    actualChildList.Add(child);
//
//                    if (template.IsTemplate) {
//                        toExpand.Add(child);
//                    }
//                }
//
//                currentElement.children = actualChildList.ToArray();
//            }
//
//            for (int i = 0; i < toExpand.Count; i++) {
//                ExpandTemplateElement(toExpand[i]);
//            }
//
//            return root;
//        }
//
//        public UIElement CreateElement(UIView view) {
//            UIElement root = Activator.CreateInstance(processedType.type) as UIElement;
//            return null;
//        }
//
//        public UIElement CreateElement(UIElement[] scopedChildren = null) {
//            UIElement root = Activator.CreateInstance(processedType.type) as UIElement;
//            processedType.AssignObservedProperties(root);
//            Hydrate(root, scopedChildren);
//            InitializeTree(root);
//            return root;
//        }
//
//        private void InitializeTree(UIElement root) {
//            root.Initialize();
//            for (int i = 0; i < root.children.Length; i++) {
//                UIElement child = root.children[i];
//                List<ExpressionBinding> bindings = child.originTemplate?.expressionBindings;
//
//                if (bindings == null) continue;
//
//                for (int j = 0; j < bindings.Count; j++) {
//                    //child.referenceContexts[binding.contextId]
//                    ExpressionBinding binding = bindings[j];
//                }
//            }
//        }
//
//        private void AssignLiteralBindings(UIElement child) {
//            for (int i = 0; i < literalBindings.Count; i++) {
//                FieldInfo fieldInfo = processedType.GetField(literalBindings[i].propName);
//                fieldInfo.SetValue(child, literalBindings[i].value);
//            }
//        }
//
//        private void CreateContextBindings(UIElement element, TemplateContext context) {
//            for (int i = 0; i < expressionBindings.Count; i++) {
//                context.CreateBinding(expressionBindings[i], element);
//            }
//        }
//
//        private struct UIElementWithTemplate {
//
//            public readonly UIElement element;
//            public readonly UIElementTemplate template;
//
//            public UIElementWithTemplate(UIElement element, UIElementTemplate template) {
//                this.element = element;
//                this.template = template;
//            }
//
//        }
//
//        public void AddLiteralBinding(string attrName, object literalValue) {
//            literalBindings.Add(new LiteralBinding(attrName, literalValue));
//        }
//
//        public void AddExpressionBinding(ExpressionBinding binding) { }
//
//    }

}