using UIForia.Util;

namespace UIForia.Parsing.Expressions.AstNodes {

    public class StringInterpolationNode : ASTNode {

        public string rawValue;
        public StructList<StringInterpolationPart> parts;

        public override void Release() {
            parts?.Release();
            s_StringInterpolationPool.Release(this);
        }

    }

}