using System;
using System.Diagnostics;
using System.IO;
using Src.Systems;
using UIForia;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEditor;
using UnityEngine;
using Application = UIForia.Application;

namespace Tests.Mocks {

    public class MockApplication<T> : MockApplication where T : UIElement {

        public MockApplication(string template = null, ResourceManager resourceManager = null, bool createView = true) : base(typeof(T), template, resourceManager, createView) { }

        public T GetRootElement() {
            return GetView(0).RootElement.GetChild(0) as T;
        }

    }

    public class MockApplication : Application {

        public static MockApplication SetupWithSettings<T>(TemplateSettings settings, string appName = null, bool usePreCompiledTemplates = false) where T : UIElement {
            if (appName == null) {
                StackTrace stackTrace = new StackTrace();
                appName = stackTrace.GetFrame(1).GetMethod().Name;
            }

            CompiledTemplateData compiledTemplates = usePreCompiledTemplates
                ? TemplateLoader.LoadPrecompiledTemplates(settings)
                : TemplateLoader.LoadRuntimeTemplates(typeof(T), settings);

            return new MockApplication(compiledTemplates, null);
        }


        public static TemplateSettings GetDefaultSettings(string appName) {
            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = appName;
            settings.templateRoot = "Data";
            settings.assemblyName = typeof(MockApplication).Assembly.GetName().Name;
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = ".generated.xml.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");
            return settings;
        }
        
        public static MockApplication Setup<T>(string appName = null, bool usePreCompiledTemplates = false) where T : UIElement {
            if (appName == null) {
                StackTrace stackTrace = new StackTrace();
                appName = stackTrace.GetFrame(1).GetMethod().Name;
            }

            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = appName;
            settings.templateRoot = "Data";
            settings.assemblyName = typeof(MockApplication).Assembly.GetName().Name;
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = ".generated.xml.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");

            CompiledTemplateData compiledTemplates = usePreCompiledTemplates
                ? TemplateLoader.LoadPrecompiledTemplates(settings)
                : TemplateLoader.LoadRuntimeTemplates(typeof(T), settings);

            return new MockApplication(compiledTemplates, null);
        }

        public MockApplication(CompiledTemplateData templateData, ResourceManager resourceManager) : base(templateData, resourceManager) {
            MockLayoutSystem layoutSystem = new MockLayoutSystem(this);
            MockRenderSystem renderSystem = new MockRenderSystem(null, this);
            MockInputSystem inputSystem = new MockInputSystem(layoutSystem);
            m_Systems[m_Systems.IndexOf(m_RenderSystem)] = renderSystem;
            m_Systems[m_Systems.IndexOf(m_InputSystem)] = inputSystem;
            m_Systems[m_Systems.IndexOf(m_LayoutSystem)] = layoutSystem;
            m_InputSystem = inputSystem;
            m_RenderSystem = renderSystem;
            m_LayoutSystem = layoutSystem;
        }

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

//            if (createView) {
//                CreateView("Test View", new Rect(), elementType, template);
//            }
        }

        public new MockInputSystem InputSystem => (MockInputSystem) m_InputSystem;
        public UIElement RootElement => m_Views[0].RootElement;

        public void SetViewportRect(Rect rect) {
            m_Views[0].Viewport = rect;
        }

    }

    public class MockRenderSystem : VertigoRenderSystem {

        public override void OnUpdate() {
            // do nothing
        }

        public MockRenderSystem(Camera camera, Application application) : base(camera, application) { }

    }

}