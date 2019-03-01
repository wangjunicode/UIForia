using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expression;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Templates {

    /// <summary>
    /// This represents the result of a parsed UITemplate. Invoking 'Create()' will create an instance of the
    /// template that was parsed. 
    /// </summary>
    public class ParsedTemplate {

        private static int s_TemplateIdGenerator;

        public readonly int templateId;

        private bool isCompiled;

        private LightList<UIStyleGroupContainer> styleContainers;

        private readonly List<string> usings;
        private readonly List<UISlotContentTemplate> inheritedContent;
        private readonly List<StyleDefinition> styleDefinitions;
        public readonly UIElementTemplate rootElementTemplate;
        public readonly ExpressionCompiler compiler; // todo -- static?
        public ParsedTemplate baseTemplate;
        public Application app;
        


        public ParsedTemplate(Application app, Type type, List<UITemplate> contents, List<AttributeDefinition> attributes, List<string> usings, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> imports) : this(null, type, usings, null, styleDefinitions, imports) {
            this.app = app;
            this.rootElementTemplate = new UIElementTemplate(app, type, contents, attributes);
        }

        public ParsedTemplate(ParsedTemplate baseTemplate, Type type, List<string> usings, List<UISlotContentTemplate> contentTemplates, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> imports) {
            this.templateId = ++s_TemplateIdGenerator;
            this.baseTemplate = baseTemplate;
            this.RootType = type;
            this.rootElementTemplate = null;
            this.usings = usings;
            this.inheritedContent = contentTemplates;
            this.styleDefinitions = styleDefinitions;
            this.Imports = imports;
            this.compiler = new ExpressionCompiler();
            ValidateStyleDefinitions();
        }

        public List<UITemplate> childTemplates => rootElementTemplate.childTemplates;

        public Type RootType { get; }
        public List<ImportDeclaration> Imports { get; }

        public UIElement Create() {
            Compile();
            if (baseTemplate == null) {
                return rootElementTemplate.CreateUnscoped();
            }

            return baseTemplate.rootElementTemplate.CreateUnscoped(RootType, inheritedContent);
        }

        public void Compile() {
            if (isCompiled) return;
            isCompiled = true;
            // todo -- remove allocations

            compiler.AddNamespaces(usings);
            compiler.AddNamespace("UIForia.Rendering");
            compiler.AddNamespace("UIForia.Layout");
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

        public bool TryResolveStyleGroup(string styleName, out UIStyleGroupContainer container) {
            if (styleDefinitions == null) {
                container = null;
                return false;
            }

            StyleDefinition def;
            // if no dot in path then the style name is the alias
            if (styleName.IndexOf('.') == -1) {
                def = GetStyleDefinitionFromAlias(StyleDefinition.k_EmptyAliasName);
                container = app.styleImporter.GetStyleGroupByStyleName(def.importPath, def.body, styleName);
                return container != null;
            }

            string[] path = styleName.Split('.');
            if (path.Length != 2) {
                throw new Exception("Invalid style path: " + path);
            }

            def = GetStyleDefinitionFromAlias(path[0]);
            container = app.styleImporter.GetStyleGroupByStyleName(def.importPath, def.body, styleName);
            return container != null;
        }

        public UIStyleGroupContainer ResolveStyleGroup(string styleName) {
            if (styleDefinitions == null) {
                return null;
            }

            StyleDefinition def;
            // if no dot in path then the style name is the alias
            if (styleName.IndexOf('.') == -1) {
                def = GetStyleDefinitionFromAlias(StyleDefinition.k_EmptyAliasName);
                return app.styleImporter.GetStyleGroupByStyleName(def.importPath, def.body, styleName);
            }

            string[] path = styleName.Split('.');
            if (path.Length != 2) {
                throw new Exception("Invalid style path: " + path);
            }

            def = GetStyleDefinitionFromAlias(path[0]);
            return app.styleImporter.GetStyleGroupByStyleName(def.importPath, def.body, path[1]);
        }

        private void ValidateStyleDefinitions() {
            if (styleDefinitions == null) return;
            for (int i = 0; i < styleDefinitions.Count; i++) {
                StyleDefinition current = styleDefinitions[i];
                for (int j = 0; j < styleDefinitions.Count; j++) {
                    if (j == i) {
                        continue;
                    }

                    if (styleDefinitions[j].alias == current.alias) {
                        if (current.alias == StyleDefinition.k_EmptyAliasName) {
                            throw new Exception("You cannot provide multiple style tags with a default alias");
                        }

                        throw new Exception("Duplicate style alias: " + current.alias);
                    }
                }
            }
        }

        private StyleDefinition GetStyleDefinitionFromAlias(string alias) {
            for (int i = 0; i < styleDefinitions.Count; i++) {
                if (styleDefinitions[i].alias == alias) {
                    return styleDefinitions[i];
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

        internal UIStyleGroupContainer ResolveElementStyle(string tagName) {
            if (styleDefinitions == null) {
                return default;
            }

            if (styleContainers == null) {
                styleContainers = new LightList<UIStyleGroupContainer>();
            }

            for (int i = 0; i < styleContainers.Count; i++) {
                if (styleContainers[i].styleType == StyleType.Implicit && styleContainers[i].name == tagName) {
                    return styleContainers[i];
                }
            }

            LightList<UIStyleGroup> groups = new LightList<UIStyleGroup>();

            // if no dot in path then the style name is the alias
            for (int i = 0; i < styleDefinitions.Count; i++) {
                StyleDefinition def = styleDefinitions[i];
                UIStyleGroupContainer container = app.styleImporter.GetStyleGroupsByTagName(def.importPath, def.body, tagName);
                if (container != null && container.styleType == StyleType.Implicit) {
                    groups.AddRange(container.groups);
                }
            }

            UIStyleGroupContainer containerResult = new UIStyleGroupContainer(tagName, StyleType.Implicit, groups);
            styleContainers.Add(containerResult);

            return containerResult;
        }

    }

}
