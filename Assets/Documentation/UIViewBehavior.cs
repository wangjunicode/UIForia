using System;
using System.Data;
using System.IO;
using UIForia.Elements;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia {

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

        private CommandBuffer commandBuffer;

        private void Update() {
            if (type == null) return;

            if (application != null) {
                if (commandBuffer == null) {
                    commandBuffer = new CommandBuffer();
                    commandBuffer.name = "UIForia";
                    camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
                }

                application.DPIScaleFactor = 1; // todo -- remove this
                application.Update();
                commandBuffer.Clear();
                application.Render(camera, commandBuffer);
            }
        }

        public void OnApplicationQuit() {
            application?.Dispose();
        }

    }

}