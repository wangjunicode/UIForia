using System.Reflection;

namespace UIForia {

    internal struct ElementTypeInfo {

        public MethodInfo onCreate;
        public MethodInfo onDestroy;
        public MethodInfo onEnable;
        public MethodInfo onDisable;
        public MethodInfo onUpdate;
        public MethodInfo onPostUpdate;
        public MethodInfo[] methods;

        public ConstructorInfo defaultConstructor;
        // other life cycle methods / event handling methods 

    }

}