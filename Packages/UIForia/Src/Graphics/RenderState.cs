using UnityEngine;

namespace Vertigo {

    public struct RenderState {

        public Color32 fillColor;
        public Color32 strokeColor;
        public float strokeWidth;
        public Rect uvRect;
        public Vector2 uvTiling;
        public Vector2 uvOffset;
        public float uvRotation;
        public Vector2 uvPivot;
        public VertexChannel sdfDataChannel;
        public VertexChannel texCoordChannel;
        public float defaultZ;
            
    }

}