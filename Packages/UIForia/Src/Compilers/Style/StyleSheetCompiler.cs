using System;
using System.Linq;
using UIForia.Animation;
using UIForia.Exceptions;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleSheetCompiler {

        private readonly StyleSheetImporter styleSheetImporter;
        private static readonly UIStyle s_ScratchStyle = new UIStyle();

        private StyleCompileContext context;

        public StyleSheetCompiler(StyleSheetImporter styleSheetImporter) {
            this.styleSheetImporter = styleSheetImporter;
        }

        public StyleSheet Compile(string styleId, LightList<StyleASTNode> rootNodes) {
            // todo -- remove this allocation
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
            int animationCount = 0;
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case StyleRootNode _:
                        containerCount++;
                        break;
                    case AnimationRootNode _:
                        animationCount++;
                        break;
                }
            }

            StyleSheet styleSheet = new StyleSheet(
                context.constants.ToArray(),
                containerCount > 0 
                    ? new UIStyleGroupContainer[containerCount]
                    : ArrayPool<UIStyleGroupContainer>.Empty,
                animationCount > 0
                    ? new AnimationData[animationCount]
                    : ArrayPool<AnimationData>.Empty
            );

            int containerIndex = 0;
            int animationIndex = 0;
            for (int index = 0; index < rootNodes.Count; index++) {
                switch (rootNodes[index]) {
                    case StyleRootNode styleRoot:
                        styleSheet.styleGroupContainers[containerIndex] = CompileStyleGroup(styleRoot);
                        containerIndex++;
                        break;
                    case AnimationRootNode animNode:
                        styleSheet.animations[animationIndex] = CompileAnimation(animNode);
                        break;
                }
            }

            context.Release();

            return styleSheet;
        }

        private AnimationData CompileAnimation(AnimationRootNode animNode) {
            AnimationData data = new AnimationData();
            data.name = animNode.animName;
            data.fileName = context.fileName;
            data.frames = CompileKeyFrames(animNode);
            data.options = CompileAnimationOptions(animNode);
            return data;
        }

        private AnimationKeyFrame[] CompileKeyFrames(AnimationRootNode animNode) {
            if (animNode.keyFrameNodes == null) {
                // todo throw error or log warning?
                return new AnimationKeyFrame[0];
            }
            AnimationKeyFrame[] frames = new AnimationKeyFrame[animNode.keyFrameNodes.Count];
            for (int i = 0; i < animNode.keyFrameNodes.Count; i++) {
                KeyFrameNode keyFrameNode = animNode.keyFrameNodes[i];
                float time = float.Parse(keyFrameNode.identifier) / 100;

                for (int j = 0; j < keyFrameNode.children.Count; j++) {
                    PropertyNode propertyNode = (PropertyNode) keyFrameNode.children[j];
                    StylePropertyMappers.MapProperty(s_ScratchStyle, propertyNode, context);
                }

                int count = s_ScratchStyle.m_StyleProperties.Count;
                StyleProperty[] properties = s_ScratchStyle.m_StyleProperties.Array;
                StructList<StyleKeyFrameValue> keyValues = new StructList<StyleKeyFrameValue>(count);

                for (int j = 0; j < count; j++) {
                    keyValues[j] = new StyleKeyFrameValue(properties[j]);
                }

                keyValues.size = count;
                frames[i] = new AnimationKeyFrame(time);
                frames[i].properties = keyValues;
                s_ScratchStyle.m_StyleProperties.Count = 0;
            }

            return frames;
        }

        private AnimationOptions CompileAnimationOptions(AnimationRootNode animNode) {
            AnimationOptions options = new AnimationOptions();
            
            if (animNode.optionNodes == null) {
                return options;
            }
            
            LightList<AnimationOptionNode> optionNodes = animNode.optionNodes;
            if (optionNodes == null) {
                return options;
            }

            for (int i = 0; i < optionNodes.Count; i++) {
                string optionName = optionNodes[i].optionName;
                StyleASTNode value = optionNodes[i].value;
                
                if (optionName == nameof(AnimationOptions.duration)) {
                    options.duration = (int) StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.iterations)) {
                    options.iterations = (int) StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.loopTime)) {
                    options.loopTime = StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.delay)) {
                    options.delay = StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.direction)) {
                    options.direction = StylePropertyMappers.MapEnum<AnimationDirection>(value, context);
                }
                else if (optionName == nameof(AnimationOptions.loopType)) {
                    options.loopType = StylePropertyMappers.MapEnum<AnimationLoopType>(value, context);
                }
                else if (optionName == nameof(AnimationOptions.playbackType)) {
                    options.playbackType = StylePropertyMappers.MapEnum<AnimationPlaybackType>(value, context);
                }
                else if (optionName == nameof(AnimationOptions.forwardStartDelay)) {
                    options.forwardStartDelay = (int)StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.reverseStartDelay)) {
                    options.reverseStartDelay = (int)StylePropertyMappers.MapNumber(value, context);
                }
                else if (optionName == nameof(AnimationOptions.timingFunction)) {
                    options.timingFunction = StylePropertyMappers.MapEnum<EasingFunction>(value, context);
                }
                else {
                    throw new CompileException(optionNodes[i], "Invalid option argument for animation");
                }
            }

            return options;
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

        private void CompileStyleGroups(StyleNodeContainer root, StyleType styleType, LightList<UIStyleGroup> groups, UIStyleGroup targetGroup) {
            for (int index = 0; index < root.children.Count; index++) {
                StyleASTNode node = root.children[index];
                switch (node) {
                    case PropertyNode propertyNode:
                        // add to normal ui style set
                        StylePropertyMappers.MapProperty(targetGroup.normal, propertyNode, context);
                        break;
                    case AttributeNodeContainer attribute:
                        if (root is AttributeNodeContainer) {
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
                            MapProperties(targetGroup.focused, styleContainer.children);
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

        private UIStyleRule MapAttributeContainerToRule(ChainableNodeContainer nodeContainer) {
            if (nodeContainer == null) return null;

            if (nodeContainer is AttributeNodeContainer attribute) {
                return new UIStyleRule(attribute.invert, attribute.identifier, attribute.value, MapAttributeContainerToRule(attribute.next));
            }

            if (nodeContainer is ExpressionNodeContainer expression) { }

            throw new NotImplementedException("Sorry this feature experiences a slight delay.");
        }

    }

}
