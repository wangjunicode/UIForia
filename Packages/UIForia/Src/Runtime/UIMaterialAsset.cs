using System;
using UnityEngine;

namespace UIForia {

    public class UIMaterialAsset : ScriptableObject {

        // todo -- maybe default pipeline states?
        
        public string shaderFile;
        public string vertexShaderName;
        public string fragmentShaderName;
        
        public ShaderDefine[] defines;
        
        public bool manipulatesVertices;
        public MaterialPropertyMapping[] properties;

    }

    [Serializable]
    public struct ShaderDefine {

        public string key;
        public string value;

    }

    public enum MaterialPropertyType : byte {

        Float,
        Vector,
        Matrix,
        Texture

    }

    [Serializable]
    public struct MaterialPropertyMapping {

        public string propertyName;
        public MaterialPropertyType type;
        public string propertyPath;
        public string defaultValue;

    }

}