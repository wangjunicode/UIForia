using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UIForia;
using UIForia.Animation;
using UIForia.Elements;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Text;
using UnityEngine;
using Application = UIForia.Application;

namespace Tests.Mocks {

    public class MockApplication : Application {

        public static bool s_GenerateCode;
        public static bool s_UsePreCompiledTemplates;

        protected MockApplication(bool isPreCompiled, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister) 
            : base(isPreCompiled, templateData, resourceManager, onRegister) { }

        // temporary work around so we dont need to keep allocating lots of big buffers w/o releasing
        
        
        protected override void CreateSystems() {
            elementSystem = new ElementSystem(InitialElementCapacity);
            styleSystem = new StyleSystem(elementSystem);
            routingSystem = new RoutingSystem();
            linqBindingSystem = new LinqBindingSystem();
            textSystem = new TextSystem(elementSystem);
            layoutSystem = new MockLayoutSystem(this, elementSystem, textSystem);
            renderSystem = new MockRenderSystem(this);
            animationSystem = new AnimationSystem(elementSystem);
            inputSystem = new MockInputSystem(layoutSystem);
        }

        public static void Generate(bool shouldGenerate = true) {
            s_GenerateCode = shouldGenerate;
            s_UsePreCompiledTemplates = shouldGenerate;
        }


        public static MockApplication Setup<T>(string appName = null, List<Type> dynamicTemplateTypes = null) where T : UIElement {
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
            settings.rootType = typeof(T);
            settings.resourceManager = new ResourceManager();
            settings.dynamicallyCreatedTypes = dynamicTemplateTypes;
            
            if (s_GenerateCode) {
                TemplateCodeGenerator.Generate(typeof(T), settings);
            }

            MockApplication app = new MockApplication(s_UsePreCompiledTemplates, settings, settings.resourceManager, null);
            app.Initialize();
            return app;
        }

         public static MockApplication Setup(TemplateSettings settings,  bool usePreCompiledTemplates = false) {
            MockApplication app = new MockApplication(usePreCompiledTemplates, settings, null, null);
            app.Initialize();
            return app;
        }
         
        public new MockInputSystem InputSystem => (MockInputSystem) inputSystem;
        public UIElement RootElement => views[0].RootElement;

        public void SetViewportRect(Rect rect) {
            views[0].Viewport = rect;
        }

    }

    public class MockRenderSystem : RenderSystem2 {

        public override void OnUpdate() {
            // do nothing
        }

        public MockRenderSystem(Application application) : base(application, application.layoutSystem, application.elementSystem) { }

    }

}