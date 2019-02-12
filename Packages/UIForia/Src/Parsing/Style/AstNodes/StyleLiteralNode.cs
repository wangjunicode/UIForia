namespace UIForia.Style.Parsing {

    public class LiteralNode : StyleASTNode {

        public string rawValue;

        public override void Release() {
            rawValue = null;
            type = StyleASTNodeType.Invalid;
            s_LiteralPool.Release(this);
        }

        protected bool Equals(LiteralNode other) {
            return string.Equals(rawValue, other.rawValue);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LiteralNode) obj);
        }

        public override int GetHashCode() {
            return (rawValue != null ? rawValue.GetHashCode() : 0);
        }
    }

}