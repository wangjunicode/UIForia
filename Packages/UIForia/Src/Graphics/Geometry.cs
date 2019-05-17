using System;
using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    public class Geometry {

        // Note: Shapes should provide a triangle list relative to their own vertices.
        // this means that if a shape defines a triangle for the first vertex in it's
        // list then that triangle should be 0, not positionList.size - shape.vertexCount

        public StructList<Vector3> positionList;
        public StructList<Vector3> normalList;
        public StructList<Color> colorList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<Vector4> texCoordList2;
        public StructList<Vector4> texCoordList3;
        public StructList<int> triangleList;

        public int vertexCount => positionList.size;
        public int triangleCount => triangleList.size;

        public Geometry(int capacity = 8) {
            positionList = new StructList<Vector3>(capacity);
            normalList = new StructList<Vector3>(capacity);
            colorList = new StructList<Color>(capacity);
            texCoordList0 = new StructList<Vector4>(capacity);
            texCoordList1 = new StructList<Vector4>(capacity);
            texCoordList2 = new StructList<Vector4>(capacity);
            texCoordList3 = new StructList<Vector4>(capacity);
            triangleList = new StructList<int>(capacity * 2);
        }

        public void EnsureAdditionalCapacity(int vertCount, int triCount) {
            int requiredSize = positionList.size + vertCount;

            if (requiredSize >= positionList.array.Length) {
                requiredSize *= 2;
                Array.Resize(ref positionList.array, requiredSize);
                Array.Resize(ref normalList.array, requiredSize);
                Array.Resize(ref texCoordList0.array, requiredSize);
                Array.Resize(ref texCoordList1.array, requiredSize);
                Array.Resize(ref texCoordList2.array, requiredSize);
                Array.Resize(ref texCoordList3.array, requiredSize);
                Array.Resize(ref colorList.array, requiredSize);
            }

            triangleList.EnsureAdditionalCapacity(triCount);
        }

        public void UpdateSizes(int vertCount, int triCount) {
            positionList.size += vertCount;
            normalList.size += vertCount;
            colorList.size += vertCount;
            texCoordList0.size += vertCount;
            texCoordList1.size += vertCount;
            texCoordList2.size += vertCount;
            texCoordList3.size += vertCount;
            triangleList.size += triCount;
        }
        
        public virtual void ClearGeometryData() {
            positionList.size = 0;
            normalList.size = 0;
            colorList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            texCoordList2.size = 0;
            texCoordList3.size = 0;
            triangleList.size = 0;
        }

    }

}