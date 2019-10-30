using System;
using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    /// <summary>
    /// Creates a StyleCompileContext with all imported consts.
    /// </summary>
    public class StyleSheetConstantImporter {

        private static readonly Func<StyleConstant, string, bool> s_FindStyleConstant = (element, name) => element.name == name;

        private readonly StyleSheetImporter styleSheetImporter;
        private readonly List<string> currentlyResolvingConstants;

        public StyleSheetConstantImporter(StyleSheetImporter styleSheetImporter) {
            this.styleSheetImporter = styleSheetImporter;
            this.currentlyResolvingConstants = new List<string>();
        }
        
        public StyleCompileContext CreateContext(LightList<StyleASTNode> rootNodes) {
            
            StyleCompileContext context = new StyleCompileContext(); //todo -- this needs to be pooled
            
            // first all imports must be collected as they can be referenced in exports and consts
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case ImportNode importNode:
                        StyleSheet importedStyle = styleSheetImporter.ImportStyleSheetFromFile(importNode.source);

                        LightList<StyleConstant> importedStyleConstants = LightList<StyleConstant>.Get();
                        context.importedStyleConstants.Add(importNode.alias, importedStyleConstants);

                        for (int constantIndex = 0; constantIndex < importedStyle.constants.Length; constantIndex++) {
                            StyleConstant importedStyleConstant = importedStyle.constants[constantIndex];
                            if (importedStyleConstant.exported) {
                                importedStyleConstants.Add(importedStyleConstant);
                            }
                        }

                        break;
                }
            }

            // collect all constants that could be referenced
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case ExportNode exportNode:
                        TransformConstNode(context, exportNode.constNode, true);
                        break;
                    case ConstNode constNode:
                        TransformConstNode(context, constNode, false);
                        break;
                }
            }

            ResolveConstantReferences(context);
            context.constantsWithReferences.Clear();

            return context;
        }

        private void ResolveConstantReferences(StyleCompileContext context) {
            foreach (StyleConstant constant in context.constantsWithReferences.Values) {
                Resolve(context, constant);
            }
        }

        private StyleConstant Resolve(StyleCompileContext context, StyleConstant constant) {
            // shortcut return for constants that have been resolved already
            for (int index = 0; index < context.constants.Count; index++) {
                StyleConstant c = context.constants[index];
                if (c.name == constant.name) {
                    return c;
                }
            }

            if (constant.constReferenceNode.children.Count > 0) {
                if (context.importedStyleConstants.ContainsKey(constant.constReferenceNode.identifier)) {
                    DotAccessNode importedConstant = (DotAccessNode) constant.constReferenceNode.children[0];

                    StyleConstant importedStyleConstant = context.importedStyleConstants[constant.constReferenceNode.identifier]
                        .Find(importedConstant.propertyName, s_FindStyleConstant);

                    if (importedStyleConstant.name == null) {
                        throw new CompileException(importedConstant, "Could not find referenced property in imported scope.");
                    }
                }

                throw new CompileException(constant.constReferenceNode,
                    "Constants cannot reference members of other constants.");
            }

            StyleConstant referencedConstant = ResolveReference(context, constant.constReferenceNode);

            StyleConstant styleConstant = new StyleConstant {
                name = constant.name,
                value = referencedConstant.value,
                exported = constant.exported
            };

            context.constants.Add(styleConstant);
            return styleConstant;
        }

        private StyleConstant ResolveReference(StyleCompileContext context, ConstReferenceNode constReference) {
            if (currentlyResolvingConstants.Contains(constReference.identifier)) {
                throw new CompileException(constReference, "Circular dependency detected!");
            }

            foreach (var constant in context.constants) {
                if (constant.name == constReference.identifier) {
                    // reference resolved
                    return constant;
                }
            }

            // now we have to recursively resolve the reference of the reference:
            // const x: string = @y; // we're here...
            // const y: string = @z: // ....referencing this
            // const z: string = "whatup"; // ...which will resolve to this.
            if (context.constantsWithReferences.ContainsKey(constReference.identifier)) {
                currentlyResolvingConstants.Add(constReference.identifier);
                StyleConstant resolvedConstant = Resolve(context, context.constantsWithReferences[constReference.identifier]);
                currentlyResolvingConstants.Remove(constReference.identifier);
                return resolvedConstant;
            }

            throw new CompileException(constReference, $"Could not resolve reference {constReference}. "
             + "Known references are: " + context.PrintConstants());
        }

        private void TransformConstNode(StyleCompileContext context, ConstNode constNode, bool exported) {
            if (constNode.value is ConstReferenceNode) {
                context.constantsWithReferences.Add(constNode.constName, new StyleConstant {
                    name = constNode.constName,
                    constReferenceNode = (ConstReferenceNode) constNode.value,
                    exported = exported
                });
            }
            else {
                context.constants.Add(new StyleConstant {
                    name = constNode.constName,
                    value = constNode.value,
                    exported = exported
                });
            }
        }
    }
}
