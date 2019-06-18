using System;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomScrollbarAttribute : Attribute {

        public readonly string name;

        public CustomScrollbarAttribute(string name) {
            this.name = name;
        }

    }

}