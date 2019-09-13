using UIForia.Rendering;

namespace UIForia.Parsing.Style.AstNodes {
    public class AnimationCommandNode : CommandNode {

        public StyleASTNode animationName;
        public bool isExit;
        public RunAction runAction;

        public override void Release() {
            animationName.Release();
        }
    }
}
