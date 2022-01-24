using System;
using UIForia.Elements;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UIForia {

    public class GameApplication : Application {

        protected GameApplication(bool isPreCompiled, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister, bool invertY) : base(isPreCompiled, templateData, resourceManager, onRegister, invertY) { }

        public static Application CreateFromRuntimeTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            bool invertY = camera.GetUniversalAdditionalCameraData()?.renderType == CameraRenderType.Overlay;
            GameApplication retn = new GameApplication(false, templateSettings, resourceManager, onRegister, invertY);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

        public static Application CreateFromPrecompiledTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            bool invertY = camera.GetUniversalAdditionalCameraData()?.renderType == CameraRenderType.Overlay;
            GameApplication retn = new GameApplication(true, templateSettings, resourceManager, onRegister, invertY);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

    }

}