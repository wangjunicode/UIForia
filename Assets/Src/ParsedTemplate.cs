using System;
using System.Collections.Generic;
using System.Linq;
using Src.Compilers.AliasSources;
using Src.Parsing.StyleParser;
using Src.Rendering;
using Src.Util;

namespace Src {

    public class ParsedTemplate {

        private static readonly IntMap<ParsedTemplate> s_ParsedTemplates = new IntMap<ParsedTemplate>();
        private static readonly IReadOnlyList<UIElement> EmptyElementList = ListPool<UIElement>.Empty;

        public List<ImportDeclaration> imports;
        public readonly string templatePath;
        public readonly int templateId;

        private static int s_TemplateIdGenerator;
        
        public readonly ExpressionCompiler compiler;
        public readonly ContextDefinition contextDefinition;

        public readonly UIElementTemplate rootElementTemplate;

        private bool isCompiled;
        private List<StyleDefinition> styleGroups;

        private readonly IntMap<UITemplate> m_TemplateMap;
        
        public ParsedTemplate(UIElementTemplate rootElement, string filePath = null) {
            templateId = ++s_TemplateIdGenerator;
            templatePath = filePath ?? "Template" + templateId;
            this.rootElementTemplate = rootElement;
            this.styleGroups = new List<StyleDefinition>();
            this.contextDefinition = new ContextDefinition(rootElement.RootType);
            this.compiler = new ExpressionCompiler(contextDefinition);
            this.m_TemplateMap = new IntMap<UITemplate>();
            s_ParsedTemplates[templateId] = this;
        }

        public static void Reset() {
            s_ParsedTemplates.Clear();    
        }
        
        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;
        
        public UIElement Create(UIView view) {
            Compile();
            return rootElementTemplate.CreateUnscoped(view);
//            
//            UITemplateContext context = new UITemplateContext(view);
//            UIElement instance = (UIElement) Activator.CreateInstance(rootElementTemplate.RootType);
//            TemplateScope scope = new TemplateScope(instance, context);
//
//            scope.context.rootElement = instance;
//            instance.flags |= UIElementFlags.TemplateRoot;
//            
//
//            for (int i = 0; i < childTemplates.Count; i++) {
//                rootData.AddChild(childTemplates[i].CreateScoped(scope));
//            }
//
//            rootData.element.templateChildren = rootData.children.Select(c => c.element).ToArray();
//            rootData.element.ownChildren = rootData.element.templateChildren;
//
//            AssignContext(rootElementTemplate, instance, scope.context);
//
//            return rootData;
        }

//        private void AssignContext(UITemplate template, UIElement element, UITemplateContext context) {
//            element.templateContext = context;
//            if (element.templateChildren != null) {
//                for (int i = 0; i < element.templateChildren.Length; i++) {
//                    element.templateChildren[i].templateParent = element;
//                    AssignContext(template.childTemplates[i], element.templateChildren[i], context);
//                }
//            }
//        }

        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            for (int i = 0; i < imports.Count; i++) {
                Type type = TypeProcessor.GetRuntimeType(imports[i].path);
                
                if (type == null) {
                    throw new Exception("Could not find type for: " + imports[i].path);
                }
                
                imports[i].type = type;
                contextDefinition.AddConstAliasSource(new ExternalReferenceAliasSource(imports[i].alias, type));
            }

            CompileStep(rootElementTemplate);
        }

        private void CompileStep(UITemplate template) {
            m_TemplateMap[template.id] = template;
            template.Compile(this);
            if (template.childTemplates != null) {
                for (int i = 0; i < template.childTemplates.Count; i++) {
                    CompileStep(template.childTemplates[i]);
                }
            }
        }

        public UIStyleGroup ResolveStyleGroup(string styleName) {
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