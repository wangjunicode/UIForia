using System;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing {

    public struct TemplateNodeDebugData {

        public string fileName;
        public string tagName;
        public TemplateLineInfo lineInfo;

        public override string ToString() {
            return $"<{tagName}> @({fileName} line {lineInfo})";
        }

    }

    public struct AttributeNodeDebugData {

        public string fileName;
        public string tagName;
        public string content;
        public TemplateLineInfo lineInfo;

        public AttributeNodeDebugData(string fileName, string tagName, TemplateLineInfo lineInfo, string content) {
            this.fileName = fileName;
            this.tagName = tagName;
            this.lineInfo = lineInfo;
            this.content = content;
        }

    }

    public abstract class TemplateNode {

        public SizedArray<TemplateNode> children;
        public ReadOnlySizedArray<AttributeDefinition> attributes;
        public TemplateRootNode root;
        public TemplateNode parent;
        public ProcessedType processedType;
        public string originalString;
        public TemplateLineInfo lineInfo;
        public string genericTypeResolver;
        public string requireType;
        public bool isModified;
        public Type requiredChildType;

        protected TemplateNode(ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) {
            this.attributes = attributes;
            this.lineInfo = templateLineInfo;
        }

        public void AddChild(TemplateNode child) {
            child.parent = this;
            child.root = root;
            children.Add(child);
        }

        public bool HasProperty(string attr) {
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Property) {
                    if (attributes.array[i].key == attr) {
                        return true;
                    }
                }
            }

            return false;
        }

        public TemplateNode this[int i] => children.array != null ? children.array[i] : null;

        public int ChildCount => children.size;
        public Type ElementType => processedType.rawType;

        public virtual TemplateNodeDebugData TemplateNodeDebugData => new TemplateNodeDebugData() {
            lineInfo = lineInfo,
            tagName = "",
            fileName = root != null ? root.templateShell.filePath : ((TemplateRootNode) this).templateShell.filePath
        };

        public int CountRealAttributes() {
            int count = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    count++;
                }
            }
            return count;
        }

        public abstract string GetTagName();

        public virtual void AddSlotOverride(SlotNode slotNode) {
            // todo -- diagnostic
            Debug.Log($"Cannot add a <{slotNode.GetTagName()}> to <{GetTagName()}>");
        }

        public bool TryCreateElementNode(string moduleName, string tagName, ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo lineInfo, string genericTypeResolver, string requireChildTypeExpression, out TemplateNode templateNode) {

            templateNode = null;

            ProcessedType elementType = root.templateShell.module.ResolveTagName(moduleName, tagName, new TypeProcessor.DiagnosticWrapper()); // todo -- diagnostics

            if (elementType == null) {
                return false;
            }

            if (elementType.IsUnresolvedGeneric && !string.IsNullOrEmpty(genericTypeResolver)) {
                elementType = TypeProcessor.ResolveGenericElementType(elementType, genericTypeResolver, root.templateShell.referencedNamespaces, new TypeProcessor.DiagnosticWrapper());
                if (elementType == null) {
                    return false;
                }
            }

            Type requiredType = null;

            if (!string.IsNullOrEmpty(requireChildTypeExpression)) {

                requiredType = TypeResolver.Default.ResolveTypeExpression(root.processedType.rawType, root.templateShell.referencedNamespaces, requireType);

                if (requiredType == null) {
                    return root.templateShell.ReportError(lineInfo, $"Unable to resolve required child type `{requireType}`");
                }

                if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                    return root.templateShell.ReportError(lineInfo, $"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither");
                }

            }

            if (typeof(UITextElement).IsAssignableFrom(elementType.rawType)) {
                templateNode = new TextNode(elementType, attributes, lineInfo) {
                    root = root,
                    parent = this,
                    requiredChildType = requiredType
                };
            }
            else if (elementType.IsContainerElement) {
                templateNode = new ContainerNode(moduleName, tagName, attributes, lineInfo) {
                    root = root,
                    parent = this,
                    processedType = elementType,
                    requiredChildType = requiredType
                };
            }
            else {
                templateNode = new ExpandedNode(moduleName, tagName, attributes, lineInfo) {
                    root = root,
                    parent = this,
                    processedType = elementType,
                    requiredChildType = requiredType
                };
            }

            AddChild(templateNode);

            return true;

        }

        public bool TryCreateSlotNode(string slotName, ReadOnlySizedArray<AttributeDefinition> attributes, ReadOnlySizedArray<AttributeDefinition> injectedAttributes, TemplateLineInfo templateLineInfo, SlotType slotType, string requiredChildType, out TemplateNode slot) {
            slot = null;
            
            switch (slotType) {

                case SlotType.Define:
                    slot = new SlotNode(slotName, attributes, injectedAttributes, templateLineInfo, SlotType.Define);
                    slot.requireType = requiredChildType;
                    root.AddSlot((SlotNode) slot);
                    AddChild(slot);
                    break;

                case SlotType.Forward: {

                    if (!(this is ElementNode expanded)) {
                        return root.templateShell.ReportError(lineInfo, GetTagName() + " does not support forwarded slot nodes");
                    }

                    slot = new SlotNode(slotName, attributes, injectedAttributes, templateLineInfo, SlotType.Forward);
                    slot.requireType = requiredChildType;
                    // AddChild(slot);
                    expanded.AddSlotOverride((SlotNode) slot);
                    root.AddSlot((SlotNode) slot);
                    return true;
                }

                case SlotType.Override: {

                    if (!(this is ElementNode expanded)) {
                        return root.templateShell.ReportError(lineInfo, GetTagName() + " does not support overriden slot nodes");
                    }

                    slot = new SlotNode(slotName, attributes, injectedAttributes, templateLineInfo, SlotType.Override);
                    slot.requireType = requiredChildType;
                    slot.root = root;
                    expanded.AddSlotOverride((SlotNode) slot);
                    return true;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null);
            }

            return false;
        }

        public bool TryCreateRepeatNode(ReadOnlySizedArray<AttributeDefinition> attributes, TemplateLineInfo templateLineInfo, out TemplateNode templateNode) {
            templateNode = new RepeatNode(attributes, templateLineInfo);
            AddChild(templateNode);
            return true;
        }

        public virtual void DebugDump(IndentedStringBuilder stringBuilder) {

            stringBuilder.Append(GetTagName());
            stringBuilder.Indent();
            for (int i = 0; i < children.size; i++) {
                children[i].DebugDump(stringBuilder);
            }

            stringBuilder.Outdent();
            stringBuilder.Append("\n");
        }

    }

}