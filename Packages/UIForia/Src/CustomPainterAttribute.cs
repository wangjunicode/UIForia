using System;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomPainterAttribute : Attribute {

        public readonly string name;

        public CustomPainterAttribute(string name) {
            this.name = name;
        }

    }

}