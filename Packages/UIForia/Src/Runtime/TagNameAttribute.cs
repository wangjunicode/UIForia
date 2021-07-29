using System;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public class TagNameAttribute : Attribute {

        public string moduleName;
        public string tagName;

        public TagNameAttribute(string moduleName, string tagName) {
            this.moduleName = moduleName;
            this.tagName = tagName;
        }

    }

}