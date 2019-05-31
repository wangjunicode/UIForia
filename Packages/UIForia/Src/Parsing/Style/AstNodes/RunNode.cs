namespace UIForia.Parsing.Style.AstNodes {

    public class RunNode : StyleASTNode {

        public StyleASTNode commmand;

        public override void Release() {
            commmand.Release();
        }

    }

}