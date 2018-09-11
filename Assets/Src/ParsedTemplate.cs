using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;

namespace Src {

    public class ParsedTemplate {

        private static readonly List<InitData> EmptyElementList = new List<InitData>(0);
        
        public string filePath;
        public List<UIStyle> styles;
        public List<ImportDeclaration> imports;

        public readonly ExpressionCompiler compiler;
        public readonly ContextDefinition contextDefinition;
        
        public readonly UIElementTemplate rootElementTemplate;
        
        private int idGenerator;
        private bool isCompiled;

        public ParsedTemplate(UIElementTemplate rootElement) {
            this.rootElementTemplate = rootElement;
            this.contextDefinition = new ContextDefinition(rootElement.RootType);
            this.compiler = new ExpressionCompiler(contextDefinition);
        }

        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;

        public InitData CreateWithScope(TemplateScope scope) {
            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);

            InitData instanceData = rootElementTemplate.GetCreationData(instance, scope.context);

            for (int i = 0; i < rootElementTemplate.childTemplates.Count; i++) {
                UITemplate template = rootElementTemplate.childTemplates[i];
                if (template is UIChildrenTemplate) {
                    for (int j = 0; j < scope.inputChildren.Count; j++) {
                        instanceData.AddChild(scope.inputChildren[j]);
                    }
                }
                else {
                    instanceData.AddChild(template.CreateScoped(scope));
                }
            }
            
            instanceData.element.templateChildren = scope.inputChildren.Select(c => c.element).ToArray();
            instanceData.element.ownChildren = instanceData.children.Select(c => c.element).ToArray();
            
            AssignContext(instance, scope.context);

            return instanceData;
        }

        public InitData CreateWithoutScope(UIView view) {
            if (!isCompiled) Compile();

            TemplateScope scope = new TemplateScope();
            scope.context = new UITemplateContext(view);
            scope.inputChildren = EmptyElementList;

            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);
            scope.context.rootElement = instance;

            InitData rootData = rootElementTemplate.GetCreationData(instance, scope.context);

            for (int i = 0; i < childTemplates.Count; i++) {
                rootData.AddChild(childTemplates[i].CreateScoped(scope));
            }

            rootData.element.templateChildren = rootData.children.Select(c => c.element).ToArray();
            rootData.element.ownChildren = rootData.element.templateChildren;

            AssignContext(instance, scope.context);
            
            return rootData;
        }

        private void AssignContext(UIElement element, UITemplateContext context) {
            element.templateContext = context;
            for (int i = 0; i < element.templateChildren.Length; i++) {
                AssignContext(element.templateChildren[i], context);
            }
        }
        
        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            CompileStep(rootElementTemplate);
        }

        private void CompileStep(UITemplate template) {
            template.Compile(this);
            if (template.childTemplates != null) {
                for (int i = 0; i < template.childTemplates.Count; i++) {
                    CompileStep(template.childTemplates[i]);
                }
            }

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

        public int MakeId() {
            return idGenerator++;
        }

    }

}