using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    public abstract class StyleGroupContainer : StyleASTNode {

        public string identifier;
        public readonly LightList<StyleASTNode> children;

        public StyleGroupContainer() {
            this.children = new LightList<StyleASTNode>(2);
        }

        public void AddChildNode(StyleASTNode child) {
            children.Add(child);
        }

        public override void Release() {
            for (int index = 0; index < children.Count; index++) {
                children[index].Release();
            }

            children.Clear();
        }
        
        public override string ToString() {
            return $"{identifier} = {children}";
        }

    }

}