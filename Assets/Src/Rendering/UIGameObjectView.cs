using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        private readonly IRenderSystem renderSystem;
        private readonly IInputSystem inputSystem;
        private readonly ILayoutSystem layoutSystem;
        private readonly RectTransform rectTransform;
        
        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
            this.rectTransform = viewTransform;
            layoutSystem = new LayoutSystem2(styleSystem);
            renderSystem = new GORenderSystem(layoutSystem, styleSystem, viewTransform);
            inputSystem = new GOInputSystem(layoutSystem, styleSystem);
            systems.Add(inputSystem);
            systems.Add(layoutSystem);
            systems.Add(renderSystem);
        }

        public void UpdateViewport() {
            layoutSystem.SetViewportRect(new Rect(rectTransform.rect) {
                x = 0,
                y = 0
            });
        }

    }

}