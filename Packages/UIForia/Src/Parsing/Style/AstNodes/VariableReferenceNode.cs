namespace UIForia.Parsing.Style.AstNodes {

    public class VariableReferenceNode : StyleNodeContainer {

        public string identifier;

        public VariableReferenceNode(string identifier) {
            this.identifier = identifier;
        }
        
        public override void Release() {
            throw new System.NotImplementedException();
        }

    }

}