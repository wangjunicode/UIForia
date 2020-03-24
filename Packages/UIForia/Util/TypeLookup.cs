using System;

namespace UIForia.Util {

    public struct TypeLookup {

        public string typeName;
        public string namespaceName;
        public SizedArray<TypeLookup> generics;
        public bool isArray;
        public Type resolvedType;

        public TypeLookup(string typeName) {
            this.typeName = typeName;
            this.namespaceName = null;
            this.generics = default;
            this.isArray = false;
            this.resolvedType = null;
        }
        
        
        public TypeLookup(Type type) {
            this.resolvedType = type;
            this.typeName = null;
            this.namespaceName = null;
            this.generics = default;
            this.isArray = false;
        }

        public void Release() {
            typeName = null;
            namespaceName = null;
            generics = default;
        }

        public string GetBaseTypeName() {
            if (generics.array == null || generics.size == 0) {
                return typeName;
            }

            return typeName + "`" + generics.size;
        }
        
        public override string ToString() {
            string retn = "";
            
            if (!string.IsNullOrEmpty(namespaceName) && !string.IsNullOrWhiteSpace(namespaceName)) {
                retn += namespaceName + ".";
            }

            retn += typeName;
            if (generics.array != null && generics.size > 0) {
                retn += "[";
                for (int i = 0; i < generics.size; i++) {
                    retn += generics[i].ToString();
                    if (i != generics.size - 1) {
                        retn += ",";
                    }
                }

                retn += "]";
            }

            return retn;
        }

    }

}