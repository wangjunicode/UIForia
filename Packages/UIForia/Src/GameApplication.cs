using System;
using UIForia.Elements;
using UIForia.Windows;
using UnityEngine;

namespace UIForia {

    public struct UISettings {
        public bool isPreCompiled;
        public TemplateSettings templateSettings;
        public ResourceManager resourceManager;
        public Action<UIElement> onElementRegistered;
        public IWindowSpawner defaultWindowSpawner;
    }

    public class GameApplication : Application {

        protected GameApplication(UISettings uiSettings) : base(uiSettings) { }

        public static Application CreateFromRuntimeTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            GameApplication retn = new GameApplication(new UISettings {
                isPreCompiled = false, 
                templateSettings = templateSettings, 
                resourceManager = resourceManager, 
                onElementRegistered = onRegister,
                defaultWindowSpawner =  new DefaultWindowSpawner()
            });

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

        public static Application CreateFromPrecompiledTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            GameApplication retn = new GameApplication(new UISettings {
                isPreCompiled = true, 
                templateSettings = templateSettings, 
                resourceManager = resourceManager, 
                onElementRegistered = onRegister,
                defaultWindowSpawner =  new DefaultWindowSpawner()
            });

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

    }

}