using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleSheetCompiler {

        private StyleSheetImporter styleSheetImporter;

        private StyleCompileContext context;

        public static StyleSheetCompiler New() {
            return new StyleSheetCompiler(new StyleSheetImporter());
        }

        public StyleSheetCompiler(StyleSheetImporter styleSheetImporter) {
            this.styleSheetImporter = styleSheetImporter;
        }

        public StyleSheet Compile(LightList<StyleASTNode> rootNodes) {
            context = new StyleSheetConstantImporter(styleSheetImporter).CreateContext(rootNodes);

            // todo add imported style groups
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
                        defaultGroup.normal = new UIStyle();
                        StylePropertyMappers.MapProperty(defaultGroup.normal, propertyNode, context);

                        
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
            
            result.Add(defaultGroup);

            return result;
        }

    }
}
