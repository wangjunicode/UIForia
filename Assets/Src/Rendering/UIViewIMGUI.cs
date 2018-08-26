using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIViewIMGUI : UIView {

        public UIViewIMGUI(Type elementType) : base(elementType) {
            layoutSystem = new LayoutSystem(new IMGUITextSizeCalculator(), styleSystem);
            renderSystem = new IMGUIRenderSystem(elementSystem, styleSystem, layoutSystem);
            inputSystem = new IMGUIInputSystem(layoutSystem, elementSystem, styleSystem);
            systems.Add(layoutSystem);
            systems.Add(renderSystem);
            systems.Add(inputSystem);
        }

        public void SetViewRect(Rect viewportRect) {
            layoutSystem.SetViewportRect(viewportRect);
        }
        
        protected override IInputSystem inputSystem { get; set; }
        protected override IRenderSystem renderSystem { get; set; }
        protected override ILayoutSystem layoutSystem { get; set; }

        public override void Render() {
            inputSystem.OnUpdate();
            renderSystem.OnRender();
        }

    }

}

/*
Layout Rects -> if has style, register w/ input system

    How do I tell if ui style has a hover state? for now we don't.
    How I map layout rect (ie position) to style?

*/