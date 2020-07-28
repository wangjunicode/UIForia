using System;
using System.Runtime.InteropServices;
using UIForia.Rendering;
using UIForia.Text;
using UnityEngine;

namespace UIForia {

    // [StructLayout(LayoutKind.Sequential)]
    // public struct MaterialProperty {
    //
    //     // this must align with StyleProperty.object
    //     public MaterialPropertyValue2 value;
    //     public MaterialPropertyType propertyType;
    //     public MaterialId materialId;
    //     public int stylePropertyId;
    //     public int shaderPropertyKey;
    //     public string propertyName;
    //     public int shaderPropertyId;
    //
    // }

    [Serializable]
    public struct MaterialReference {

        public string name;
        public Material material;
        public bool isShared;

    }

    public enum MaterialPropertyType {

        Color,
        Float,
        Vector,
        Range,
        Texture,

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialPropertyValue2 {
        
        [FieldOffset(0)] public Texture texture;
        [FieldOffset(8)] public float floatValue;
        [FieldOffset(8)] public Color colorValue;
        [FieldOffset(8)] public Vector4 vectorValue;

    }
    

    public struct MaterialPropertyDefinition {

        public string propertyName;
        public int shaderPropertyId;
        public int stylePropertyId;
        public MaterialPropertyType propertyType;

    }

    public class UIForiaAssets : MonoBehaviour {

        public float centerX;
        public float centerY;

        public MaterialReference[] materialReferences;

        private void Update() {


        }

    }

}