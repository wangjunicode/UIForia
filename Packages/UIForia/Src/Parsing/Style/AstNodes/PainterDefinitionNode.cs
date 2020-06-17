namespace UIForia.Parsing.Style.AstNodes {

    public class PainterDefinitionNode : StyleASTNode {

        public string painterName;
        public string shapeBody;
        public bool isCompiled;

        public override void Release() { }

    }

}