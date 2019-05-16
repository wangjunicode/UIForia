using System.Runtime.InteropServices;
using UnityEngine;

namespace Vertigo {

    [StructLayout(LayoutKind.Explicit)]
    internal struct MaterialProperty {

        [FieldOffset(0)] public readonly MaterialPropertyType type;
        [FieldOffset(4)] public readonly int key;
        [FieldOffset(8)] public readonly Texture textureValue;
        [FieldOffset(12)] public readonly Color32 colorValue;
        [FieldOffset(12)] public readonly float floatVal;
        [FieldOffset(12)] public readonly int intVal;
        [FieldOffset(12)] public readonly Vector4 vectorValue;

        public MaterialProperty(int key, Texture value) {
            this.type = MaterialPropertyType.Texture;
            this.key = key;
            this.colorValue = default;
            this.floatVal = default;
            this.intVal = default;
            this.vectorValue = default;
            this.textureValue = value;
        }

        public MaterialProperty(int key, float value) {
            this.type = MaterialPropertyType.Float;
            this.key = key;
            this.colorValue = default;
            this.intVal = default;
            this.vectorValue = default;
            this.textureValue = default;
            this.floatVal = value;
        }

        public MaterialProperty(int key, int value) {
            this.type = MaterialPropertyType.Int;
            this.key = key;
            this.colorValue = default;
            this.floatVal = default;
            this.vectorValue = default;
            this.textureValue = default;
            this.intVal = value;
        }

        public MaterialProperty(int key, Color32 value) {
            this.type = MaterialPropertyType.Color;
            this.key = key;
            this.floatVal = default;
            this.intVal = default;
            this.vectorValue = default;
            this.textureValue = default;
            this.colorValue = value;
        }

        public MaterialProperty(int key, Vector4 value) {
            this.type = MaterialPropertyType.Vector;
            this.key = key;
            this.colorValue = default;
            this.floatVal = default;
            this.intVal = default;
            this.textureValue = default;
            this.vectorValue = value;
        }

    }

}