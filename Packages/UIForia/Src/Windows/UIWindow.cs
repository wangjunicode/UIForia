using System;
using UIForia.Elements;
using UIForia.Layout;
using UnityEngine;

namespace UIForia.Windows {
    
    public class UIWindowRootElement : UIElement, IPointerQueryHandler {

        public UIWindowRootElement() {
            flags |= UIElementFlags.ImplicitElement;
            flags |= UIElementFlags.Created;
        }

        public bool ContainsPoint(Vector2 point) {
            return false;
        }
    }
    
    public class UIWindow : UIElement {

        public string windowId;
        
        private bool isDestroyed;
        private bool isShown;

        private WindowManager windowManager;

        internal IWindowSpawner spawner;
        
        public event Action<UIElement> onElementCreated;
        public event Action<UIElement> onElementReady;
        public event Action<UIElement> onElementRegistered;
        public event Action<UIElement> onElementDestroyed;
        public event Action<UIElement> onElementHierarchyEnabled;
        public event Action<UIElement> onElementHierarchyDisabled;

        private readonly Type m_ElementType;
        private readonly string m_Template;

        public int Depth { get; set; }

        public Rect Viewport { get; set; }

        internal Vector3 position;

        public readonly string name;
        
        public bool focusOnMouseDown;
        public bool sizeChanged;
        
        internal void InitUIWindow(string windowId, IWindowSpawner spawner, Size size) {
            this.windowId = windowId;
            this.spawner = spawner;
            this.Viewport = new Rect(0, 0, size.width, size.height);

            this.sizeChanged = true;
        }

        public bool Show(Action<UIWindow> afterShow = null) {
            if (isDestroyed) {
                return false;
            }

            return spawner.Show(this, afterShow);
        }

        public bool Hide(Action<UIWindow> afterHide = null) {
            if (isDestroyed) {
                return false;
            }

            return spawner.Hide(this, afterHide);
        }

        public bool Toggle(Action<UIWindow> afterShow = null, Action<UIWindow> afterHide = null) {
            if (isDestroyed) {
                return false;
            }

            if (isShown) {
                return spawner.Hide(this, afterHide);
            }

            return spawner.Show(this, afterShow);
        }

        public void Destroy() {
            windowManager.Destroy(this);
        }

        internal void ElementRegistered(UIElement element) {
            onElementRegistered?.Invoke(element);
        }

        internal void ElementCreated(UIElement element) {
            onElementCreated?.Invoke(element);
        }

        internal void ElementDestroyed(UIElement element) {
            onElementDestroyed?.Invoke(element);
        }

        internal void ElementReady(UIElement element) {
            onElementReady?.Invoke(element);
        }

        internal void ElementHierarchyEnabled(UIElement element) {
            onElementHierarchyEnabled?.Invoke(element);
        }

        internal void ElementHierarchyDisabled(UIElement element) {
            onElementHierarchyDisabled?.Invoke(element);
        }

        public void SetPosition(Vector2 position) {
            if (position != Viewport.position) {
                sizeChanged = true;
            }

            Viewport = new Rect(position.x, position.y, Viewport.width, Viewport.height);
        }

        public void SetSize(int width, int height) {
            if (width != Viewport.width || height != Viewport.height) {
                sizeChanged = true;
            }

            Viewport = new Rect(Viewport.x, Viewport.y, width, height);
        }
        
    }
}