using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Parsing.StyleParser;
using UIForia.Rendering;

namespace UIForia {

    /// <summary>
    /// This represents the result of a parsed UITemplate. Invoking 'Create()' will create an instance of the
    /// template that was parsed. 
    /// </summary>
    public class ParsedTemplate {

        private static int s_TemplateIdGenerator;
        private static readonly IntMap<ParsedTemplate> s_ParsedTemplates; // todo -- is this used?

        public readonly int templateId;

        private bool isCompiled;

        private readonly List<string> usings;
        private readonly List<UISlotContentTemplate> inheritedContent;
        private readonly List<StyleDefinition> styleGroups;
        public readonly UIElementTemplate rootElementTemplate;
        public readonly ExpressionCompiler compiler; // todo -- static?
        public ParsedTemplate baseTemplate;
        
        static ParsedTemplate() {
            s_ParsedTemplates = new IntMap<ParsedTemplate>();
        }
        
        public ParsedTemplate(Type type, List<UITemplate> contents, List<AttributeDefinition> attributes, List<string> usings, List<StyleDefinition> styleGroups, List<ImportDeclaration> imports) {
            this.RootType = type;
            this.templateId = ++s_TemplateIdGenerator;
            this.rootElementTemplate = new UIElementTemplate(type, contents, attributes);
            this.usings = usings;
            this.styleGroups = styleGroups;
            this.Imports = imports;
            this.compiler = new ExpressionCompiler();
            s_ParsedTemplates[templateId] = this;
            ValidateStyleGroups();
        }

        public ParsedTemplate(ParsedTemplate baseTemplate, Type type,  List<string> usings, List<UISlotContentTemplate> contentTemplates, List<StyleDefinition> styleGroups, List<ImportDeclaration> imports) {
            this.templateId = ++s_TemplateIdGenerator;
            this.baseTemplate = baseTemplate;
            this.RootType = type;
            this.rootElementTemplate = null;
            this.usings = usings;
            this.inheritedContent = contentTemplates;
            this.styleGroups = styleGroups;
            this.Imports = imports;
            this.compiler = new ExpressionCompiler();
            s_ParsedTemplates[templateId] = this;
            ValidateStyleGroups();
        }

        public static void Reset() {
            s_ParsedTemplates.Clear();    
        }
        
        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;
        
        public Type RootType { get; }      
        public List<ImportDeclaration> Imports { get; }


        public UIElement Create() {
            Compile();
            if (baseTemplate == null) {
                return rootElementTemplate.CreateUnscoped();
            }
            else {
                return baseTemplate.rootElementTemplate.CreateUnscoped(RootType, inheritedContent);
            }
        }
        
        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            // todo -- remove allocations
            
            compiler.AddNamespaces(usings);
            compiler.AddNamespace("UIForia.Rendering");
            compiler.AddNamespace("UIForia");
            
            compiler.AddAliasResolver(new ElementResolver("element"));
            compiler.AddAliasResolver(new ParentElementResolver("parent"));
            compiler.AddAliasResolver(new RouteResolver("route"));
            compiler.AddAliasResolver(new RouteParameterResolver("$routeParams"));
            compiler.AddAliasResolver(new ContentSizeResolver());
            compiler.AddAliasResolver(new UrlResolver("$url"));
            compiler.AddAliasResolver(new ColorResolver("$rgb"));
            compiler.AddAliasResolver(new SizeResolver("$size"));
            compiler.AddAliasResolver(new LengthResolver("$fixedLength"));
            compiler.AddAliasResolver(new MethodResolver("$px", typeof(StyleBindingCompiler).GetMethod(nameof(StyleBindingCompiler.PixelLength), new[] {typeof(float)})));
            
            if (baseTemplate != null) {
                baseTemplate.Compile();
                for (int i = 0; i < inheritedContent.Count; i++) {
                    CompileStep(inheritedContent[i]);
                }

                return;
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
            if (styleGroups == null) {
                return null;
            }
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

        private void ValidateStyleGroups() {
            if (styleGroups == null) return;
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
                throw new ParseException("Unable to find a default style group");   
            }
            
            throw new ParseException("Unable to find a style with the alias: " + alias);
        }

        public ParsedTemplate CreateInherited(Type inheritedType, List<string> usings, List<UISlotContentTemplate> contents, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> importDeclarations) {
            return new ParsedTemplate(this, inheritedType, usings, contents, styleDefinitions, importDeclarations);
        }

    }

}