using System;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ResolveTypesUsingAncestor : Attribute {
        public Type ancestorType;

        public ResolveTypesUsingAncestor(Type ancestorType) {
            this.ancestorType = ancestorType;
        }
    }
}