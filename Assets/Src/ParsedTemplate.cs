using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class ParsedTemplate {

        public Type type;
        public string filePath;
        public UIElementTemplate rootElement;
        public List<StyleTemplate> styles;
        public List<ImportDeclaration> imports;
        public ContextDefinition contextDefinition;
        public Dictionary<string, UIElement> slotMap;

        public bool isCompiled;
        public bool isSuccessfullyCompiled;

        public List<UITemplate> childTemplates => rootElement.childTemplates;
        public Type ElementType => rootElement.ElementType;

        private static readonly List<UIElement> EmptyElementList = new List<UIElement>(0);
        
        public UIElement CreateWithScope(TemplateScope scope) {

            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElement.processedElementType.type);

            List<UIElement> children = new List<UIElement>();
            
            for (int i = 0; i < rootElement.childTemplates.Count; i++) {
                children.Add(rootElement.childTemplates[i].CreateScoped(scope));        
            }

            instance.children = children;

            return instance;

        }

        public bool Compile() {
            if (isCompiled) return isSuccessfullyCompiled;
            isCompiled = true;
            isSuccessfullyCompiled = rootElement.Compile(contextDefinition);
            return isSuccessfullyCompiled;
        }

        public UIElement CreateWithoutScope(UIView view, List<UIElement> inputChildren = null) {
            
            if (!isCompiled) Compile();
            
            TemplateContext context = new TemplateContext(view);

            TemplateScope scope = new TemplateScope();
            scope.view = view;
            scope.context = context;
            scope.inputChildren = inputChildren ?? EmptyElementList;
            
            UIElement root = rootElement.CreateScoped(scope);

            context.rootElement = root;

            // rootPropsToAttributes().ApplyValues(root);

            return root;
        }

        private StyleTemplate GetStyleTemplate(string id) {
            for (int i = 0; i < styles.Count; i++) {
                if (styles[i].id == id) return styles[i];
            }

            return null;
        }

    }

}