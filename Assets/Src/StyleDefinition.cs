namespace Src {

    public struct StyleDefinition {

        internal const string k_EmptyAliasName = "base";
        
        public readonly string alias;
        public readonly string classPath;

        public StyleDefinition(string alias, string classPath) {
            this.alias = alias;
            this.classPath = classPath;
        }
        
    }

}