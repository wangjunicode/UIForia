using System;
using System.Collections.Generic;
using System.Linq;
using Src.Compilers.AliasSources;
using Src.Parsing.StyleParser;
using Src.Rendering;
using Src.Util;

namespace Src {

    public class ParsedTemplate {

        private static readonly List<MetaData> EmptyElementList = new List<MetaData>(0);

        public List<ImportDeclaration> imports;
        public readonly string templateId;
        public readonly ExpressionCompiler compiler;
        public readonly ContextDefinition contextDefinition;

        public readonly UIElementTemplate rootElementTemplate;

        private static int idGenerator;
        private bool isCompiled;
        private List<StyleDefinition> styleGroups;
        
        public ParsedTemplate(UIElementTemplate rootElement, string filePath = null) {
            if (filePath != null) {
                templateId = filePath;
            }
            else {
                templateId = "Template" + (++idGenerator);
            }
            this.rootElementTemplate = rootElement;
            this.styleGroups = new List<StyleDefinition>();
            this.contextDefinition = new ContextDefinition(rootElement.RootType);
            this.compiler = new ExpressionCompiler(contextDefinition);
        }

        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;

        // todo -- ensure we have a <Children/> tag somewhere in our template if there are input children
        
        public MetaData CreateWithScope(TemplateScope scope) {
            if (!isCompiled) Compile();

            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);

            MetaData instanceData = rootElementTemplate.GetCreationData(instance, scope.context);
            instanceData.element.templateChildren = ArrayPool<UIElement>.Empty;

            // todo -- ensure only one <Children/> element
            for (int i = 0; i < rootElementTemplate.childTemplates.Count; i++) {
                UITemplate template = rootElementTemplate.childTemplates[i];
                if (template is UIChildrenTemplate) {
                    if (scope.inputChildren != null) {
                        instanceData.element.templateChildren = new UIElement[scope.inputChildren.Count];
                        for (int j = 0; j < scope.inputChildren.Count; j++) {
                            instanceData.AddChild(scope.inputChildren[j]);
                            instanceData.element.templateChildren[j] = scope.inputChildren[j].element;
                        }
                    }

                    scope.inputChildren = null;
                }
                else {
                    instanceData.AddChild(template.CreateScoped(scope));
                }
            }
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
                    element.templateChildren[i].templateParent = element;
                    AssignContext(element.templateChildren[i], context);
                }
            }
        }

        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            for (int i = 0; i < imports.Count; i++) {
                Type type = TypeProcessor.GetRuntimeType(imports[i].path);
                if(type == null) throw new Exception("Could not find type for: " + imports[i].path);
                imports[i].type = type;
                contextDefinition.AddConstAliasSource(new ExternalReferenceAliasSource(imports[i].alias, type));
            }

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

        public UIBaseStyleGroup ResolveStyleGroup(string styleName) {
            StyleDefinition def;
            // if no dot in path then the style name is the alias
            if (styleName.IndexOf('.') == -1) {
                def = GetStyleDefinitionFromAlias(StyleDefinition.k_EmptyAliasName);
                return StyleParser.GetParsedStyle(def.importPath, def.body, styleName);
            }

            string[] path = styleName.Split('.');
            if (path.Length != 2) {
                throw new Exception("Invalid style path: " + path);
            }

            def = GetStyleDefinitionFromAlias(path[0]);
            return StyleParser.GetParsedStyle(def.importPath, null, path[0]);
        }

        public static int MakeId() {
            return idGenerator++;
        }

        public void SetStyleGroups(List<StyleDefinition> styleDefinitions) {
            styleGroups = styleDefinitions;
            for (int i = 0; i < styleGroups.Count; i++) {
                StyleDefinition current = styleGroups[i];
                for (int j = 0; j < styleGroups.Count; j++) {
                    if (j == i) {
                        continue;
                    }
                    if (styleGroups[j].alias == current.alias) {
                        if (current.alias == StyleDefinition.k_EmptyAliasName) {
                            throw new Exception("You cannot provide multiple style tags with a default alias");
                        }
                        throw new Exception("Duplicate style alias: " + current.alias);
                    }    
                }
            }
        }

        private StyleDefinition GetStyleDefinitionFromAlias(string alias) {
            for (int i = 0; i < styleGroups.Count; i++) {
                if (styleGroups[i].alias == alias) {
                    return styleGroups[i];
                }
            }

            if (alias == StyleDefinition.k_EmptyAliasName) {
                throw new UIForia.ParseException("Unable to find a default style group");   
            }
            
            throw new UIForia.ParseException("Unable to find a style with the alias: " + alias);
        }
        
    }

}