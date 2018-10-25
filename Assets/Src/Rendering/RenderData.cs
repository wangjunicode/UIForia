using System;
using System.Diagnostics;
using Rendering;
using Src.Elements;
using Src.Systems;
using UnityEngine;

namespace Src.Systems {

    public struct DrawInfo {
        
        public UIElement element;
        public Vector4 clipVector;
        public Vector3 renderPosition;
        public Mesh mesh;
        public Material material;
        
    }
    
    [DebuggerDisplay("{" + nameof(element) + ".ToString()}")]
    public class RenderData {
        
        public Mesh mesh;
        public UIElement element;               
        public Material material;
        public Vector4 clipVector;
        public Vector3 renderPosition;
        
        private bool isElementDrawable;
        
        public RenderData(UIElement element) {
            this.element = element;
        }

        public ElementRenderer Renderer => element.Renderer;

    }

    
    
}