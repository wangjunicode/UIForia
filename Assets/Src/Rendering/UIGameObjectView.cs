using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        private readonly RectTransform rectTransform;
        
        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
            this.rectTransform = viewTransform;
            layoutSystem = new LayoutSystem(styleSystem);
            renderSystem = new DirectRenderSystem(layoutSystem, styleSystem);
//            renderSystem = new GORenderSystem(layoutSystem, styleSystem, viewTransform);
            inputSystem = new GOInputSystem(layoutSystem, styleSystem);
            systems.Add(inputSystem);
            systems.Add(layoutSystem);
            systems.Add(renderSystem);
        }

        public void UpdateViewport() {
//            layoutSystem.SetViewportRect(new Rect(rectTransform.rect) {
//                x = 0,
//                y = 0
//            });
        }

        public override void Update() {
            Rect viewport = rectTransform.rect;
            viewport.y = viewport.height + viewport.y;
            layoutSystem.SetViewportRect(viewport);
            styleSystem.SetViewportRect(viewport);
            base.Update();
        }

    }

}