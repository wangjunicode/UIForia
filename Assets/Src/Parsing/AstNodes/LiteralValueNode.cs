namespace UIForia {

    public abstract class LiteralValueNode : ExpressionNode {

        protected LiteralValueNode(ExpressionNodeType expressionType) : base(expressionType) { }

        public override bool TypeCheck(ContextDefinition contextDefinition) {
            return true;
        }

    }

}