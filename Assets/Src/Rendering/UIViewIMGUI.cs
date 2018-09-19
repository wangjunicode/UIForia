using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {

        private readonly ILayoutSystem layoutSystem;
        
        public UIViewIMGUI(Type elementType) : base(elementType) {
//            layoutSystem = new LayoutSystem(new IMGUITextSizeCalculator(), styleSystem);
//            IRenderSystem renderSystem = new IMGUIRenderSystem(this, styleSystem, layoutSystem);
//            IInputSystem inputSystem = new IMGUIInputSystem(layoutSystem, styleSystem);
//            systems.Add(layoutSystem);
//            systems.Add(renderSystem);
//            systems.Add(inputSystem);
        }

        public void SetViewRect(Rect viewportRect) {
            layoutSystem.SetViewportRect(viewportRect);
        }
        
    }

}
