using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ContainerElementAttribute : Attribute {

        public readonly string filePath;

#if UNITY_EDITOR
        public ContainerElementAttribute([CallerFilePath] string DO_NOT_USE = "") {
            this.filePath = DO_NOT_USE;
        }
#else
        public ContainerElementAttribute() {
        }
#endif

    }

}