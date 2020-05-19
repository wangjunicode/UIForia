using System;
using System.IO;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {

    [Template]
    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        public Application application;
        public bool usePreCompiledTemplates;

        [HideInInspector] public string applicationName = "Game App 2";

        public TemplateSettings GetTemplateSettings(Type type) {
            TemplateSettings settings = new TemplateSettings();
            settings.rootType = type;
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

            TemplateSettings settings = GetTemplateSettings(type);

            // Module.CreateRootModule<>()
            //     
            // Module.CompileApplication();
            //
            // Module.CompileLibrary();
            //
            // Module.CreateApplication();
            //
            // Module.LoadCompiledApplication();
            //
            // application = Module.CreateApplication<CompilerDemo>(typeof(Tmp), camera);
            
            // we can go from template path to module
            // how do we bootstrap this bitch?
            
            // application = usePreCompiledTemplates
            //     ? GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection)
            //     : GameApplication.CreateFromRuntimeTemplates(settings, camera, DoDependencyInjection);

        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private void Update() {
            if (type == null || application == null) return;
            application.Update();
            application.GetView(0).SetSize((int) application.Width, (int) application.Height);
        }

    }

}