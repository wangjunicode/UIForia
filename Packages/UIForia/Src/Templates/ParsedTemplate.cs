using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Compilers.ExpressionResolvers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing.Expression;
using UIForia.Rendering;

namespace UIForia.Templates {

    /// <summary>
    /// This represents the result of a parsed UITemplate. Invoking 'Create()' will create an instance of the
    /// template that was parsed. 
    /// </summary>
    public class ParsedTemplate {

        private bool isCompiled;
        
        internal Dictionary<string, UIStyleGroupContainer> sharedStyleMap;
        internal Dictionary<string, UIStyleGroupContainer> implicitStyleMap;

        private readonly List<string> usings;
        private readonly List<UISlotContentTemplate> inheritedContent;
        private readonly List<StyleDefinition> styleDefinitions;
        public readonly UIElementTemplate rootElementTemplate;
        public readonly ExpressionCompiler compiler; // todo -- static?
        public readonly ParsedTemplate baseTemplate;
        public readonly Application app;
        public readonly string templatePath;

        public ParsedTemplate(Application app, Type type, string templatePath, List<UITemplate> contents, List<AttributeDefinition> attributes, List<string> usings, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> imports) : this(null, type, usings, null, styleDefinitions, imports) {
            this.app = app;
            this.templatePath = templatePath;
            this.rootElementTemplate = new UIElementTemplate(app, type, contents, attributes);
        }

        public ParsedTemplate(ParsedTemplate baseTemplate, Type type, List<string> usings, List<UISlotContentTemplate> contentTemplates, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> imports) {
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

            CompileStyles();

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

        private void CompileStyles() {
            if (styleDefinitions == null || styleDefinitions.Count == 0) {
                return;
            }

            sharedStyleMap = new Dictionary<string, UIStyleGroupContainer>();

            for (int i = 0; i < styleDefinitions.Count; i++) {
                StyleSheet sheet = null;
                if (styleDefinitions[i].body != null) {
                    sheet = app.styleImporter.ImportStyleSheetFromString(templatePath, styleDefinitions[i].body);
                }
                else if (styleDefinitions[i].importPath != null) {
                    sheet = app.styleImporter.ImportStyleSheetFromFile(styleDefinitions[i].importPath);
                }

                if (sheet != null) {
                    string alias = styleDefinitions[i].alias;

                    for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                        UIStyleGroupContainer container = sheet.styleGroupContainers[j];

                        if (container.styleType == StyleType.Implicit) {
                            // we only take the first implicit style. This could be improved by doing a merge of some sort
                            implicitStyleMap = implicitStyleMap ?? new Dictionary<string, UIStyleGroupContainer>();
                            if (!implicitStyleMap.ContainsKey(container.name)) {
                                implicitStyleMap.Add(container.name, container);
                            }
                            continue;
                        }
                        
                        if (alias == null) {
                            sharedStyleMap.Add(container.name, container);
                        }
                        else {
                            sharedStyleMap.Add(alias + "." + container.name, container);
                        }
                    }
                }
            }
        }

        private void CompileStep(UITemplate template) {
            template.SourceTemplate = this;
            template.Compile(this);

            if (template.childTemplates != null) {
                for (int i = 0; i < template.childTemplates.Count; i++) {
                    CompileStep(template.childTemplates[i]);
                }
            }

            template.PostCompile(this);
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

        public ParsedTemplate CreateInherited(Type inheritedType, List<string> usings, List<UISlotContentTemplate> contents, List<StyleDefinition> styleDefinitions, List<ImportDeclaration> importDeclarations) {
            return new ParsedTemplate(this, inheritedType, usings, contents, styleDefinitions, importDeclarations);
        }

        internal UIStyleGroupContainer GetImplicitStyle(string tagName) {
            if (implicitStyleMap == null) return null;
            implicitStyleMap.TryGetValue(tagName, out UIStyleGroupContainer retn);
            return retn;
        }
        
        internal UIStyleGroupContainer GetSharedStyle(string styleName) {
            if (sharedStyleMap == null) return null;
            sharedStyleMap.TryGetValue(styleName, out UIStyleGroupContainer retn);
            return retn;
        }
       
    }

}