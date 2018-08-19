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

        private static readonly List<RegistrationData> EmptyElementList = new List<RegistrationData>(0);

        public UIElement CreateWithScope(TemplateScope scope) {
            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElement.processedElementType.rawType);

            RegistrationData instanceData = new RegistrationData(instance, null, scope.context);
            
            List<RegistrationData> children = new List<RegistrationData>();

            for (int i = 0; i < rootElement.childTemplates.Count; i++) {
                UITemplate template = rootElement.childTemplates[i];
                if (template is UIChildrenTemplate) {
                    children.AddRange(scope.inputChildren);
                }
                else {
                    children.Add(template.CreateScoped(scope));
                }
            }

            for (int i = 0; i < children.Count; i++) {
                scope.SetParent(children[i], instanceData);
            }
                        
            rootElement.ApplyConstantStyles(instance, scope);

            return instance;
        }

       
        public UIElement CreateWithoutScope(UIView view) {
            if (!isCompiled) Compile();

            UITemplateContext context = new UITemplateContext(view);

            List<RegistrationData> outputList = new List<RegistrationData>();
            
            TemplateScope scope = new TemplateScope(outputList);
            scope.view = view;
            scope.context = context;
            scope.inputChildren = EmptyElementList;

            UIElement root = (UIElement) Activator.CreateInstance(rootElement.processedElementType.rawType);
            context.rootElement = root;

            RegistrationData rootData = new RegistrationData(root, null, context);
            
            scope.SetParent(rootData, default(RegistrationData));
            
            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), rootData);
            }

            rootElement.ApplyConstantStyles(root, scope);

            scope.RegisterAll();
            
            return root;
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