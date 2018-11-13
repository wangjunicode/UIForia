using System.Diagnostics;
using UnityEngine;

namespace UIForia.Systems {
    
    [DebuggerDisplay("{" + nameof(element) + ".ToString()}")]
    public class RenderData {
        
        public Mesh mesh;
        public UIElement element;               
        public Material material;
        public Vector4 clipVector;
        public Vector3 renderPosition;
        private CullResult cullResult;
        private CullResult previousCullResult;

        public readonly bool isMeshProvider;
        public readonly bool isMaterialProvider;

        public RenderData(UIElement element) {
            this.element = element;
            this.isMeshProvider = this.element is IMeshProvider;
            this.isMaterialProvider = this.element is IMaterialProvider;
        }

        public ElementRenderer Renderer => element.Renderer;
        public bool CullResultChanged => previousCullResult != cullResult;
       
        public CullResult CullResult {
            get { return cullResult; }
            set {
                previousCullResult = cullResult;
                cullResult = value;
            }
        }

    }

    
    
}