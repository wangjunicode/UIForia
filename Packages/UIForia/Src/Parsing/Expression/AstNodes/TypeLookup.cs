namespace UIForia.Parsing.Expression.AstNodes {

    public struct TypeLookup {

        public string typeName;
        public string namespaceName;
        public TypeLookup[] generics;

        public override string ToString() {
            string retn = "";
            if (!string.IsNullOrEmpty(namespaceName) && !string.IsNullOrWhiteSpace(namespaceName)) {
                retn += namespaceName + ".";
            }

            retn += typeName;
            if (generics != null && generics.Length > 0) {
                retn += "`" + generics.Length + "[";
                for (int i = 0; i < generics.Length; i++) {
                    retn += generics[i].ToString();
                    if (i != generics.Length) {
                        retn += ",";
                    }
                }

                retn += "]";
            }

            return retn;
        }

    }

}