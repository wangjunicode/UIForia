using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class ParsedTemplate {

        public Type type;
        public string filePath;
        public UIElementTemplate rootElement;
        public List<UIStyle> styles;
        public List<ImportDeclaration> imports;
        public ContextDefinition contextDefinition;

        private bool isCompiled;

        public List<UITemplate> childTemplates => rootElement.childTemplates;
        public Type ElementType => rootElement.ElementType;

        private static readonly List<UIElement> EmptyElementList = new List<UIElement>(0);

        public UIElement CreateWithScope(TemplateScope scope) {
            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElement.processedElementType.type);

            List<UIElement> children = new List<UIElement>();

            for (int i = 0; i < rootElement.childTemplates.Count; i++) {
                UIElement child = rootElement.childTemplates[i].CreateScoped(scope);
                if (child != null) {
                    children.Add(child);
                    child.parent = instance;
                }
            }

            instance.children = children;
            rootElement.ApplyStyles(instance, scope);

            return instance;
        }

        private void Compile() {
            if (isCompiled) return;
            
            Stack<UITemplate> stack = new Stack<UITemplate>();
            
            stack.Push(rootElement);
            
            while (stack.Count > 0) {
                UITemplate template = stack.Pop();
                template.CompileStyles(this);
                template.Compile(this);
                for (int i = 0; i < template.childTemplates.Count; i++) {
                    stack.Push(template.childTemplates[i]);
                }
            }

            isCompiled = true;
        }

        public UIElement CreateWithoutScope(UIView view, List<UIElement> inputChildren = null) {
            if (!isCompiled) Compile();

            TemplateContext context = new TemplateContext(view);

            TemplateScope scope = new TemplateScope();
            scope.view = view;
            scope.context = context;
            scope.inputChildren = inputChildren ?? EmptyElementList;

            UIElement root = (UIElement) Activator.CreateInstance(rootElement.processedElementType.type);
            root.children = new List<UIElement>();

            for (int i = 0; i < childTemplates.Count; i++) {
                root.children.Add(childTemplates[i].CreateScoped(scope));
                root.children[i].parent = root;
            }

            rootElement.ApplyStyles(root, scope);
            context.rootElement = root;

            return root;
        }

        public UIStyle GetStyleInstance(string styleName) {
            // todo handle searching imports
            for (int i = 0; i < styles.Count; i++) {
                if (styles[i].localId == styleName) {
                    return styles[i];
                }
            }

            return null;
        }

    }

}