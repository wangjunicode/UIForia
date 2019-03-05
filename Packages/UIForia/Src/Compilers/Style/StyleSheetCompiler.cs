using System;
using System.Linq;
using UIForia.Exceptions;
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

        public StyleSheet Compile(string styleId, LightList<StyleASTNode> rootNodes) {
            try {
                context = new StyleSheetConstantImporter(styleSheetImporter).CreateContext(rootNodes);
            }
            catch (CompileException e) {
                e.SetFileName(styleId);
                throw;
            }

            context.fileName = styleId;

            // todo add imported style groups

            int containerCount = 0;
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case StyleRootNode _:
                        containerCount++;
                        break;
                }
            }

            StyleSheet styleSheet = new StyleSheet(context.constants.ToArray(), new UIStyleGroupContainer[containerCount]);

            int containerIndex = 0;
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case StyleRootNode styleRoot:
                        styleSheet.styleGroupContainers[containerIndex] = CompileStyleGroup(styleRoot);
                        containerIndex++;
                        break;
                }
            }

            context.Release();

            return styleSheet;
        }

        private UIStyleGroupContainer CompileStyleGroup(StyleRootNode styleRoot) {
            UIStyleGroup defaultGroup = new UIStyleGroup();
            defaultGroup.normal = new UIStyle();
            defaultGroup.name = styleRoot.identifier ?? styleRoot.tagName;
            StyleType styleType = styleRoot.tagName != null ? StyleType.Implicit : StyleType.Shared;

            LightList<UIStyleGroup> styleGroups = new LightList<UIStyleGroup>(4);
            styleGroups.Add(defaultGroup);

            CompileStyleGroups(styleRoot, styleType, styleGroups, defaultGroup);

            return new UIStyleGroupContainer(defaultGroup.name, styleType, styleGroups);
        }

        private void CompileStyleGroups(StyleGroupContainer root, StyleType styleType, LightList<UIStyleGroup> groups, UIStyleGroup targetGroup) {
            for (int index = 0; index < root.children.Count; index++) {
                StyleASTNode node = root.children[index];
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set
                        StylePropertyMappers.MapProperty(targetGroup.normal, propertyNode, context);
                        break;
                    case AttributeGroupContainer attribute:
                        if (root is AttributeGroupContainer) {
                            throw new CompileException(attribute, "You cannot nest attribute group definitions.");
                        }

                        UIStyleGroup attributeGroup = new UIStyleGroup();
                        attributeGroup.normal = new UIStyle();
                        attributeGroup.name = root.identifier;
                        attributeGroup.rule = MapAttributeContainerToRule(attribute);
                        attributeGroup.styleType = styleType;
                        groups.Add(attributeGroup);
                        CompileStyleGroups(attribute, styleType, groups, attributeGroup);

                        break;
                    case StyleStateContainer styleContainer:
                        if (styleContainer.identifier == "hover") {
                            targetGroup.hover = targetGroup.hover ?? new UIStyle();
                            MapProperties(targetGroup.hover, styleContainer.children);
                        }
                        else if (styleContainer.identifier == "focus") {
                            targetGroup.focused = targetGroup.focused ?? new UIStyle();
                            MapProperties(targetGroup.hover, styleContainer.children);
                        }
                        else if (styleContainer.identifier == "active") {
                            targetGroup.active = targetGroup.active ?? new UIStyle();
                            MapProperties(targetGroup.active, styleContainer.children);
                        }
                        else throw new CompileException(styleContainer, $"Unknown style state '{styleContainer.identifier}'. Please use [hover], [focus] or [active] instead.");

                        break;
                    default:
                        throw new CompileException(node, $"You cannot have a {node} at this level.");
                }
            }
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
