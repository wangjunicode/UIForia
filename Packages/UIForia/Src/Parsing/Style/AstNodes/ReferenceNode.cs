using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ReferenceNode> s_ReferenceNodePool = new ObjectPool<ReferenceNode>();

        internal static ReferenceNode ReferenceNode(string value) {
            ReferenceNode referenceNode = s_ReferenceNodePool.Get();
            referenceNode.referenceName = value;
            return referenceNode;
        }
    }

    public class ReferenceNode : StyleGroupContainer {

        public string referenceName;

        public ReferenceNode() {
            type = StyleASTNodeType.Reference;
        }

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_ReferenceNodePool.Release(this);
        }

        protected bool Equals(ReferenceNode other) {
            return string.Equals(referenceName, other.referenceName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReferenceNode) obj);
        }

        public override int GetHashCode() {
            return (referenceName != null ? referenceName.GetHashCode() : 0);
        }
    }
}
