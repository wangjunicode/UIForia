using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
            layoutSystem = new LayoutSystem(new GOTextSizeCalculator(), styleSystem);
            renderSystem = new GORenderSystem(layoutSystem, viewTransform);
            inputSystem = null;
        }

        protected override IRenderSystem renderSystem { get; set; }
        protected override ILayoutSystem layoutSystem { get; set; }
        protected override IInputSystem inputSystem { get; set; }

        public override void Render() {
            renderSystem.OnUpdate();
        }

    }

}