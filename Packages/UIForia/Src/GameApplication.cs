using System;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {

        protected GameApplication(string id, string templateRootPath = null) : base(id, templateRootPath) { }

        public static GameApplication Create<T>(string applicationId, Camera camera, string templateRootPath = null) where T : UIElement {
            return Create(applicationId, typeof(T), camera, templateRootPath);
        }

        public static GameApplication Create(string applicationId, Type type, Camera camera, string templateRootPath = null) {
            if (type == null || !typeof(UIElement).IsAssignableFrom(type)) {
                throw new Exception($"The type given to {nameof(Create)} must be non null and a subclass of {nameof(UIElement)}");
            }
            GameApplication retn = new GameApplication(applicationId, templateRootPath);
            retn.Bootstrap();
            retn.SetCamera(camera);
            UIView view = retn.AddView(new Rect(0, 0, Screen.width, Screen.height), type);
            view.is2D = true;
            retn.onUpdate += () => {
                // todo if view was destroyed return or remove auto-update handler
                view.SetSize(Screen.width, Screen.height);
            };
            return retn;
        }

    }

}