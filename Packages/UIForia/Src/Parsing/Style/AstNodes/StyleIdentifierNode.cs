namespace UIForia.Style.Parsing {

    public class IdentifierNode : StyleASTNode {

        public string name;
        
        public bool IsAlias => name[0] == '$';

        public override void Release() {
            s_IdentifierPool.Release(this);
        }

        protected bool Equals(IdentifierNode other) {
            return string.Equals(name, other.name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierNode) obj);
        }

        public override int GetHashCode() {
            return (name != null ? name.GetHashCode() : 0);
        }
    }

}