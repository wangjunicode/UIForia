using System;
using System.IO;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {
    
    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        public Application application;
        public bool usePreCompiledTemplates;
        public string styleBasePath;
        
        [HideInInspector] public string applicationName = "Game App 2";

        public TemplateSettings GetTemplateSettings(Type type) {
            TemplateSettings settings = new TemplateSettings();
            settings.rootType = type;
            settings.applicationName = applicationName;
            settings.assemblyName = "Assembly-CSharp";
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated2");
            settings.codeFileExtension = "generated.cs";
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
            settings.styleBasePath = styleBasePath ?? string.Empty;

            return settings;
        }

        public void Start() {
            type = Type.GetType(typeName);
            if (type == null) return;

            TemplateSettings settings = GetTemplateSettings(type);
            settings.materialAssets = GetComponent<UIForiaAssets>()?.materialReferences;
            
#if UNITY_EDITOR
            application = usePreCompiledTemplates
                ? GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection)
                : GameApplication.CreateFromRuntimeTemplates(settings, camera, DoDependencyInjection);
#else
            application = GameApplication.CreateFromPrecompiledTemplates(settings, camera, DoDependencyInjection);
#endif

        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private void Update() {
            if (type == null) return;
            application?.Update();
            application?.GetView(0).SetSize((int) application.Width, (int) application.Height);
        }

        private void OnDestroy() {
            application?.Destroy();
        }

    }

}