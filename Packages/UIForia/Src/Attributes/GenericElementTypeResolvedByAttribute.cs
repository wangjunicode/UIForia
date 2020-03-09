using System;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class GenericElementTypeResolvedByAttribute : Attribute {

        public readonly string propertyName;

        public GenericElementTypeResolvedByAttribute(string propertyName) {
            this.propertyName = propertyName;
        }

    }

}