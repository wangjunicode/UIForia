
namespace UIForia {

    public struct StyleDefinition {

        internal const string k_EmptyAliasName = "__base__";
        
        public readonly string alias;
        public readonly string importPath;
        public readonly string body;
        
        public StyleDefinition(string alias, string importPath,  string body = null) {
            this.alias = alias;
            this.importPath = importPath;
            this.body = body;
        }
        
    }

}