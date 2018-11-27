namespace UIForia.Parsing {

    public class IdentifierNode : ASTNode {

        public string name;
        
        public override void Release() {
            s_IdentifierPool.Release(this);
        }

    }

}