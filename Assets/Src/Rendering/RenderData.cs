using System;
using System.Diagnostics;
using Rendering;
using Src.Elements;
using Src.Systems;
using UnityEngine;

namespace Src.Systems {
    
    [DebuggerDisplay("{" + nameof(element) + ".ToString()}")]
    public class RenderData {
        
        public Mesh mesh;
        public UIElement element;               
        public Material material;
        public Vector4 clipVector;
        public Vector3 renderPosition;

        public readonly bool isMeshProvider;
        public readonly bool isMaterialProvider;
        
        public RenderData(UIElement element) {
            this.element = element;
            this.isMeshProvider = this.element is IMeshProvider;
            this.isMaterialProvider = this.element is IMaterialProvider;
        }

        public ElementRenderer Renderer => element.Renderer;

    }

    
    
}