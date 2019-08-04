using UIForia.Rendering.Vertigo;
using UnityEngine;

namespace UIForia.Rendering {

    internal struct Batch {

        public int drawCallSize;
        public BatchType batchType;
        public PooledMesh pooledMesh;
        public Mesh unpooledMesh;
        public Material material;
        public UIForiaData uiforiaData;
        public UIForiaPropertyBlock uiforiaPropertyBlock;

    }

}