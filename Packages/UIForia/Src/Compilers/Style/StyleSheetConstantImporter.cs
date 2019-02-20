using System;
using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    /// <summary>
    /// Creates a StyleCompileContext with all imported consts.
    /// </summary>
    public class StyleSheetConstantImporter {

        private static readonly Func<StyleConstant, string, bool> s_FindStyleConstant = (element, name) => element.name == name;

        private StyleSheetImporter styleSheetImporter;

        private List<string> currentlyResolvingConstants = new List<string>();

        public StyleSheetConstantImporter(StyleSheetImporter styleSheetImporter) {
            this.styleSheetImporter = styleSheetImporter;
        }

        public StyleCompileContext CreateContext(LightList<StyleASTNode> rootNodes) {
            StyleCompileContext context = new StyleCompileContext();

            // first all imports must be collected as they can be referenced in exports and consts
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case ImportNode importNode:
                        StyleSheet importedStyle = styleSheetImporter.importStyleSheetFromFile(importNode.source);

                        LightList<StyleConstant> importedStyleConstants = LightListPool<StyleConstant>.Get();
                        context.importedStyleConstants.Add(importNode.alias, importedStyleConstants);

                        for (int constantIndex = 0; constantIndex < importedStyle.constants.Count; constantIndex++) {
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
            foreach (StyleConstant c in context.constants) {
                if (c.name == constant.name) {
                    return c;
                }
            }

            if (constant.referenceNode.children.Count > 0) {
                if (context.importedStyleConstants.ContainsKey(constant.referenceNode.referenceName)) {
                    DotAccessNode importedConstant = (DotAccessNode) constant.referenceNode.children[0];

                    StyleConstant importedStyleConstant = context.importedStyleConstants[constant.referenceNode.referenceName]
                        .Find(importedConstant.propertyName, s_FindStyleConstant);

                    if (importedStyleConstant.name == null) {
                        throw new CompileException(importedConstant, "Could not find referenced property in imported scope.");
                    }
                }

                throw new CompileException(constant.referenceNode,
                    "Constants cannot reference members of other constants.");
            }

            StyleConstant referencedConstant = ResolveReference(context, constant.referenceNode);

            StyleConstant styleConstant = new StyleConstant {
                name = constant.name,
                value = referencedConstant.value,
                exported = constant.exported
            };

            context.constants.Add(styleConstant);
            return styleConstant;
        }

        private StyleConstant ResolveReference(StyleCompileContext context, ReferenceNode reference) {
            if (currentlyResolvingConstants.Contains(reference.referenceName)) {
                throw new CompileException(reference, "Circular dependency detected!");
            }

            foreach (var constant in context.constants) {
                if (constant.name == reference.referenceName) {
                    // reference resolved
                    return constant;
                }
            }

            // now we have to recursively resolve the reference of the reference:
            // const x: string = @y; // we're here...
            // const y: string = @z: // ....referencing this
            // const z: string = "whatup"; // ...which will resolve to this.
            if (context.constantsWithReferences.ContainsKey(reference.referenceName)) {
                currentlyResolvingConstants.Add(reference.referenceName);
                StyleConstant resolvedConstant = Resolve(context, context.constantsWithReferences[reference.referenceName]);
                currentlyResolvingConstants.Remove(reference.referenceName);
                return resolvedConstant;
            }

            throw new CompileException(reference, $"Could not resolve reference {reference}. "
             + "Known references are " + context.PrintConstants());
        }

        private void TransformConstNode(StyleCompileContext context, ConstNode constNode, bool exported) {
            if (constNode.value is ReferenceNode) {
                context.constantsWithReferences.Add(constNode.constName, new StyleConstant {
                    name = constNode.constName,
                    referenceNode = (ReferenceNode) constNode.value,
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
