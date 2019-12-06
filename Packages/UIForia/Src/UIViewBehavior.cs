using System;
using System.IO;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {
    
    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        private Application application;
        public bool usePreCompiledTemplates;
        
        [HideInInspector]
        public string applicationName = "Game App";

        public TemplateSettings GetTemplateSettings() {
            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = applicationName;
            settings.assemblyName = "Assembly-CSharp";
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated");
            settings.codeFileExtension = "generated.cs";
            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + applicationName;
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
            return settings;
        }
            
        public void Start() {
            type = Type.GetType(typeName);
            if (type == null) return;
            // 1. creates the application
            if (usePreCompiledTemplates) {
            }
            else {
                TemplateSettings settings = GetTemplateSettings();
                application = GameApplication.Create(type, settings, camera);
            }

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