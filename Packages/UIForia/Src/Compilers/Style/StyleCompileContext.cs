using System.Collections.Generic;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleCompileContext {

        public Dictionary<string, ConstNode> constNodes = new Dictionary<string, ConstNode>();
        public Dictionary<string, LightList<StyleConstant>> importedStyleConstants = new Dictionary<string, LightList<StyleConstant>>();

        public Dictionary<string, StyleConstant> constantsWithReferences = new Dictionary<string, StyleConstant>();
        public LightList<StyleConstant> constants = LightListPool<StyleConstant>.Get();
        public LightList<UIStyleGroup> importedGroups = LightListPool<UIStyleGroup>.Get();

        public void Release() {
            LightListPool<StyleConstant>.Release(ref constants);
            LightListPool<UIStyleGroup>.Release(ref importedGroups);
            constNodes.Clear();
            constantsWithReferences.Clear();
        }

        public StyleASTNode GetValueForReference(StyleASTNode node) {
            if (node is ReferenceNode referenceNode) {
                foreach (var c in constants) {
                    if (c.name == referenceNode.referenceName) {
                        return c.value;
                    }
                }

                throw new CompileException(referenceNode, $"Couldn't resolve reference {referenceNode}");
            }

            return node;
        }

        internal string PrintConstants() {
            if (constants.Count == 0) return string.Empty;

            string result = constants[0].name;
            for (int index = 1; index < constants.Count; index++) {
                result += ", " + constants[index].name;
            }

            return result;
        }
    }
}
