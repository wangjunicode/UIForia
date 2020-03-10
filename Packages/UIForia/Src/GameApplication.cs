using System;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {

        protected GameApplication(bool isPreCompiled, Module rootModule, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister)
            : base(isPreCompiled, rootModule, templateData, resourceManager, onRegister) { }

        public static Application CreateFromRuntimeTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {

            Module module = Module.LoadRootModule(templateSettings.rootType);

            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            GameApplication retn = new GameApplication(false, module, templateSettings, resourceManager, onRegister);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

        // todo -- when using precompiled we shouldn't need to do the module creation, should be loaded from some init function
        public static Application CreateFromPrecompiledTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {
            ResourceManager resourceManager = new ResourceManager();

            templateSettings.resourceManager = resourceManager;

            Type rootModuleType = Module.GetModuleTypeFromElementType(templateSettings.rootType);

            Module module = Module.LoadRootModule(rootModuleType);
            
            GameApplication retn = new GameApplication(true, module, templateSettings, resourceManager, onRegister);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

    }

}