using UnityEngine;

namespace Vertigo {

    public struct RenderState {

        public Color32 fillColor;
        public Color32 strokeColor;
        public Rect uvRect;
        public Vector2 uvTiling;
        public Vector2 uvOffset;
        public float uvRotation;
        public TextureCoordChannel sdfDataChannel;
        public TextureCoordChannel texCoordChannel;
        // public float4x4 transform;
            
    }

}