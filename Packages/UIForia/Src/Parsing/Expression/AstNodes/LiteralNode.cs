namespace UIForia.Parsing.Expression.AstNodes {

    public class LiteralNode : ASTNode {

        public string rawValue;

        public override void Release() {
            rawValue = null;
            type = ASTNodeType.Invalid;
            s_LiteralPool.Release(this);
        }
    }

}