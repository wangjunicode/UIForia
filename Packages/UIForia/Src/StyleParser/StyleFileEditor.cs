using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Parsing {

    public struct PropertyEditorNode {

        public string key;
        public string value;
        public bool isStatic;

    }

    public struct TransitionEditorNode {

        public string key;
        public UITimeMeasurement duration;
        public UITimeMeasurement delay;
        public PropertyAnimCurve curve;

    }

    public class ConstantEditorNode {

        public string name;
        public string value;
        public bool exported;

        public ConstantEditorNode(string name, string value, bool exported = false) {
            this.name = name;
            this.value = value;
            this.exported = exported;
        }

    }

    public class AttributeEditorNode : StyleBlockEditorNode {

        public string attrKey;
        public string attrValue;
        public AttributeOperator op;

        public AttributeEditorNode(string attrKey, AttributeOperator op, string attrValue = null) {
            this.attrKey = attrKey;
            this.op = op;
            this.attrValue = attrValue;
        }

        public override string Name {
            get {
                if (string.IsNullOrEmpty(attrValue) || op == AttributeOperator.Exists) {
                    return $"[attr:{attrKey}]";
                }

                switch (op) {

                    case AttributeOperator.Equal:
                        return $"[attr:{attrKey}=\"{attrValue}\"]";

                    default:
                    case AttributeOperator.Exists:
                        return $"[attr:{attrKey}]";

                    case AttributeOperator.Contains:
                        return $"[attr:{attrKey} contains \"{attrValue}\"]";

                    case AttributeOperator.StartsWith:
                        return $"[attr:{attrKey} starts-with \"{attrValue}\"]";

                    case AttributeOperator.EndsWith:
                        return $"[attr:{attrKey} ends-with \"{attrValue}\"]";
                }

            }
        }

    }

    public class MixinUsageEditorNode : StyleBlockEditorNode {

        public string mixinName;

        public MixinUsageEditorNode(string mixinName) {
            this.mixinName = mixinName;
        }

        public override string Name => $"mixin({mixinName})";

    }

    public class TagNameEditorNode : StyleBlockEditorNode {

        public string tagName;

        public TagNameEditorNode(string tagName) {
            this.tagName = tagName;
        }

        public override string Name => $"[when:tag({tagName})";

    }

    public class StateEditorNode : StyleBlockEditorNode {

        public StyleState state;

        public StateEditorNode(StyleState state) {
            this.state = state;
        }

        public override string Name {
            get => $"[{state.ToString().ToLower()}]";
        }

    }

    public class ConditionEditorNode : StyleBlockEditorNode {

        public string conditionName;

        public ConditionEditorNode(string conditionName) {
            this.conditionName = conditionName;
        }

        public override string Name => $"#{conditionName}";

    }

    public class FirstWithTagEditorNode : StyleBlockEditorNode {

        public string tagName;

        public FirstWithTagEditorNode(string tagName) {
            this.tagName = tagName;
        }

        public override string Name => $"[when:first-with-tag({tagName})]";

    }

    public class LastWithTagEditorNode : StyleBlockEditorNode {

        public string tagName;

        public LastWithTagEditorNode(string tagName) {
            this.tagName = tagName;
        }

        public override string Name => $"[when:last-with-tag({tagName})]";

    }

    public class OnlyWithTagEditorNode : StyleBlockEditorNode {

        public string tagName;

        public OnlyWithTagEditorNode(string tagName) {
            this.tagName = tagName;
        }

        public override string Name => $"[when:only-with-tag({tagName})]";

    }

    public class FocusWithinEditorNode : StyleBlockEditorNode {

        public override string Name => "[when:focus-within]";

    }

    public class NoChildrenEditorNode : StyleBlockEditorNode {

        public override string Name => "[when:empty]";

    }

    public class OnlyChildEditorNode : StyleBlockEditorNode {

        public override string Name => "[when:only-child]";

    }

    public class NthChildEditorNode : StyleBlockEditorNode {

        public int stepSize;
        public int offset;

        public NthChildEditorNode(NthChildData data) {
            stepSize = data.stepSize;
            offset = data.offset;
        }

        public override string Name {
            get {
                if (stepSize == 2 && offset == 0) {
                    return "[when:nth-child(even)]";
                }

                if (stepSize == 2 && offset == 1) {
                    return "[when:nth-child(odd)]";
                }

                return $"[when:nth-child({stepSize}n{(offset > 0 ? '+' : ' ')}{offset})]";
            }
        }

    }

    public class ChildCountEditorNode : StyleBlockEditorNode {

        public ChildCountOperator op;
        public uint childCount;

        public ChildCountEditorNode(ChildCountOperator op, uint childCount) {
            this.op = op;
            this.childCount = childCount;
        }

        public override string Name {
            get {
                switch (op) {
                    default:
                    case ChildCountOperator.EqualTo:
                        return $"[when:child-count(== {childCount}]";

                    case ChildCountOperator.NotEqualTo:
                        return $"[when:child-count(!= {childCount}]";

                    case ChildCountOperator.GreaterThan:
                        return $"[when:child-count(> {childCount}]";

                    case ChildCountOperator.LessThan:
                        return $"[when:child-count(< {childCount}]";

                    case ChildCountOperator.GreaterThanEqualTo:
                        return $"[when:child-count(>= {childCount}]";

                    case ChildCountOperator.LessThanEqualTo:
                        return $"[when:child-count(<= {childCount}]";

                }

            }
        }

    }

    public class FirstChildEditorNode : StyleBlockEditorNode {

        public override string Name {
            get => "[when:first-child]";
        }

    }

    public class LastChildEditorNode : StyleBlockEditorNode {

        public override string Name {
            get => "[when:last-child]";
        }

    }

    public class StyleRootBlockEditorNode : StyleBlockEditorNode {

        public override string Name => "[root]";

    }

    public abstract class StyleBlockEditorNode {

        public StyleBlockEditorNode parent;

        public List<StyleBlockEditorNode> children;
        public List<PropertyEditorNode> propertyList;
        public List<TransitionEditorNode> transitionList;

        public StyleBlockEditorNode() {
            children = new List<StyleBlockEditorNode>();
            propertyList = new List<PropertyEditorNode>();
        }

        public abstract string Name { get; }

    }

    public abstract class StyleLikeDeclarationEditorNode {

        public string name;
        public StyleBlockEditorNode rootBlock;
        public bool isExported;

        protected StyleLikeDeclarationEditorNode(string name, bool isExported) {
            this.name = name;
            this.isExported = isExported;
        }

        public StyleBlockEditorNode GetRootBlock() {
            return rootBlock;
        }

        public StyleBlockEditorNode GetStyleBlock(string path) {
            throw new NotImplementedException();
        }

    }

    public class StyleDeclarationEditorNode : StyleLikeDeclarationEditorNode {

        public StyleDeclarationEditorNode(string name, bool isExported) : base(name, isExported) { }

    }

    public class MixinDeclarationEditorNode : StyleLikeDeclarationEditorNode {

        public MixinDeclarationEditorNode(string name, bool isExported) : base(name, isExported) { }

    }

    public class StyleFileEditor {

        private StyleFileShell shell;

        private LightList<StyleDeclarationEditorNode> styleDeclarations;
        private LightList<MixinDeclarationEditorNode> mixinDeclarations;
        private LightList<ConstantEditorNode> constantNodes;

        internal StyleFileEditor(StyleFileShell shell) {
            this.shell = shell;

            styleDeclarations = new LightList<StyleDeclarationEditorNode>(shell.styles.Length);
            mixinDeclarations = new LightList<MixinDeclarationEditorNode>(shell.constants.Length);
            constantNodes = new LightList<ConstantEditorNode>(shell.constants.Length);

            for (int i = 0; i < shell.styles.Length; i++) {
                StyleDeclarationEditorNode styleNode = new StyleDeclarationEditorNode(
                    shell.GetString(shell.styles[i].nameRange),
                    shell.styles[i].exported
                );
                CreateStyleLikeNode(shell.styles[i].rootBlockIndex, styleNode);
                styleDeclarations.Add(styleNode);
            }

            for (int i = 0; i < shell.mixins.Length; i++) {
                MixinDeclarationEditorNode mixinNode = new MixinDeclarationEditorNode(
                    shell.GetString(shell.mixins[i].nameRange),
                    shell.mixins[i].exported
                );
                CreateStyleLikeNode(shell.mixins[i].rootBlockIndex, mixinNode);
                mixinDeclarations.Add(mixinNode);
            }

            for (int i = 0; i < shell.constants.Length; i++) {
                constantNodes.Add(new ConstantEditorNode(
                    shell.GetString(shell.constants[i].identifier),
                    shell.GetString(shell.constants[i].value),
                    shell.constants[i].exported)
                );
            }

        }

        private void CreateStyleLikeNode(ParseBlockId rootBlockIndex, StyleLikeDeclarationEditorNode target) {

            ref ParseBlockNode rootBlock = ref shell.blocks[rootBlockIndex.id];

            target.rootBlock = new StyleRootBlockEditorNode();
            target.rootBlock.propertyList = CreatePropertyList(ref rootBlock);
            target.rootBlock.transitionList = CreateTransitionList(ref rootBlock);

            StructStack<BlockInfo> stack = StructStack<BlockInfo>.Get();

            LightList<BlockInfo> buffer = LightList<BlockInfo>.Get();

            ParseBlockId ptr = rootBlock.firstChild;

            while (ptr.id != 0) {
                buffer.Add(new BlockInfo() {
                    parent = target.rootBlock,
                    blockIdx = ptr
                });
                ptr = shell.blocks[ptr.id].nextSibling;
            }

            for (int i = buffer.size - 1; i >= 0; i--) {
                stack.Push(buffer[i]);
            }

            while (stack.size != 0) {
                BlockInfo blockInfo = stack.Pop();
                ref ParseBlockNode block = ref shell.blocks[blockInfo.blockIdx.id];
                StyleBlockEditorNode blockEditor = CreateBlockNode(block);
                blockEditor.propertyList = CreatePropertyList(ref block);
                blockEditor.transitionList = CreateTransitionList(ref block);

                blockEditor.parent = blockInfo.parent;
                blockInfo.parent.children.Add(blockEditor);

                ptr = block.firstChild;
                buffer.size = 0;

                while (ptr.id != 0) {
                    buffer.Add(new BlockInfo() {
                        parent = blockEditor,
                        blockIdx = ptr
                    });
                    ptr = shell.blocks[ptr.id].nextSibling;
                }

                for (int i = buffer.size - 1; i >= 0; i--) {
                    stack.Push(buffer[i]);
                }

            }

            StructStack<BlockInfo>.Release(ref stack);
            buffer.Release();
        }

        private StyleBlockEditorNode CreateBlockNode(ParseBlockNode block) {
            switch (block.type) {
                case BlockNodeType.Selector:
                    throw new NotImplementedException();

                case BlockNodeType.Attribute:
                    return new AttributeEditorNode(
                        shell.GetString(block.blockData.attributeData.attrKeyRange),
                        block.blockData.attributeData.compareOp,
                        shell.GetString(block.blockData.attributeData.attrValueRange)
                    );

                case BlockNodeType.FirstChild:
                    return new FirstChildEditorNode();

                case BlockNodeType.LastChild:
                    return new LastChildEditorNode();

                case BlockNodeType.State:
                    return new StateEditorNode(block.stateRequirement);

                case BlockNodeType.Mixin: // special treatment here because a mixin block is just a 'paste' operation, we we just return the name
                    return new MixinUsageEditorNode(shell.GetString(block.blockData.tagData.tagNameRange));

                case BlockNodeType.Condition:
                    return new ConditionEditorNode(shell.GetString(block.blockData.conditionData.conditionRange));

                case BlockNodeType.OnlyChild:
                    return new OnlyChildEditorNode();

                case BlockNodeType.NoChildren:
                    return new NoChildrenEditorNode();

                case BlockNodeType.FocusWithin:
                    return new FocusWithinEditorNode();

                case BlockNodeType.OnlyWithTag:
                    return new OnlyWithTagEditorNode(shell.GetString(block.blockData.tagData.tagNameRange));

                case BlockNodeType.LastWithTag:
                    return new LastWithTagEditorNode(shell.GetString(block.blockData.tagData.tagNameRange));

                case BlockNodeType.FirstWithTag:
                    return new FirstWithTagEditorNode(shell.GetString(block.blockData.tagData.tagNameRange));

                case BlockNodeType.NthChild:
                    return new NthChildEditorNode(block.blockData.nthChildData);

                case BlockNodeType.TagName:
                    return new TagNameEditorNode(shell.GetString(block.blockData.tagData.tagNameRange));

                case BlockNodeType.ChildCount:
                    return new ChildCountEditorNode(block.blockData.childCountData.op, (uint) block.blockData.childCountData.number);

                case BlockNodeType.NthWithTag:
                    throw new NotImplementedException("NthWithTag not yet supported");

                default:
                    return null;
            }
        }

        private List<TransitionEditorNode> CreateTransitionList(ref ParseBlockNode rootBlock) {
            List<TransitionEditorNode> list = new List<TransitionEditorNode>();
            DeclarationId ptr = rootBlock.lastDeclaration;

            while (ptr.id != 0) {
                ref StyleDeclaration property = ref shell.propertyDefinitions[ptr.id];

                if (property.declType != StyleDeclarationType.Transition) {
                    ptr = property.prevSibling;
                    continue;
                }

                string value;
                if (property.declarationData.transitionDeclarationData.propertyId != -1) {
                    value = PropertyParsers.PropertyIndexToName(property.declarationData.transitionDeclarationData.propertyId);
                }
                else if (property.declarationData.transitionDeclarationData.shortHandId != 1) {
                    value = PropertyParsers.ShortHandIndexToName(property.declarationData.transitionDeclarationData.shortHandId);
                }
                else {
                    value = shell.GetString(property.declarationData.transitionDeclarationData.customPropertyRange);
                }

                TransitionDeclaration transition = shell.transitions[property.declarationData.transitionDeclarationData.transitionId];

                list.Add(new TransitionEditorNode() {
                    key = value,
                    // curve = transition.propertyAnimCurve,
                    duration = new UITimeMeasurement(transition.duration),
                    delay = new UITimeMeasurement(transition.delay)
                });

                ptr = property.prevSibling;
            }

            list.Reverse();
            return list;
        }

        private List<PropertyEditorNode> CreatePropertyList(ref ParseBlockNode rootBlock) {
            List<PropertyEditorNode> list = new List<PropertyEditorNode>();
            DeclarationId ptr = rootBlock.lastDeclaration;

            while (ptr.id != 0) {
                ref StyleDeclaration property = ref shell.propertyDefinitions[ptr.id];
                string propertyName;

                switch (property.declType) {

                    case StyleDeclarationType.Property:
                        propertyName = PropertyParsers.PropertyIndexToName(property.declarationData.propertyDeclarationData.propertyId);
                        break;

                    case StyleDeclarationType.ShortHand:
                        propertyName = PropertyParsers.ShortHandIndexToName(property.declarationData.propertyDeclarationData.propertyId);
                        break;

                    case StyleDeclarationType.MaterialVar:
                    case StyleDeclarationType.PainterVar:
                    case StyleDeclarationType.MixinUsage:
                    case StyleDeclarationType.MixinProperty:
                    default:
                        ptr = property.prevSibling;
                        continue;
                }

                string value = shell.GetString(property.declarationData.propertyDeclarationData.valueRange);

                list.Add(new PropertyEditorNode() {
                    key = propertyName,
                    value = value,
                    isStatic = false,
                });
                ptr = property.prevSibling;
            }

            list.Reverse();
            return list;
        }

        public StyleDeclarationEditorNode GetStyle(string name) {
            for (int i = 0; i < styleDeclarations.size; i++) {
                if (styleDeclarations[i].name == name) {
                    return styleDeclarations[i];
                }
            }

            return null;
        }

        public MixinDeclarationEditorNode GetMixin(string name) {
            for (int i = 0; i < mixinDeclarations.size; i++) {
                if (mixinDeclarations[i].name == name) {
                    return mixinDeclarations[i];
                }
            }

            return null;
        }

        public ConstantEditorNode GetConstant(string identifier) {
            for (int i = 0; i < constantNodes.size; i++) {
                if (constantNodes[i].name == identifier) {
                    return constantNodes[i];
                }
            }

            return null;
        }

        public void SetConstant(string identifier, string value, bool exported = false) {
            for (int i = 0; i < constantNodes.size; i++) {
                if (constantNodes[i].name == identifier) {
                    constantNodes[i].value = value;
                    constantNodes[i].exported = exported;
                    return;
                }
            }

            constantNodes.Add(new ConstantEditorNode(identifier, value, exported));

        }

        private struct BlockInfo {

            public ParseBlockId blockIdx;
            public StyleBlockEditorNode parent;

        }

    }

}