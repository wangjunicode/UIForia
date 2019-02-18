using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
        
        internal static readonly ObjectPool<UrlNode> s_UrlNodePool = new ObjectPool<UrlNode>();

        internal static UrlNode UrlNode(StyleASTNode url) {
            UrlNode retn = s_UrlNodePool.Get();
            retn.url = url;
            return retn;
        }
    }

    public class UrlNode : StyleASTNode {

        public StyleASTNode url;

        public UrlNode() {
            type = StyleASTNodeType.Url;
        }

        public override void Release() {
            StyleASTNodeFactory.s_UrlNodePool.Release(this);
        }

        public override string ToString() {
            return $"url({url})";
        }
    }
}
