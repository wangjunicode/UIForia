using System;
using System.Collections.Generic;
using UIForia.Util;

namespace UIForia {

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
        protected override Type elementType => rootType;

        public override void Compile(ParsedTemplate template) {
            if (rootType == null) {
                rootType = TypeProcessor.GetType(typeName, template.imports).rawType;
            }

            templateToExpand = TemplateParser.GetParsedTemplate(rootType);
            templateToExpand.Compile();

            base.Compile(template);
            
            if (template.rootElementTemplate == this) {
                return;
            }

            UITemplate expandedRoot = templateToExpand.rootElementTemplate;
            
            // todo solve roots added twice
            int triggeredCount = triggeredBindings.Length;
            
            triggeredBindings = MergeBindingArray(triggeredBindings, expandedRoot.triggeredBindings);
            perFrameBindings = MergeBindingArray(perFrameBindings, expandedRoot.perFrameBindings);
            mouseEventHandlers = MergeArray(mouseEventHandlers, expandedRoot.mouseEventHandlers);
            keyboardEventHandlers = MergeArray(keyboardEventHandlers, expandedRoot.keyboardEventHandlers);
            dragEventCreators = MergeArray(dragEventCreators, expandedRoot.dragEventCreators);
            dragEventHandlers = MergeArray(dragEventHandlers, expandedRoot.dragEventHandlers);
            baseStyles = MergeArray(baseStyles, expandedRoot.baseStyles);
            Array.Reverse(baseStyles);

            for (int i = 0; i < triggeredCount; i++) {
                triggeredBindings[i].useRootContext = true;
            }
        }

        private static Binding[] MergeBindingArray(Binding[] a, Binding[] b) {
            int startCount = a.Length;
            a = ResizeToMerge(a, b);
            int idx = 0;
            for (int i = 0; i < startCount; i++) {
                a[i].useRootContext = true;
            }
            for (int i = startCount; i < a.Length; i++) {
                a[i] = b[idx++];
            }

            return a;
        }
        
        private static T[] MergeArray<T>(T[] a, T[] b) {
            int startCount = a.Length;
            a = ResizeToMerge(a, b);
            int idx = 0;
          
            for (int i = startCount; i < a.Length; i++) {
                a[i] = b[idx++];
            }

            return a;
        }
        
        private static T[] ResizeToMerge<T>(T[] a, T[] b) {
            if (a.Length == 0 && b.Length == 0) {
                return a;
            }

            if (a.Length > 0 && b.Length == 0) {
                return a;
            }

            if (a.Length == 0 && b.Length > 0) {
                return new T[b.Length];
            }

            if (a.Length > 0 && b.Length > 0) {
                Array.Resize(ref a, a.Length + b.Length);
            }

            return a;
        }

        protected void ValidateProps() {
//            ProcessedType processedType;
//            processedType.ValidateProps(bindings);
        }

        public UIElement CreateUnscoped(UIView view) {
            UIElement element = (UIElement) Activator.CreateInstance(rootType);
            element.flags |= UIElementFlags.TemplateRoot;
            element.OriginTemplate = this;
            templateToExpand.Compile();

            List<UITemplate> actualChildren = childTemplates;

            UITemplateContext context = new UITemplateContext(element);
            TemplateScope scope = new TemplateScope(element, context);

            element.children = ArrayPool<UIElement>.GetExactSize(actualChildren.Count);

            element.templateParent = null;
            element.templateRoot = element;

            for (int i = 0; i < element.children.Length; i++) {
                element.children[i] = actualChildren[i].CreateScoped(scope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
                element.children[i].templateRoot = element;
            }

            UIChildrenElement childrenElement = element.TranscludedChildren;
            if (childrenElement != null) {
                childrenElement.children = ArrayPool<UIElement>.Empty;
            }

            AssignContext(element, context);

            return element;
        }

        // children of this element are transcluded
        // actual children are built from parsed template's root children
        public override UIElement CreateScoped(TemplateScope inputScope) {
            // todo -- some templates don't need their own scope

            UIElement element = (UIElement) Activator.CreateInstance(rootType);
            element.flags |= UIElementFlags.TemplateRoot;
            element.OriginTemplate = this;

            templateToExpand.Compile();

            List<UITemplate> transcludedTemplates = childTemplates;
            List<UITemplate> actualChildren = templateToExpand.childTemplates;

            UITemplateContext context = new UITemplateContext(element);
            TemplateScope scope = new TemplateScope(element, context);

            element.children = ArrayPool<UIElement>.GetExactSize(actualChildren.Count);

            for (int i = 0; i < element.children.Length; i++) {
                element.children[i] = actualChildren[i].CreateScoped(scope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
                element.children[i].templateRoot = inputScope.rootElement;
            }

            UIChildrenElement childrenElement = element.TranscludedChildren;

            if (childrenElement != null) {
                childrenElement.children = new UIElement[transcludedTemplates.Count];
                for (int i = 0; i < childrenElement.children.Length; i++) {
                    childrenElement.children[i] = transcludedTemplates[i].CreateScoped(inputScope);
                    childrenElement.children[i].parent = childrenElement;
                    childrenElement.children[i].templateParent = element;
                    childrenElement.children[i].templateRoot = inputScope.rootElement;
                }
            }

            // todo -- create slots here

            AssignContext(element, context);

            // find <Slot>
            //     -> attach from input

            return element;
        }

    }

}