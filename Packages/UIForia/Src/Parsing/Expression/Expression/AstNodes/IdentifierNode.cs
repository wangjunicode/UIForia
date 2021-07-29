namespace UIForia.Parsing.Expressions.AstNodes {

    public class IdentifierNode : ASTNode {

        public string name;
        public string secondary;

        public bool IsAlias => name[0] == '$';

        public override void Release() {
            secondary = null;
            name = null;
            s_IdentifierPool.Release(this);
        }

    }

}