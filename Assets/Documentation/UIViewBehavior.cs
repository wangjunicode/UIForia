using System;
using UIForia.Elements;
using UnityEngine;

namespace UIForia {
    
    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        public Application application;

        public void Start() {
            type = Type.GetType(typeName);
            if (type == null) return;
            // 1. creates the application
            application = GameApplication.Create( "Game App", type, camera);
            application.onElementRegistered += DoDependencyInjection;
        }

        // optional!
        private void DoDependencyInjection(UIElement element) {
            // DiContainer.Inject(element);
        }

        private void Update() {
            if (type == null) return;
            // 2. update the application every frame
            application?.Update();
        }

    }

}