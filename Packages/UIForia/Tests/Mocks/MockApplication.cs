using System;
using UIForia;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using Application = UIForia.Application;

namespace Tests.Mocks {

    public class MockApplication : Application {

        public MockApplication(Type elementType, string template = null) {
            MockLayoutSystem layoutSystem = new MockLayoutSystem(m_StyleSystem);
            MockRenderSystem renderSystem = new MockRenderSystem();
            MockInputSystem inputSystem = new MockInputSystem(layoutSystem, m_StyleSystem);
            m_Systems[m_Systems.IndexOf(m_RenderSystem)] = renderSystem;
            m_Systems[m_Systems.IndexOf(m_InputSystem)] = inputSystem;
            m_Systems[m_Systems.IndexOf(m_LayoutSystem)] = layoutSystem;
            m_InputSystem = inputSystem;
            m_RenderSystem = renderSystem;
            m_LayoutSystem = layoutSystem;

            AddView(new Rect(), elementType, template);
        }

        public new MockInputSystem InputSystem => (MockInputSystem) m_InputSystem;
        public UIElement RootElement => m_Views[0].RootElement;

        public void SetViewportRect(Rect rect) {
            m_Views[0].Viewport = rect;
        }

    }

    public class MockRenderSystem : IRenderSystem {

        public void OnReset() { }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementCreated(UIElement element) { }

        public event Action<LightList<RenderData>, LightList<RenderData>, Vector3, Camera> DrawDebugOverlay;

        public RenderData GetRenderData(UIElement element) {
            return null;
        }

        public void SetCamera(Camera camera) { }

    }

}