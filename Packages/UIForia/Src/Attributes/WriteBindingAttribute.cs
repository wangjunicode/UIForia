using System;

namespace UIForia.Elements {

    [AttributeUsage(AttributeTargets.Event)]
    public class WriteBindingAttribute : System.Attribute {

        public string propertyName;

        public WriteBindingAttribute(string propertyName) {
            this.propertyName = propertyName;
        }

    }

}