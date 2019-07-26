using System;
using System.IO;
using SVGX;
using UIForia;

using UIForia.Elements;
using UIForia.Rendering;
using UnityEditor;
using UnityEngine;
using Application = UIForia.Application;

namespace Tests.Mocks {

    public class MockApplication : Application {

        public MockApplication(Type elementType, string template = null, ResourceManager resourceManager = null, bool createView = true) : base(GUID.Generate().ToString(), null, resourceManager) {
            
            TemplateRootPath = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Tests"));
            MockLayoutSystem layoutSystem = new MockLayoutSystem(this, m_StyleSystem);
            MockRenderSystem renderSystem = new MockRenderSystem();
            MockInputSystem inputSystem = new MockInputSystem(layoutSystem);
            m_Systems[m_Systems.IndexOf(m_RenderSystem)] = renderSystem;
            m_Systems[m_Systems.IndexOf(m_InputSystem)] = inputSystem;
            m_Systems[m_Systems.IndexOf(m_LayoutSystem)] = layoutSystem;
            m_InputSystem = inputSystem;
            m_RenderSystem = renderSystem;
            m_LayoutSystem = layoutSystem;

            if (createView) {
                CreateView("Test View", new Rect(), elementType, template);
            }
        }

        public new MockInputSystem InputSystem => (MockInputSystem) m_InputSystem;
        public UIElement RootElement => m_Views[0].RootElement;

        public void SetViewportRect(Rect rect) {
            m_Views[0].Viewport = rect;
        }

        public static MockApplication CreateWithoutView() {
            return new MockApplication(null, null, null, false);
        }
    }

    public class MockRenderSystem : IRenderSystem {

        public void OnReset() { }

        public void OnUpdate() {
            DrawDebugOverlay?.Invoke(null);
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementCreated(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) {}
        
        public event Action<ImmediateRenderContext> DrawDebugOverlay;

        public void SetCamera(Camera camera) { }

    }

}