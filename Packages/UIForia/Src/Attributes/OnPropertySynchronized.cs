using System;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnPropertySynchronized : Attribute {

        public readonly string propertyName;

        public OnPropertySynchronized(string propertyName) {
            this.propertyName = propertyName;
        }

    }

}