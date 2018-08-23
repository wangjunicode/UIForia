using System;
using Src.Systems;
using UnityEngine;

namespace Rendering {

    public sealed class UIGameObjectView : UIView {

        public UIGameObjectView(Type elementType, RectTransform viewTransform) : base(elementType) {
            renderSystem = new GameObjectRenderSystem(layoutSystem, viewTransform);
        }

        protected override IRenderSystem renderSystem { get; set; }

        public override void Render() {
            renderSystem.OnUpdate();
        }

    }

}