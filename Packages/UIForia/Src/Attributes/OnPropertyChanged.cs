using System;

namespace UIForia.Attributes {

    public enum PropertyChangeSource {

        BindingRead,
        Synchronized,
        Initialized

    }
    
    [Flags]
    public enum PropertyChangedType {

        BindingRead = 1,
        Synchronized = 2,
        All = BindingRead | Synchronized
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnPropertySynchronized : OnPropertyChanged {

        public OnPropertySynchronized(string propertyName) : base(propertyName, PropertyChangedType.Synchronized) { }

    }    
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnPropertyChanged : Attribute {

        public readonly string propertyName;
        public readonly PropertyChangedType changedType;
        
        public OnPropertyChanged(string propertyName, PropertyChangedType changedType = PropertyChangedType.BindingRead) {
            this.propertyName = propertyName;
            this.changedType = changedType;
        }

    }

}