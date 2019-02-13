namespace UIForia.Parsing.Style.AstNodes {

    public class StyleIdentifierNode : StyleASTNode {

        public string name;
        
        public bool IsAlias => name[0] == '$';

        public override void Release() {
            s_IdentifierPool.Release(this);
        }

        protected bool Equals(StyleIdentifierNode other) {
            return string.Equals(name, other.name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StyleIdentifierNode) obj);
        }

        public override int GetHashCode() {
            return (name != null ? name.GetHashCode() : 0);
        }

        public override string ToString() {
            return $"StyleIdentifierNode[{name}]";
        }
    }

}