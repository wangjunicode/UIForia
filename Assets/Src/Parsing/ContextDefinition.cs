namespace Src {

    public class ContextDefinition {

        public readonly string name;
        public readonly ProcessedType type;
        public string alias;
        public string indexAlias;
        public string lengthAlias;
        
        public ContextDefinition(string name, ProcessedType type) {
            this.name = name;
            this.type = type;
        }    

    }

}