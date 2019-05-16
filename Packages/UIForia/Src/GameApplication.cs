using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Parsing.Expression;
using UIForia.Rendering;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class GameApplication : Application {

        private readonly LightList<UIElement> windowStack = new LightList<UIElement>();
        
        public T CreateWindow<T>(Vector2 position, Size size) where T : UIElement {
            throw new NotImplementedException();
            ParsedTemplate template = templateParser.GetParsedTemplate(typeof(T));
            
            UIElement windowRoot = template.Create();
            
            windowRoot.style.SetTransformPositionX(position.x, StyleState.Normal);
            windowRoot.style.SetTransformPositionY(position.y, StyleState.Normal);
            windowRoot.style.SetPreferredWidth(size.width, StyleState.Normal);
            windowRoot.style.SetPreferredHeight(size.height, StyleState.Normal);
            windowRoot.parent = m_Views[0].RootElement;
            windowRoot.children.Add(windowRoot);
            
            windowStack.Add(windowRoot);
            
            return (T) windowRoot;
        }

        public void HideWindow(UIElement window) {
            
        }

        protected GameApplication(string id, string templateRootPath = null) : base(id, templateRootPath) { }

        public static GameApplication Create<T>(string applicationId, Camera camera, string templateRootPath = null, Action<Application> onBootstrap = null) where T : UIElement {
            return Create(applicationId, typeof(T), camera, templateRootPath, onBootstrap);
        }

        public static GameApplication Create(string applicationId, Type type, Camera camera, string templateRootPath = null, Action<Application> onBootstrap = null) {
            if (type == null || !typeof(UIElement).IsAssignableFrom(type)) {
                throw new Exception($"The type given to {nameof(Create)} must be non null and a subclass of {nameof(UIElement)}");
            }
            GameApplication retn = new GameApplication(applicationId, templateRootPath);
            onBootstrap?.Invoke(retn);
            retn.SetCamera(camera);
            UIView view = retn.AddView("Default View", new Rect(0, 0, Screen.width, Screen.height), type);
            retn.onUpdate += () => {
                // todo if view was destroyed return or remove auto-update handler
                view.SetSize(Screen.width, Screen.height);
            };
            return retn;
        }

    }

}