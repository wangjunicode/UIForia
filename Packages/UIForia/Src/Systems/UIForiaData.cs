using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    
    public class UIForiaData {

        public FontData fontData;
        public StructList<Vector4> colors = new StructList<Vector4>();
        public StructList<Vector4> objectData0 = new StructList<Vector4>();
        public StructList<Vector4> objectData1 = new StructList<Vector4>();
        public StructList<Vector4> clipUVs = new StructList<Vector4>();
        public StructList<Vector4> clipRects = new StructList<Vector4>();
        public StructList<Matrix4x4> transformData = new StructList<Matrix4x4>();
        public Texture mainTexture;
        public Texture clipTexture;

    }

}