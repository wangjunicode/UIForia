using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        private readonly IRenderSystem renderSystem;
        private readonly ILayoutSystem layoutSystem;
        private readonly IInputSystem inputSystem;
        
        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
            layoutSystem = new LayoutSystem(new GOTextSizeCalculator(), styleSystem);
            renderSystem = new GORenderSystem(layoutSystem, viewTransform);
            inputSystem = null;
        }

      

        public void Render() {
            renderSystem.OnUpdate();
        }

    }

}