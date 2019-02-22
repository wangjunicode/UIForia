using System;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleSheetCompiler {

        private StyleSheetImporter styleSheetImporter;

        private StyleCompileContext context;

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
            defaultGroup.normal = new UIStyle();
            defaultGroup.name = styleRoot.identifier ?? styleRoot.tagName;
            defaultGroup.styleType = styleRoot.tagName != null ? StyleType.Implicit : StyleType.Shared;

            return CompileStyleContainer(styleRoot, defaultGroup);
        }

        private LightList<UIStyleGroup> CompileStyleContainer(StyleGroupContainer root, UIStyleGroup defaultGroup) {
            LightList<UIStyleGroup> result = LightListPool<UIStyleGroup>.Get();
            result.Add(defaultGroup);

            for (int index = 0; index < root.children.Count; index++) {
                StyleASTNode node = root.children[index];
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set
                        StylePropertyMappers.MapProperty(defaultGroup.normal, propertyNode, context);
                        break;
                    case AttributeGroupContainer attribute:
                        if (root is AttributeGroupContainer) {
                            throw new CompileException(attribute, "You cannot nest attribute group definitions.");
                        }
                        UIStyleGroup attributeGroup = new UIStyleGroup();            
                        attributeGroup.normal = new UIStyle();
                        attributeGroup.name = root.identifier;
                        attributeGroup.rule = MapAttributeContainerToRule(attribute);
                        result.AddRange(CompileStyleContainer(attribute, attributeGroup));

                        break;
                    case StyleStateContainer styleContainer:
                        if (styleContainer.identifier == "hover") {
                            defaultGroup.hover = GetUIStyleOrDefault(defaultGroup.hover);
                            MapProperties(defaultGroup.hover, styleContainer.children);
                        }
                        else if (styleContainer.identifier == "focus") {
                            defaultGroup.focused = GetUIStyleOrDefault(defaultGroup.focused);
                            MapProperties(defaultGroup.hover, styleContainer.children);
                        }
                        else if (styleContainer.identifier == "active") {
                            defaultGroup.focused = GetUIStyleOrDefault(defaultGroup.active);
                            MapProperties(defaultGroup.active, styleContainer.children);
                        }
                        else throw new CompileException(styleContainer, $"Unknown style state. Please use [hover], [focus] or [active] instead.");

                        break;
                    default:
                        throw new CompileException(node, $"You cannot have a {node} at this level.");
                }
            }

            return result;
        }

        private static UIStyle GetUIStyleOrDefault(UIStyle style) {
            if (style == null) return new UIStyle();
            return style;
        }

        private void MapProperties(UIStyle targetStyle, LightList<StyleASTNode> styleContainerChildren) {
            for (int i = 0; i < styleContainerChildren.Count; i++) {
                StyleASTNode node = styleContainerChildren[i];
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set
                        StylePropertyMappers.MapProperty(targetStyle, propertyNode, context);
                        break;
                    default:
                        throw new CompileException(node, $"You cannot have a {node} at this level.");
                }
            }
        }

        private UIStyleRule MapAttributeContainerToRule(ChainableGroupContainer groupContainer) {
            if (groupContainer == null) return null;

            if (groupContainer is AttributeGroupContainer attribute) {
                return new UIStyleRule(attribute.invert, attribute.identifier, attribute.value, MapAttributeContainerToRule(attribute.next));
            }

            if (groupContainer is ExpressionGroupContainer expression) {
            }

            throw new NotImplementedException("Sorry this feature experiences a slight delay.");
        }
    }
}
