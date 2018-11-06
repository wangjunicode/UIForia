using System;
using Src.Rendering;
using Src;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Tests.Mocks {

    public class MockView : UIView {

        public MockView(Type elementType, string template = null) : base(elementType, template) {
            layoutSystem = new MockLayoutSystem(styleSystem);
            inputSystem = new MockInputSystem(layoutSystem, styleSystem);
            renderSystem = new MockRenderSystem();
            systems.Add(layoutSystem);
            systems.Add(inputSystem);
        }

        public MockInputSystem InputSystem => (MockInputSystem) inputSystem;

    }

    public class MockRenderSystem : IRenderSystem {

        public void OnReset() { }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }
        
        public void OnElementCreatedFromTemplate(UIElement element) { }

        public event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay;

        public RenderData GetRenderData(UIElement element) {
            return null;
        }

    }

}