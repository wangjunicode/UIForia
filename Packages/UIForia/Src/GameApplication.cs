using System;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {

        protected GameApplication(bool isPreCompiled, Module rootModule, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister)
            : base(isPreCompiled, rootModule, templateData, resourceManager, onRegister) { }

        public static Application CreateFromRuntimeTemplates(TemplateSettings templateSettings, Camera camera, Action<UIElement> onRegister) {

            TypeResolver.Initialize();

            Type rootModuleType = Module.GetModuleTypeFromElementType(templateSettings.rootType);

            if (rootModuleType == null) {
                throw new Exception("Unable to find module for type " + templateSettings.rootType);
            }

            Module module = Module.CreateRootModule(rootModuleType);

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

            Module module = Module.CreateRootModule(rootModuleType);
            
            GameApplication retn = new GameApplication(true, module, templateSettings, resourceManager, onRegister);

            retn.Initialize();

            retn.SetCamera(camera);

            return retn;
        }

    }

}