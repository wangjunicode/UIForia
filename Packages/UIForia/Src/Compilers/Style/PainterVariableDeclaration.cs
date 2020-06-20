using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Compilers.Style {

    [StructLayout(LayoutKind.Explicit)]
    public struct PainterVariableDeclaration {

        [FieldOffset(0)] public Type type;
        [FieldOffset(8)] public string name;
        [FieldOffset(16)] public object objectValue;
        [FieldOffset(24)] public int propertyId;
        [FieldOffset(28)] public float floatValue;
        [FieldOffset(28)] public int intValue;
        [FieldOffset(28)] public Color32 colorValue;
        [FieldOffset(28)] public float2 float2Value;

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
        
        public PainterVariableDeclaration(Type propertyType, string propertyName, Color32 colorValue) : this() {
            this.type = propertyType;
            this.name = propertyName;
            this.colorValue = colorValue;
        }

        public PainterVariableDeclaration(Type propertyType, string propertyName, float2 vector2Value) : this() {
            this.type = propertyType;
            this.name = propertyName;
            this.float2Value = vector2Value;
        }
        
        public PainterVariableDeclaration(Type propertyType, string propertyName, Texture textureValue) : this() {
            this.type = propertyType;
            this.name = propertyName;
            this.objectValue = textureValue;
        }
        
    }

}