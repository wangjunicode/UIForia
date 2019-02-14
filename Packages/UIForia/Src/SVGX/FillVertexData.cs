using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class FillVertexData {

        public int triangleIndex;
        public readonly LightList<Vector3> position;
        public readonly LightList<Color> colors;
        public readonly LightList<int> triangles;
        public readonly LightList<Vector4> texCoords;
        public readonly LightList<Vector4> flags;
        public readonly LightList<Vector4> fillFlags;
        public readonly Mesh mesh;
        
        private readonly List<Vector3> positionList;
        private readonly List<Vector4> fillFlagList;
        private readonly List<Vector4> flagsList;
        private readonly List<Color> colorsList;
        private readonly List<Vector4> texCoordList;
        private readonly List<int> trianglesList;
        
        public FillVertexData() {
            position = new LightList<Vector3>(32);
            colors = new LightList<Color>(32);
            texCoords = new LightList<Vector4>(32);
            flags = new LightList<Vector4>(32);
            fillFlags = new LightList<Vector4>(32);
            triangles = new LightList<int>(32 * 3);
            
            this.positionList = new List<Vector3>(32);
            this.fillFlagList = new List<Vector4>(32);
            this.flagsList = new List<Vector4>(32);
            this.colorsList = new List<Color>(32);
            this.texCoordList = new List<Vector4>(32);
            this.trianglesList = new List<int>(32 * 3);
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Clear() {
            texCoords.Clear();
            position.Clear();
            colors.Clear();
            triangles.Clear();
            flags.Clear();
            fillFlags.Clear();
            triangleIndex = 0;
        }

        // todo -- remove list allocation
        public Mesh FillMesh() {
            // do we really need to clear? 
            mesh.Clear(true);
            
            int posCount = position.Count;
            positionList.Clear();
            for (int i = 0; i < posCount; i++) {
                Vector3 p = position[i];
                positionList.Add(new Vector3(p.x, -p.y, p.z));
            }

            mesh.SetVertices(positionList);
            mesh.SetColors(colors.ToList(colorsList));
            mesh.SetUVs(0, texCoords.ToList(texCoordList));
            mesh.SetUVs(1, flags.ToList(flagsList));
            mesh.SetUVs(2, fillFlags.ToList(fillFlagList));
            mesh.SetTriangles(triangles.ToList(trianglesList), 0);
            
            positionList.Clear();
            fillFlagList.Clear();
            flagsList.Clear();
            colorsList.Clear();
            texCoordList.Clear();
            trianglesList.Clear();
            
            return mesh;
        }

        public void EnsureAdditionalCapacity(int size) {
            position.EnsureAdditionalCapacity(size);
            colors.EnsureAdditionalCapacity(size);
            texCoords.EnsureAdditionalCapacity(size);
            flags.EnsureAdditionalCapacity(size);
            fillFlags.EnsureAdditionalCapacity(size);
            triangles.EnsureAdditionalCapacity(size * 3);
        }

    }


}