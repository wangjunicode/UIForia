using UIForia.Util;

namespace UIForia.Style.Parsing {

    public abstract class StyleGroupContainer : StyleASTNode {
        public string identifier;
        public LightList<StyleASTNode> children { get; private set; }

        public StyleGroupContainer() {
            this.children = LightListPool<StyleASTNode>.Get();
        }

        public void AddChildNode(StyleASTNode child) {
            children.Add(child);
        }

        public override void Release() {
            for (int index = 0; index < children.Count; index++) {
                StyleASTNode child = children[index];
                child.Release();
            }

            children.Clear();
            children = null;
        }
    }
}
