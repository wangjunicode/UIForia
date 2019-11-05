using System;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {
        
        protected GameApplication(CompiledTemplateData templateData, ResourceManager resourceManager) : base(templateData, resourceManager) { }
//        public static GameApplication Create<T>(string applicationId, Camera camera, string templateRootPath = null, Action<Application> onBootstrap = null) where T : UIElement {
//            return Create(applicationId, typeof(T), camera, templateRootPath, onBootstrap);
//        }

        // todo -- remove
//        public static GameApplication Create(string applicationId, Type type, Camera camera, string templateRootPath = null, Action<Application> onBootstrap = null) {
//            if (type == null || !typeof(UIElement).IsAssignableFrom(type)) {
//                throw new Exception($"The type given to {nameof(Create)} must be non null and a subclass of {nameof(UIElement)}");
//            }
//            GameApplication retn = new GameApplication(applicationId, templateRootPath);
//            onBootstrap?.Invoke(retn);
//            retn.SetCamera(camera);
//            retn.CreateView("Default View", new Rect(0, 0, Screen.width, Screen.height), type);
//          
//            return retn;
//        }
        
        public static GameApplication Create(CompiledTemplateData templateData, Camera camera, Action<Application> onBootstrap = null) {
            
            GameApplication retn = new GameApplication(templateData, null);
            
            onBootstrap?.Invoke(retn);
            
            retn.SetCamera(camera);
            
            // retn.CreateView("Default View", new Rect(0, 0, Screen.width, Screen.height), type);
          
            return retn;
        }

    }

}