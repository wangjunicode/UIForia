using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Parsing.StyleParser;
using UIForia.Rendering;

namespace UIForia {

    public class ParsedTemplate {

        private static int s_TemplateIdGenerator;
        private static readonly IntMap<ParsedTemplate> s_ParsedTemplates;

        public readonly int templateId;
        public readonly string templatePath;

        private bool isCompiled;

        public List<string> usings;
        public List<StyleDefinition> styleGroups;
        public List<ImportDeclaration> imports;
        
        public UIElementTemplate rootElementTemplate;
        public readonly ExpressionCompiler compiler; // todo -- static?

        static ParsedTemplate() {
            s_ParsedTemplates = new IntMap<ParsedTemplate>();
        }
        
        public ParsedTemplate(UIElementTemplate rootElement, string filePath = null) {
            this.templateId = ++s_TemplateIdGenerator;
            this.templatePath = filePath ?? "Template" + templateId;
            this.rootElementTemplate = rootElement;
            this.styleGroups = new List<StyleDefinition>();
            this.compiler = new ExpressionCompiler();
            s_ParsedTemplates[templateId] = this;
        }

        public static void Reset() {
            s_ParsedTemplates.Clear();    
        }
        
        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;
        public Type RootType => rootElementTemplate.RootType;

        public UIElement Create() {
            Compile();
            return rootElementTemplate.CreateUnscoped();
        }
        
        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            compiler.AddNamespaces(usings);
            compiler.AddAliasResolver(new ElementResolver("element"));
            compiler.AddAliasResolver(new ParentElementResolver("parent"));
            compiler.AddAliasResolver(new RouteResolver("route"));
            compiler.AddAliasResolver(new RouteParameterResolver("$routeParams"));
            
            for (int i = 0; i < imports.Count; i++) {
                Type type = TypeProcessor.GetRuntimeType(imports[i].path);
                
                if (type == null) {
                    throw new Exception("Could not find type for: " + imports[i].path);
                }
                
                imports[i].type = type;
                throw new NotImplementedException();
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

            template.PostCompile(this);
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
            return StyleParser.GetParsedStyle(def.importPath, null, path[1]);
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

        public ParsedTemplate Clone() {
            ParsedTemplate retn = new ParsedTemplate(rootElementTemplate, templatePath);
            imports.CopyToList(retn.imports);
            styleGroups.CopyToList(retn.styleGroups);
            usings.CopyToList(retn.usings);
            return retn;
        }

    }

}