using UIForia.Rendering.Vertigo;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    internal struct Batch {

        public int drawCallSize;
        public BatchType batchType;
        public PooledMesh pooledMesh;
        public MaterialPropertyBlock propertyBlock;
        public Mesh unpooledMesh;
        public Material material;
        public UIForiaData uiforiaData;
        public StructList<Matrix4x4> transformData;
        public int renderStateId;

    }

}

namespace UIForia.Graphics {

    public unsafe struct Batch {

        public RangeInt memberIdRange;
        public int meshId;
        public int transformIndex;
        public VertexLayout vertexLayout;
        public int vertexChannelCount;
        public int vertexCount;
        public int triangleCount;
        public VertexChannelDesc* geometry;
        public int* triangles;

    }

}