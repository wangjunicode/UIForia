using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {
        
        public UIViewIMGUI(Type elementType) : base(elementType) {
            renderSystem = new IMGUIRenderSystem(styleSystem, layoutSystem);
        }

        public void SetViewRect(Rect viewportRect) {
            renderSystem.SetViewportRect(viewportRect);
        }
        
        public override IRenderSystem renderSystem { get; protected set; }
        
        public override void Render() {
            renderSystem.OnRender();
        }

    }

}