using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {


    public class StrokeVertexData {

        public readonly Mesh mesh;
        
        public readonly LightList<Vector3> position;
        public readonly LightList<Vector4> prevNext;
        public readonly LightList<Vector4> flags;
        public readonly LightList<Vector2> texCoords;
        public readonly LightList<Color> colors;
        public readonly LightList<int> triangles;
        public int triangleIndex;

        private readonly List<Vector3> positionList;
        private readonly List<Vector4> prevNextList;
        private readonly List<Vector4> flagsList;
        private readonly List<Color> colorsList;
        private readonly List<Vector2> texCoordList;
        private readonly List<int> trianglesList;
        
        public StrokeVertexData() {
            this.position = new LightList<Vector3>(32);
            this.prevNext = new LightList<Vector4>(32);
            this.flags = new LightList<Vector4>(32);
            this.colors = new LightList<Color>(32);
            this.texCoords = new LightList<Vector2>(32);
            this.triangles = new LightList<int>(32 * 3);
            
            // until mesh gets better array support this should ensure we don't allocate
            this.positionList = new List<Vector3>(32);
            this.prevNextList = new List<Vector4>(32);
            this.flagsList = new List<Vector4>(32);
            this.colorsList = new List<Color>(32);
            this.texCoordList = new List<Vector2>(32);
            this.trianglesList = new List<int>(32 * 3);
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Clear() {
            // does not clear the mesh on purpose so this object can be pooled and re-used next frame
            position.Clear();
            texCoords.Clear();
            prevNext.Clear();
            flags.Clear();
            colors.Clear();
            triangles.Clear();
            triangleIndex = 0;
        }

        public void EnsureAdditionalCapacity(int vertexCount) {
            position.EnsureAdditionalCapacity(vertexCount);
            prevNext.EnsureAdditionalCapacity(vertexCount);
            flags.EnsureAdditionalCapacity(vertexCount);
            colors.EnsureAdditionalCapacity(vertexCount);
            texCoords.EnsureAdditionalCapacity(vertexCount);
            triangles.EnsureAdditionalCapacity(vertexCount * 3);
        }

        public Mesh FillMesh() {
            mesh.Clear();

            int posCount = position.Count;
            for (int i = 0; i < posCount; i++) {
                Vector3 p = position[i];
                positionList.Add(new Vector3(p.x, -p.y, p.z));
            }
            
            for (int i = 0; i < prevNext.Count; i++) {
                Vector4 p = prevNext[i];
                prevNextList.Add(new Vector4(p.x, -p.y, p.z, -p.w));
            }
            
            mesh.SetVertices(positionList);
            mesh.SetColors(colors.ToList(colorsList));
            mesh.SetUVs(0, texCoords.ToList(texCoordList));
            mesh.SetUVs(1, flags.ToList(flagsList));
            mesh.SetUVs(2, prevNextList);
            mesh.SetTriangles(triangles.ToList(trianglesList), 0);
            
            positionList.Clear();
            prevNextList.Clear();
            flagsList.Clear();
            colorsList.Clear();
            texCoordList.Clear();
            trianglesList.Clear();

            return mesh;
        }

    }

}