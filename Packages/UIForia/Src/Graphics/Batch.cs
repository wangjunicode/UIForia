using UIForia.Rendering.Vertigo;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

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

        public int meshId;
        
        public RangeInt memberIdRange;
        public VertexLayout vertexLayout;
        public int vertexChannelCount;
        public int vertexCount;
        public int triangleCount;
        public VertexChannelDesc* geometry;
        public int* triangles;
        public MaterialId materialId;
        
        public MaterialPropertyOverride* propertyOverrides;
        public int propertyOverrideCount;

        public int clipRectCount;
        public AxisAlignedBounds2D* clipRects;
        
        public byte stencilRefValue;
        public StencilState stencilState;
        public ColorWriteMask colorMask;
        
        public MaterialPermutation materialPermutation;
        public BatchType type;
        public int indirectArgOffset;

        public bool HasUIForiaMaterial() {
            return true;
        }
        
    }

}