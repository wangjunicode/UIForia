using System;

namespace UIForia.Layout {

    [AttributeUsage(AttributeTargets.Class)]
    public class LayoutBoxName : Attribute {

        public readonly string name;

        public LayoutBoxName(string name) {
            this.name = name;
        }

    }

}