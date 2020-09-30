using System;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {

    public class TestApplication : Application {

        internal TestApplication(string applicationName) : base(applicationName) { }

    }

    public class EditorApplication : Application {

        internal EditorApplication(string applicationName) : base(applicationName) { }

    }

    public class GameApplication2 : Application {

        internal GameApplication2(string applicationName) : base(applicationName) { }

    }

    public class GameApplication : Application {

        protected GameApplication(bool isPreCompiled, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister) : base(isPreCompiled, templateData, resourceManager, onRegister) { }

        public static Application CreateFromRuntimeTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            GameApplication retn = new GameApplication(false, templateSettings, resourceManager, onRegister);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

        public static Application CreateFromPrecompiledTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            GameApplication retn = new GameApplication(true, templateSettings, resourceManager, onRegister);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

    }

}