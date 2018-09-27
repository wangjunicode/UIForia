using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Rendering;

namespace Src {

    public class ParsedTemplate {

        private static readonly List<MetaData> EmptyElementList = new List<MetaData>(0);

        public string filePath;
        public List<UIStyle> styles;
        public List<ImportDeclaration> imports;
        public List<StyleDefinition> styleGroups;
        public readonly ExpressionCompiler compiler;
        public readonly ContextDefinition contextDefinition;

        public readonly UIElementTemplate rootElementTemplate;

        private int idGenerator;
        private bool isCompiled;

        public ParsedTemplate(UIElementTemplate rootElement, List<StyleDefinition> styleDefinitions = null) {
            this.rootElementTemplate = rootElement;
            this.styleGroups = new List<StyleDefinition>();
            this.contextDefinition = new ContextDefinition(rootElement.RootType);
            this.compiler = new ExpressionCompiler(contextDefinition);
            if (styleDefinitions != null) {
                for (int i = 0; i < styleDefinitions.Count; i++) {
                    AddStyleDefinition(styleDefinitions[i]);
                }
            }
        }

        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;

        public MetaData CreateWithScope(TemplateScope scope) {
            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);

            MetaData instanceData = rootElementTemplate.GetCreationData(instance, scope.context);

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

        public MetaData CreateWithoutScope(UIView view) {
            if (!isCompiled) Compile();

            TemplateScope scope = new TemplateScope();
            scope.context = new UITemplateContext(view);
            scope.inputChildren = EmptyElementList;

            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);
            scope.context.rootElement = instance;

            MetaData rootData = rootElementTemplate.GetCreationData(instance, scope.context);

            for (int i = 0; i < childTemplates.Count; i++) {
                rootData.AddChild(childTemplates[i].CreateScoped(scope));
            }

            rootData.element.templateChildren = rootData.children.Select(c => c.element).ToArray();
            rootData.element.ownChildren = rootData.element.templateChildren;

            AssignContext(instance, scope.context);

            return rootData;
        }

        private static void AssignContext(UIElement element, UITemplateContext context) {
            element.templateContext = context;
            if (element.templateChildren != null) {
                for (int i = 0; i < element.templateChildren.Length; i++) {
                    AssignContext(element.templateChildren[i], context);
                }
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

        public UIBaseStyleGroup GetStyleGroupInstance(string styleName) {
            if (styleName.IndexOf('.') == -1) {
                return StyleGroupProcessor.ResolveStyle(FindStyleGroupClassPath(StyleDefinition.k_EmptyAliasName), styleName);
            }

            string[] path = styleName.Split('.');
            if (path.Length != 2) {
                throw new Exception("Invalid style path: " + path);
            }

            return StyleGroupProcessor.ResolveStyle(FindStyleGroupClassPath(path[0]), path[1]);
        }

        public int MakeId() {
            return idGenerator++;
        }

        public void AddStyleDefinition(StyleDefinition styleDefinition) {
            for (int i = 0; i < styleGroups.Count; i++) {
                if (styleGroups[i].alias == styleDefinition.alias) {
                    throw new Exception("Duplicate style alias: " + styleDefinition.alias);
                }
            }

            styleGroups.Add(styleDefinition);
        }

        private string FindStyleGroupClassPath(string alias) {
            for (int i = 0; i < styleGroups.Count; i++) {
                if (styleGroups[i].alias == alias) {
                    return styleGroups[i].classPath;
                }
            }

            return null;
        }

    }

}