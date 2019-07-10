using System;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateNode {

        public LightList<DirectiveDefinition> directives;
        public StructList<AttributeDefinition2> attributes;
        public LightList<TemplateNode> children;
        public TemplateAST astRoot;
        public TemplateNode parent;
        public string textContent;
        public ProcessedType processedType;
        public string slotName;

        [ThreadStatic] private static LightList<TemplateNode> s_Pool;

        public TemplateNode() {
            this.directives = new LightList<DirectiveDefinition>(4);
            this.attributes = new StructList<AttributeDefinition2>();
            this.children = new LightList<TemplateNode>();
        }

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

    }

}