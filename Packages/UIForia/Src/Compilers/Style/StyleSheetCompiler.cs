using System;
using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleSheetCompiler {

        private StyleSheetImporter styleSheetImporter;

        private List<string> currentlyResolvingConstants = new List<string>();

        private StyleCompileContext context;

        public StyleSheetCompiler(StyleSheetImporter styleSheetImporter) {
            this.styleSheetImporter = styleSheetImporter;
        }

        public StyleSheet Compile(LightList<StyleASTNode> rootNodes) {
            context = new StyleSheetConstantImporter(styleSheetImporter).CreateContext(rootNodes);

            StyleSheet styleSheet = new StyleSheet(context.constants, LightListPool<UIStyleGroup>.Get());


            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case StyleRootNode styleRoot:
                        styleSheet.styleGroups.AddRange(CompileStyleGroup(styleRoot));
                        break;
                }
            }

            context.Release();

            return styleSheet;
        }

        private LightList<UIStyleGroup> CompileStyleGroup(StyleRootNode styleRoot) {
            UIStyleGroup defaultGroup = new UIStyleGroup();
            defaultGroup.name = styleRoot.identifier ?? styleRoot.tagName;
            LightList<UIStyleGroup> result = LightListPool<UIStyleGroup>.Get();
            foreach (var node in styleRoot.children) {
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set

                        StylePropertyMappers.MapProperty(defaultGroup.normal, propertyNode.propertyName, propertyNode.children);

                        break;
                    case AttributeGroupContainer attribute:
                        foreach (var uiStyleGroup in result) {
                            // is attribute name already in here?
                        }

                        break;
                    case StyleStateContainer styleContainer:
                        break;

                    default:
                        throw new ParseException($"You cannot have a {node} at this level.");
                }
            }

            return result;
        }

    }
}
