using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

    [DebuggerDisplay("type={type} {key}={value}")]
    public struct AttributeDefinition3 {

        public RangeInt key;
        public RangeInt value;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;

        public AttributeDefinition3(AttributeType type, AttributeFlags flags, RangeInt key, RangeInt value, int line = -1, int column = -1) {
            this.type = type;
            this.flags = flags;
            this.key = key;
            this.value = value;
            this.line = line;
            this.column = column;
        }

    }

    public struct TextContent {

        public LineInfo lineInfo;
        public RangeInt stringRange;
        public bool isExpression;

    }

    public struct UsingDeclaration {

        public RangeInt namespaceRange;
        // public UsingDeclarationType type;

    }

    public struct StyleDeclaration {

        public RangeInt path;
        public RangeInt sourceBody;
        public RangeInt alias;

    }

    internal class TemplateFileShellBuilder {

        private StructList<TextContent> textContents;
        private StructList<TextExpression> textExpressions;

        private StructList<TemplateASTRoot> rootNodes;
        private StructList<TemplateASTNode> templateNodes;
        private StructList<AttributeDefinition3> attributeList;
        private StructList<UsingDeclaration> usings;
        private StructList<StyleDeclaration> styles;
        private StructList<SlotAST> slots;

        private DataList<char>.Shared charBuffer;

        internal TemplateFileShellBuilder() {
            this.charBuffer = new DataList<char>.Shared(1024, Allocator.Persistent);
            this.rootNodes = new StructList<TemplateASTRoot>(2);
            this.styles = new StructList<StyleDeclaration>();
            this.textExpressions = new StructList<TextExpression>();
            this.templateNodes = new StructList<TemplateASTNode>(16);
            this.attributeList = new StructList<AttributeDefinition3>(32);
            this.usings = new StructList<UsingDeclaration>(4);
            this.textContents = new StructList<TextContent>();
            this.slots = new StructList<SlotAST>();
        }

        ~TemplateFileShellBuilder() {
            charBuffer.Dispose();
        }

        public TemplateFileShell Build(string templateLocation) {

            TemplateFileShell shell = new TemplateFileShell(templateLocation);
            shell.textContents = textContents.ToArray();
            shell.rootNodes = rootNodes.ToArray();
            shell.templateNodes = templateNodes.ToArray();
            shell.attributeList = attributeList.ToArray();
            shell.usings = usings.ToArray();
            shell.slots = slots.ToArray();
            shell.charBuffer = charBuffer.ToArray();
            shell.styles = styles.ToArray();

            charBuffer.size = 0;
            styles.size = 0;
            textContents.size = 0;
            rootNodes.size = 0;
            templateNodes.size = 0;
            attributeList.size = 0;
            usings.size = 0;
            slots.size = 0;
            return shell;
        }

        private ref TemplateASTNode AddTemplateNode(TemplateNodeType nodeType, IList<AttributeDefinition3> attributes, LineInfo lineInfo) {
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

            return ref templateNodes.array[idx];
        }

        public TemplateASTBuilder CreateRootNode(in RangeInt templateId, IList<AttributeDefinition3> attributes, LineInfo lineInfo) {
            rootNodes.Add(new TemplateASTRoot() {
                templateNameRange = templateId,
                slotDefinitionCount = 0,
                firstSlotDefinitionIndex = 0,
                templateIndex = templateNodes.size
            });

            AddTemplateNode(TemplateNodeType.Root, attributes, lineInfo);
            return new TemplateASTBuilder(templateNodes.size - 1, this, rootNodes.size - 1);
        }

        public void AddUsing(UsingDeclaration declaration) {

            CharSpan declSpan = GetCharSpan(declaration.namespaceRange);

            for (int i = 0; i < usings.size; i++) {
                UsingDeclaration usingDecl = usings.array[i];
                CharSpan value = GetCharSpan(usingDecl.namespaceRange);
                if (value == declSpan) {
                    return;
                }
            }

            usings.Add(declaration);
        }

        private TemplateASTBuilder CreateSlotNode(TemplateASTBuilder parentId, in CharSpan slotName, IList<AttributeDefinition3> attributes, LineInfo lineInfo, SlotType slotType) {
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

            ref TemplateASTNode node = ref AddTemplateNode(nodeType, attributes, lineInfo);

            node.parentId = parentId.index;

            RangeInt slotNameRange = AddString(slotName);
            node.moduleNameRange = slotNameRange;

            ref TemplateASTRoot root = ref rootNodes.array[parentId.rootId];

            slots = slots ?? new StructList<SlotAST>(4);

            if (root.slotDefinitionCount == 0) {
                root.firstSlotDefinitionIndex = slots.size;
            }

            root.slotDefinitionCount++;

            // when compiling we'll need to verify that the target node can support slots
            // parsing is super permissive, restrict in a later phase
            slots.Add(new SlotAST() {
                slotNameRange = slotNameRange,
                templateNodeId = nodeId,
                slotType = slotType
            });

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private unsafe RangeInt AddString(string span, bool attemptDeduplicate = false) {

            // todo -- we could de-dup this if we wanted to, exercise for later I think

            if (string.IsNullOrEmpty(span)) {
                return default;
            }

            int length = span.Length;

            charBuffer.EnsureAdditionalCapacity(length);
            fixed (char* str = span) {
                UnsafeUtility.MemCpy(charBuffer.GetArrayPointer() + charBuffer.size, str, length * sizeof(char));
            }

            RangeInt retn = new RangeInt(charBuffer.size, length);
            charBuffer.size += length;
            return retn;

        }

        public unsafe RangeInt AddString(in CharSpan span, bool attemptDeduplicate = false) {

            // todo -- we could de-dup this if we wanted to, exercise for later I think

            if (span.HasValue) {
                int length = span.Length;

                charBuffer.EnsureAdditionalCapacity(length);

                UnsafeUtility.MemCpy(charBuffer.GetArrayPointer() + charBuffer.size, span.data + span.rangeStart, length * sizeof(char));

                RangeInt retn = new RangeInt(charBuffer.size, length);
                charBuffer.size += length;
                return retn;
            }

            return default;
        }

        private TemplateASTBuilder CreateElementNode(TemplateASTBuilder parentId, in CharSpan moduleName, in CharSpan tagName, IList<AttributeDefinition3> attributes, LineInfo lineInfo) {
            int nodeId = templateNodes.size;

            AddChild(parentId.index, nodeId);

            ref TemplateASTNode node = ref AddTemplateNode(TemplateNodeType.Unresolved, attributes, lineInfo);

            node.parentId = parentId.index;

            node.moduleNameRange = AddString(moduleName);
            node.tagNameRange = AddString(tagName);

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private TemplateASTBuilder CreateTextNode(TemplateASTBuilder parentId, LineInfo lineInfo, IList<AttributeDefinition3> attributes, TemplateNodeType nodeType) {
            int nodeId = templateNodes.size;

            AddChild(parentId.index, nodeId);

            ref TemplateASTNode node = ref AddTemplateNode(nodeType, attributes, lineInfo);

            node.parentId = parentId.index;

            return new TemplateASTBuilder(nodeId, this, parentId.rootId);
        }

        private void SetTextContent(string textContent, LineInfo lineInfo) {
            // todo -- user api?
        }

        private void SetTextContent(StructList<char> textContent, LineInfo lineInfo) {
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

            templateNodes.array[nodeId].textContentRange = range;

            for (int i = range.start; i < range.end; i++) {
                textContents.Add(new TextContent() {
                    stringRange = AddString(textExpressions.array[i].text),
                    isExpression = textExpressions.array[i].isExpression,
                    lineInfo = lineInfo
                });
            }

        }

        private RangeInt AddAttributes(IList<AttributeDefinition3> attributes) {
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

            public TemplateASTBuilder AddSlotChild(in CharSpan slotName, IList<AttributeDefinition3> childAttributes, LineInfo lineInfo, SlotType slotType) {
                return templateFileShellBuilder.CreateSlotNode(this, slotName, childAttributes, lineInfo, slotType);
            }

            public TemplateASTBuilder AddElementChild(in CharSpan moduleName, in CharSpan tagName, IList<AttributeDefinition3> attributes, LineInfo lineInfo) {
                return templateFileShellBuilder.CreateElementNode(this, moduleName, tagName, attributes, lineInfo);
            }

            public void SetTextContent(StructList<char> textContent, LineInfo lineInfo) {
                templateFileShellBuilder.SetTextContent(textContent, lineInfo);
            }

            // public TemplateASTBuilder AddTextChild(IList<AttributeDefinition3> attributes, LineInfo templateLineInfo) {
            //     return templateFileShellBuilder.CreateTextNode(this, templateLineInfo, attributes, TemplateNodeType.Text);
            // }

            // public TemplateASTBuilder AddRepeatChild(List<AttributeDefinition2> attributes, LineInfo lineInfo, string genericTypeResolver, string requireChildTypeExpression) {
            //     return templateFileShellBuilder.CreateRepeatNode(this, attributes, lineInfo, genericTypeResolver, requireChildTypeExpression);
            // }

        }

        public CharSpan GetCharSpan(RangeInt key) {

            if (key.length == 0) {
                return default;
            }

            if (key.start < 0 || key.start > charBuffer.size) {
                return default;
            }

            if (key.end > charBuffer.size) {
                return default;
            }

            unsafe {
                return new CharSpan(charBuffer.GetArrayPointer(), key.start, key.end);
            }
        }

        public bool AddStyleSource(in CharSpan styleSource) {

            for (int i = 0; i < styles.size; i++) {
                if (styles.array[i].sourceBody.length > 0) {
                    return false;
                }
            }

            styles.Add(new StyleDeclaration() {
                sourceBody = AddString(styleSource)
            });

            return true;
        }

        public bool AddStyleReference(in RangeInt src, in RangeInt alias) {

            styles.Add(new StyleDeclaration() {
                path = src,
                alias = alias
            });

            return true;
        }

        internal bool StyleAliasIsDeclared(RangeInt alias) {
            if (styles.size <= 0 || alias.length <= 0) {
                return false;
            }
            
            CharSpan aliasSpan = GetCharSpan(alias);
            for (int i = 0; i < styles.size; i++) {
                RangeInt range = styles.array[i].alias;
                if (range.length > 0) {
                    CharSpan existingAlias = GetCharSpan(range);
                    if (existingAlias == aliasSpan) {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool StyleSourceIsReferenced(RangeInt src) {
            CharSpan srcSpan = GetCharSpan(src);

            for (int i = 0; i < styles.size; i++) {
                RangeInt range = styles.array[i].path;
                if (range.length > 0) {
                    CharSpan existingSrc = GetCharSpan(range);
                    if (existingSrc == srcSpan) {
                        return true;
                    }
                }
            }

            return false;
        }

    }

}