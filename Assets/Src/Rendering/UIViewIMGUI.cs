using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {
        
        public UIViewIMGUI(Type elementType) : base(elementType) {
            renderSystem = new IMGUIRenderSystem(elementSystem, styleSystem, layoutSystem);
        }

        public void SetViewRect(Rect viewportRect) {
            renderSystem.SetViewportRect(viewportRect);
        }
        
        protected override IRenderSystem renderSystem { get; set; }
        
        public override void Render() {
            renderSystem.OnRender();
        }

    }

}