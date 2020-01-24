using System;
using System.IO;
using UIForia.Compilers;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        private Application application;
        public bool usePreCompiledTemplates;

        [HideInInspector] public string applicationName = "Game App 2";

        public TemplateSettings GetTemplateSettings() {
            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = applicationName;
            settings.assemblyName = "Assembly-CSharp";
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated2");
            settings.codeFileExtension = "generated.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated2/" + applicationName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
            return settings;
        }

        public void Start() {
            type = Type.GetType(typeName);
            if (type == null) return;
            
            // 1. creates the application
            
            TemplateSettings settings = GetTemplateSettings();
            settings.resourceManager = new ResourceManager();
            
            CompiledTemplateData compiledTemplates = usePreCompiledTemplates
                ? TemplateLoader.LoadPrecompiledTemplates(settings)
                : TemplateLoader.LoadRuntimeTemplates(type, settings);
            
            application = GameApplication.Create(compiledTemplates, camera);
            application.onElementRegistered += DoDependencyInjection;
        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private void Update() {
            if (type == null) return;
            // 2. update the application every frame
            application?.Update();
        }

    }

}