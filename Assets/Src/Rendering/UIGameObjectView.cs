using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        private readonly IRenderSystem renderSystem;
        private readonly IInputSystem inputSystem;
        private readonly ILayoutSystem layoutSystem;
        private readonly RectTransform rectTransform;
        public Font tempFont;
        public UIGameObjectView(Font tempFont, Type elementType, RectTransform viewTransform) : base(elementType) {
            this.rectTransform = viewTransform;
            layoutSystem = new LayoutSystem(new GOTextSizeCalculator(), styleSystem);
            renderSystem = new GORenderSystem(layoutSystem, styleSystem, viewTransform);
            inputSystem = null;
            ((GORenderSystem) renderSystem).tempFont = tempFont;
            systems.Add(layoutSystem);
            systems.Add(renderSystem);
        }

        public void UpdateViewport() {
            layoutSystem.SetViewportRect(rectTransform.rect);
        }

    }

}