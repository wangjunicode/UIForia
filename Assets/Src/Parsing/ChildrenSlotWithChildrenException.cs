using System;

namespace Src {

    public class ChildrenSlotWithChildrenException : Exception {

        public ChildrenSlotWithChildrenException(string typeName) : base("<Children/> elements cannot have children and must be self closing. See " + typeName) { }

    }

}