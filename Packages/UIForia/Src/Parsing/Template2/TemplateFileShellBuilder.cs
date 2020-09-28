using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia {

    [DebuggerDisplay("type={type} {key}={value}")]
    public struct AttributeDefinition2 {

        public string key;
        public string value;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;

        public AttributeDefinition2(AttributeType type, AttributeFlags flags, string key, string value, int line = -1, int column = -1) {
            this.type = type;
            this.flags = flags;
            this.key = key;
            this.value = value;
            this.line = line;
            this.column = column;
        }
      
    }

    [Serializable]
    public struct IndexedString {

        public int index;
        public string value;

    }

    [Serializable]
    public struct TextContent {

        public int index;
        public LineInfo lineInfo;
        public RangeInt textExpressions;

    }

    public class TemplateFileShellBuilder {

        internal StructList<IndexedString> tagNames;
        internal StructList<IndexedString> moduleNames;
        internal StructList<IndexedString> requiredTypes;
        internal StructList<IndexedString> genericTypeResolvers;
        internal StructList<TextContent> textContents;
        internal StructList<TextExpression> textExpressions;

        internal StructList<TemplateASTRoot> rootNodes;
        internal StructList<TemplateASTNode> templateNodes;
        internal StructList<AttributeDefinition2> attributeList;
        internal StructList<UsingDeclaration> usings;
        internal LightList<string> referencedNamespaces;
        internal StructList<SlotAST> slots;

        internal TemplateFileShellBuilder() {
            this.rootNodes = new StructList<TemplateASTRoot>(2);
            this.templateNodes = new StructList<TemplateASTNode>(16);
            this.attributeList = new StructList<AttributeDefinition2>(32);
            this.usings = new StructList<UsingDeclaration>(4);
            this.referencedNamespaces = new LightList<string>(4);
            this.tagNames = new StructList<IndexedString>();
            this.moduleNames = new StructList<IndexedString>();
            this.genericTypeResolvers = new StructList<IndexedString>();
            this.textContents = new StructList<TextContent>();
            this.textExpressions = new StructList<TextExpression>();
            this.requiredTypes = new StructList<IndexedString>();
            this.slots = new StructList<SlotAST>();
        }

        public void Build(TemplateFileShell shell) {
            shell.tagNames = tagNames.ToArray();
            shell.moduleNames = moduleNames.ToArray();
            shell.requiredTypes = requiredTypes.ToArray();
            shell.genericTypeResolvers = genericTypeResolvers.ToArray();
            shell.textContents = textContents.ToArray();
            shell.textExpressions = textExpressions.ToArray();
            shell.rootNodes = rootNodes.ToArray();
            shell.templateNodes = templateNodes.ToArray();
            shell.attributeList = attributeList.ToArray();
            shell.usings = usings.ToArray();
            shell.referencedNamespaces = referencedNamespaces.ToArray();
            shell.slots = slots.ToArray();

            tagNames.size = 0;
            moduleNames.size = 0;
            requiredTypes.size = 0;
            genericTypeResolvers.size = 0;
            textContents.size = 0;
            textExpressions.size = 0;
            rootNodes.size = 0;
            templateNodes.size = 0;
            attributeList.size = 0;
            usings.size = 0;
            referencedNamespaces.size = 0;
            slots.size = 0;
        }

        private ref TemplateASTNode AddTemplateNode(TemplateNodeType nodeType, IList<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireType) {
            int idx = templateNodes.size;
            RangeInt attrRange = AddAttributes(attributes);
            templateNodes.Add(new TemplateASTNode() {
                index = idx,
                parentId = -1,
                nextSiblingIndex = -1,
                firstChildIndex = -1,
                lineNumber = lineInfo.line,
                columnNumber = lineInfo.column,
                attributeRangeStart = attrRange.start,
                attributeRangeEnd = attrRange.end,
                templateNodeType = nodeType,
            });

            if (genericTypeResolver != null) {
                genericTypeResolvers.Add(new IndexedString() {
                    index = idx, value = genericTypeResolver
                });
            }

            if (requireType != null) {
                requiredTypes.Add(new IndexedString() {
                    index = idx, value = requireType
                });
            }

            return ref templateNodes.array[idx];
        }

        public TemplateASTBuilder CreateRootNode(string templateId, IList<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireType) {
            rootNodes.Add(new TemplateASTRoot() {
                // templateNameId = templateId,
                slotDefinitionCount = 0,
                firstSlotDefinitionIndex = 0,
                templateIndex = templateNodes.size
            });
            AddTemplateNode(TemplateNodeType.Root, attributes, lineInfo, genericTypeResolver, requireType);
            return new TemplateASTBuilder(templateNodes.size - 1, this, rootNodes.size - 1);
        }

        public void AddUsing(UsingDeclaration declaration) {
            usings.Add(declaration);

            // todo -- this is awkward
            if (declaration.name != null) {
                referencedNamespaces.Add(declaration.name);
            }
        }

        private TemplateASTBuilder CreateSlotNode(TemplateASTBuilder parentId, string slotName, IList<AttributeDefinition2> attributes, LineInfo lineInfo, SlotType slotType, string requireType) {
            int nodeId = templateNodes.size;

            TemplateNodeType nodeType = 0;

            switch (slotType) {
                case SlotType.Define:
                    AddChild(parentId.index, nodeId);
                    nodeType = TemplateNodeType.SlotDefine;
                    break;

                case SlotType.Forward:
                    nodeType = TemplateNodeType.SlotForward;
                    break;

                case SlotType.Override:
                    nodeType = TemplateNodeType.SlotOverride;
                    break;
            }

            ref TemplateASTNode node = ref AddTemplateNode(nodeType, attributes, lineInfo, null, requireType);

            node.parentId = parentId.index;
            // node.tagName = slotName;

            ref TemplateASTRoot root = ref rootNodes.array[parentId.rootId];

            slots = slots ?? new StructList<SlotAST>(4);

            if (root.slotDefinitionCount == 0) {
                root.firstSlotDefinitionIndex = slots.size;
            }

            root.slotDefinitionCount++;

            // when compiling we'll need to verify that the target node can support slots
            // parsing is super permissive, restrict in a later phase
            slots.Add(new SlotAST() {
                slotName = slotName,
                templateNodeId = nodeId,
                slotType = slotType
            });

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private TemplateASTBuilder CreateElementNode(TemplateASTBuilder parentId, string moduleName, string tagName, IList<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requiredType) {
            int nodeId = templateNodes.size;

            AddChild(parentId.index, nodeId);

            ref TemplateASTNode node = ref AddTemplateNode(TemplateNodeType.Unresolved, attributes, lineInfo, genericTypeResolver, requiredType);

            node.parentId = parentId.index;

            if (!string.IsNullOrEmpty(moduleName)) {
                // if (!stringTable.TryGetValue(moduleName, out idx)) {
                //     stringTable[moduleName] = idx;
                // }
                
                moduleNames.Add(new IndexedString() {
                    index = nodeId,
                    value = moduleName
                });
            }

            if (!string.IsNullOrEmpty(tagName)) {
                tagNames.Add(new IndexedString() {
                    index = nodeId,
                    value = tagName
                });
            }

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private TemplateASTBuilder CreateRepeatNode(TemplateASTBuilder parentId, List<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireChildTypeExpression) {
            int nodeId = templateNodes.size;

            AddChild(parentId.index, nodeId);

            ref TemplateASTNode node = ref AddTemplateNode(TemplateNodeType.Repeat, attributes, lineInfo, genericTypeResolver, requireChildTypeExpression);

            node.parentId = parentId.index;

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private TemplateASTBuilder CreateTextNode(TemplateASTBuilder parentId, LineInfo lineInfo, IList<AttributeDefinition2> attributes, TemplateNodeType nodeType) {
            int nodeId = templateNodes.size;

            AddChild(parentId.index, nodeId);

            ref TemplateASTNode node = ref AddTemplateNode(nodeType, attributes, lineInfo, null, null);

            node.parentId = parentId.index;

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private void SetTextContent(string textContent, LineInfo lineInfo) {
            int nodeId = templateNodes.size - 1;

            RangeInt range = new RangeInt(textExpressions.size, 0);
            try {
                TextTemplateProcessor.ProcessTextExpressions(textContent, textExpressions);
                range.length = textExpressions.size - range.start;
            }
            catch (Exception e) {
                // todo -- dont drop this on the floor
                Debug.Log(e);
                textExpressions.size = range.start;
            }

            textContents.Add(new TextContent() {
                index = nodeId,
                textExpressions = range,
                lineInfo = lineInfo
            });
        }

        private RangeInt AddAttributes(IList<AttributeDefinition2> attributes) {
            if (attributes == null || attributes.Count == 0) return default;
            int start = attributeList.size;
            attributeList.AddRange(attributes);
            return new RangeInt(start, attributes.Count);
        }

        private void AddChild(int parentId, int nodeId) {
            ref TemplateASTNode parentNode = ref templateNodes.array[parentId];

            if (parentNode.childCount == 0) {
                parentNode.firstChildIndex = nodeId;
            }
            else {
                int ptr = parentNode.firstChildIndex;
                while (true) {
                    ref TemplateASTNode child = ref templateNodes.array[ptr];
                    if (child.nextSiblingIndex < 0) {
                        child.nextSiblingIndex = nodeId;
                        break;
                    }

                    ptr = child.nextSiblingIndex;
                }
            }

            parentNode.childCount++;
        }

        public struct TemplateASTBuilder {

            internal readonly int index;
            internal readonly int rootId;
            internal readonly TemplateFileShellBuilder templateFileShellBuilder;

            internal TemplateASTBuilder(int index, TemplateFileShellBuilder templateFileShellBuilder, int rootId) {
                this.index = index;
                this.templateFileShellBuilder = templateFileShellBuilder;
                this.rootId = rootId;
            }

            public TemplateASTBuilder AddSlotChild(string slotName, IList<AttributeDefinition2> childAttributes, LineInfo lineInfo, SlotType slotType, string requireType) {
                return templateFileShellBuilder.CreateSlotNode(this, slotName, childAttributes, lineInfo, slotType, requireType);
            }

            public TemplateASTBuilder AddElementChild(string moduleName, string tagName, IList<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireChildTypeExpression) {
                return templateFileShellBuilder.CreateElementNode(this, moduleName, tagName, attributes, lineInfo, genericTypeResolver, requireChildTypeExpression);
            }

            public TemplateASTBuilder AddTextChild(IList<AttributeDefinition2> attributes, LineInfo templateLineInfo) {
                return templateFileShellBuilder.CreateTextNode(this, templateLineInfo, attributes, TemplateNodeType.Text);
            }

            public TemplateASTBuilder AddRepeatChild(List<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireChildTypeExpression) {
                return templateFileShellBuilder.CreateRepeatNode(this, attributes, lineInfo, genericTypeResolver, requireChildTypeExpression);
            }

            public void SetTextContent(string textContent, LineInfo lineInfo) {
                templateFileShellBuilder.SetTextContent(textContent, lineInfo);
            }

        }

    }

}