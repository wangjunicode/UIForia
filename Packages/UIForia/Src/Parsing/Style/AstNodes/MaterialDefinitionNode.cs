namespace UIForia.Parsing.Style.AstNodes {

    public class MaterialDefinitionNode : StyleASTNode {

        public string materialName;
        public AssetLoadMethod loadMethod;
        public string assetLoadPath;
        public string body;

        public override void Release() { }

    }

}