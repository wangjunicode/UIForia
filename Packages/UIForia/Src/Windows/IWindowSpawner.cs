using System;
using UnityEngine;

namespace UIForia.Windows {
    public interface IWindowSpawner {
        bool Show(UIWindow window, Action<UIWindow> afterShow = null);
        bool Hide(UIWindow window, Action<UIWindow> afterHide = null);
        void Minimize(UIWindow window);
        void Maxmimize(UIWindow window);
    }
    
    public class DefaultWindowSpawner : IWindowSpawner {

        private Vector2 lastWindowLocation;

        public bool Show(UIWindow window, Action<UIWindow> afterShow) {
            window.RootElement.SetEnabled(true);
            lastWindowLocation = window.position;
            return true;
        }

        public bool Hide(UIWindow window, Action<UIWindow> afterHide) {
            window.RootElement.SetEnabled(false);
            return true;
        }

        public void Minimize(UIWindow window) {
            // todo - run minimize animation
        }

        public void Maxmimize(UIWindow window) {
            // todo - run maximize animation
        }
    }
}
