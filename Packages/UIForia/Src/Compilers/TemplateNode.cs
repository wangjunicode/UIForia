using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    [DebuggerDisplay("{processedType.rawType.ToString()}")]
    public sealed class TemplateNode {

        public LightList<DirectiveDefinition> directives;
        public StructList<AttributeDefinition2> attributes;
        public LightList<TemplateNode> children;
        public TemplateAST astRoot;
        public TemplateNode parent;
        public StructList<TextExpression> textContent;
        public ProcessedType processedType;
        public string slotName;

        [ThreadStatic] private static LightList<TemplateNode> s_Pool;
        public string originalString;

        public TemplateNode() {
            this.directives = new LightList<DirectiveDefinition>(4);
            this.attributes = new StructList<AttributeDefinition2>();
            this.children = new LightList<TemplateNode>();
        }

        public Type RootType => astRoot.root.processedType.rawType;

        public Type ElementType => processedType.rawType;
        
        internal static TemplateNode Get() {
            if (s_Pool == null) {
                s_Pool = new LightList<TemplateNode>(32);
            }

            if (s_Pool.Count != 0) {
                return s_Pool.RemoveLast();
            }

            return new TemplateNode();
        }


        internal static void Release(ref TemplateNode node) {
            if (s_Pool == null) {
                s_Pool = new LightList<TemplateNode>(32);
            }

            node.processedType = default;
            node.directives.Clear();
            node.attributes.Clear();
            node.children.Clear();
            node.parent = null;
            node.textContent = null;
            s_Pool.Add(node);
            node = null;
        }

        public bool HasAttribute(string attrName) {
            if (attributes == null) return false;
            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i].key == attrName) {
                    return true;
                }
            }

            return false;
        }

        public string GetStringContent() {
            string retn = "";
            if (textContent == null) return retn;

            for (int i = 0; i < textContent.Count; i++) {
                retn += textContent[i].text;
            }

            return retn;
        }

        public bool IsTextConstant() {
            if (textContent == null || textContent.size == 0) return false;

            for (int i = 0; i < textContent.Count; i++) {
                if (textContent.array[i].isExpression) {
                    return false;
                }
            }

            return true;
        }

        public TemplateNodeType GetTemplateType() {
            if (astRoot.root == this) {
                return TemplateNodeType.Root;
            }

            if (processedType.rawType == typeof(UISlotDefinition)) {
                return TemplateNodeType.SlotDefinition;
            }

            if (processedType.rawType == typeof(UISlotContent)) {
                return TemplateNodeType.SlotContent;
            }

            if (typeof(UIContainerElement).IsAssignableFrom(processedType.rawType)) {
                return TemplateNodeType.ContainerElement;
            }

            if ((typeof(UITextElement).IsAssignableFrom(processedType.rawType))) {
                return TemplateNodeType.TextElement;
            }

            return TemplateNodeType.HydrateElement;
        }

        public int GetAttributeCount() {
            int retn = 0;
            if (attributes == null) return 0;

            for (int i = 0; i < attributes.size; i++) {
                if (attributes[i].type == AttributeType.Attribute) {
                    retn++;
                }
            }

            return retn;
        }

        public int GetBindingCount() {
            int retn = 0;
            
            if (attributes != null) {

                for (int i = 0; i < attributes.size; i++) {
                    if (attributes[i].type == AttributeType.Property) {
                        retn++;
                    }
                    else if (attributes[i].type == AttributeType.Attribute && (attributes[i].flags & AttributeFlags.Const) == 0) {
                        retn++;
                    }
                }
            }

            if (processedType.requiresUpdateFn) {
                retn++;
            }

            return retn;
        }

    }

}