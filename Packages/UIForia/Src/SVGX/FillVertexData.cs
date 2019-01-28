using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class FillVertexData {

        public int triangleIndex;
        public readonly LightList<Vector3> position;
        public readonly LightList<Color> colors;
        public readonly LightList<int> triangles;
        public readonly LightList<Vector2> texCoords;

        public FillVertexData() {
            position = new LightList<Vector3>(32);
            colors = new LightList<Color>(32);
            texCoords = new LightList<Vector2>(32);
            triangles = new LightList<int>(32 * 3);
        }

        public void Clear() {
            texCoords.Clear();
            position.Clear();
            colors.Clear();
            triangles.Clear();
            triangleIndex = 0;
        }
    }


}