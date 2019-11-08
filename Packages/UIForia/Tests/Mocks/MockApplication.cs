using System;
using System.IO;
using Src.Systems;
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
            MockLayoutSystem layoutSystem = new MockLayoutSystem(this);
            MockRenderSystem renderSystem = new MockRenderSystem(null, this);
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

    public class MockRenderSystem : VertigoRenderSystem {
        
        public override void OnUpdate() {
            // do nothing
        }

        public MockRenderSystem(Camera camera, Application application) : base(camera, application) { }

    }

}