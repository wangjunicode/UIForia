using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {

        private readonly ILayoutSystem layoutSystem;
        
        public UIViewIMGUI(Type elementType) : base(elementType) {
            layoutSystem = new LayoutSystem(new IMGUITextSizeCalculator(), styleSystem);
            IRenderSystem renderSystem = new IMGUIRenderSystem(this, styleSystem, layoutSystem);
            IInputSystem inputSystem = new IMGUIInputSystem(layoutSystem, this, styleSystem);
            systems.Add(layoutSystem);
            systems.Add(renderSystem);
            systems.Add(inputSystem);
        }

        public void SetViewRect(Rect viewportRect) {
            layoutSystem.SetViewportRect(viewportRect);
        }
        
    }

}

/*
Layout Rects -> if has style, register w/ input system

    How do I tell if ui style has a hover state? for now we don't.
    How I map layout rect (ie position) to style?

*/