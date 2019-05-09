namespace UIForia.Parsing.Style.AstNodes {

    public class AnimationOptionNode : StyleASTNode {

        public string  optionName;
        public StyleASTNode value;

        public override void Release() {
            value.Release();
        }

    }

}