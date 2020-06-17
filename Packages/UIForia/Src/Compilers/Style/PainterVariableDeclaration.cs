using System;
using System.Runtime.InteropServices;

namespace UIForia.Compilers.Style {

    [StructLayout(LayoutKind.Explicit)]
    public struct PainterVariableDeclaration {

        [FieldOffset(0)] public Type type;
        [FieldOffset(8)] public string name;
        [FieldOffset(16)] public object value;
        [FieldOffset(24)] public int propertyId;
        [FieldOffset(28)] public float floatValue;
        [FieldOffset(28)] public int intValue;

        public PainterVariableDeclaration(Type propertyType, string propertyName, float floatValue) : this() {
            this.type = propertyType;
            this.name = propertyName;
            this.floatValue = floatValue;
        }

        public PainterVariableDeclaration(Type propertyType, string propertyName, int intValue) : this() {
            this.type = propertyType;
            this.name = propertyName;
            this.intValue = intValue;
        }

    }

}