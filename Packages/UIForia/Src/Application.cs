using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia {

    public abstract class Application {

        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        protected readonly BindingSystem m_BindingSystem;
        protected readonly IStyleSystem m_StyleSystem;
        protected ILayoutSystem m_LayoutSystem;
        protected IRenderSystem m_RenderSystem;
        protected IInputSystem m_InputSystem;

        protected readonly SkipTree<UIElement> m_ElementTree;
        protected Router m_Router;

        protected readonly List<ISystem> m_Systems;

        public event Action<UIElement> onElementCreated;
        public event Action<UIElement> onElementDestroyed;
        public event Action<UIElement> onElementEnabled;
        public event Action<UIElement> onElementDisabled;

        public event Action onWillRefresh;
        public event Action onRefresh;
        public event Action onUpdate;
        public event Action onReady;
        public event Action onDestroy;
        public event Action<UIView> onViewAdded;
        public event Action<UIView> onViewRemoved;

        protected readonly List<UIView> m_Views;

        protected readonly List<List<UIElement>> m_DepthMap;
        protected static readonly DepthIndexComparer s_DepthIndexComparer = new DepthIndexComparer();

        public static readonly Application Game = new GameApplication();


        static Application() {
            ArrayPool<UIElement>.SetMaxPoolSize(64);
        }

        protected Application() {
            this.m_Systems = new List<ISystem>();
            this.m_ElementTree = new SkipTree<UIElement>();
            this.m_DepthMap = new List<List<UIElement>>();
            this.m_Views = new List<UIView>();
            this.m_Router = new Router();

            m_StyleSystem = new StyleSystem();
            m_BindingSystem = new BindingSystem();
            m_LayoutSystem = new LayoutSystem(m_StyleSystem);
            m_InputSystem = new DefaultInputSystem(m_LayoutSystem, m_StyleSystem);
            m_RenderSystem = new RenderSystem(null, m_LayoutSystem);
            m_Systems.Add(m_StyleSystem);
            m_Systems.Add(m_BindingSystem);
            m_Systems.Add(m_InputSystem);
            m_Systems.Add(m_LayoutSystem);
            m_Systems.Add(m_RenderSystem);
        }

        public IStyleSystem StyleSystem => m_StyleSystem;
        public BindingSystem BindingSystem => m_BindingSystem;
        public IRenderSystem RenderSystem => m_RenderSystem;
        public ILayoutSystem LayoutSystem => m_LayoutSystem;
        public IInputSystem InputSystem => m_InputSystem;
        public Camera Camera { get; private set; }
        public Router Router => m_Router;

        public void SetCamera(Camera camera) {
            Camera = camera;
            RenderSystem.SetCamera(camera);
        }

        public UIView AddView(Rect rect, Type type, string template = null) {
            UIView view = new UIView(this, rect, m_Views.Count, type, template);

            m_Views.Add(view);

            RegisterElement(view.RootElement);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }

            onViewAdded?.Invoke(view);
            return view;
        }

        internal void RegisterElement(UIElement element) {
            if (element.parent == null) {
                Debug.Assert(element.view.RootElement == element, nameof(element.view.RootElement) + " must be null if providing a null parent");

                element.flags |= UIElementFlags.AncestorEnabled;
                element.depth = 0;
            }
            else {
                if (element.parent.isEnabled) {
                    element.flags |= UIElementFlags.AncestorEnabled;
                }

                // todo -- doesn't handle disabled index
                element.siblingIndex = element.parent.children.IndexOf(element);

                element.depth = element.parent.depth + 1;
            }

            List<UIElement> list;
            if (m_DepthMap.Count <= element.depth) {
                list = ListPool<UIElement>.Get();
                m_DepthMap.Add(list);
            }
            else {
                list = m_DepthMap[element.depth];
            }

            InitHierarchy(element);

            int index = ~list.BinarySearch(0, list.Count, element, s_DepthIndexComparer);

            list.Insert(index, element);

            for (int i = index; i < list.Count; i++) {
                list[i].depthIndex = i;
            }

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementCreated(element);
            }

            InvokeOnCreate(element);
            InvokeOnReady(element);
            onElementCreated?.Invoke(element);
        }

        public void Refresh() {
            onWillRefresh?.Invoke();
            m_Router = new Router();
            foreach (ISystem system in m_Systems) {
                system.OnReset();
            }

            m_ElementTree.TraversePreOrder((el) => el.OnDestroy());

            m_ElementTree.Clear();
            for (int i = 0; i < m_DepthMap.Count; i++) {
                m_DepthMap[i].Clear();
                List<UIElement> map = m_DepthMap[i];
                ListPool<UIElement>.Release(ref map);
            }

            m_DepthMap.Clear();

            for (int i = 0; i < m_Views.Count; i++) {
                m_Views[i].Refresh();
                RegisterElement(m_Views[i].RootElement);

                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnViewAdded(m_Views[i]);
                }
            }

            onRefresh?.Invoke();
        }

        protected void InitHierarchy(UIElement element) {
            // todo -- assert no duplicate root elements
            if (element.parent == null) {
                element.flags |= UIElementFlags.AncestorEnabled;
                element.depth = 0;
            }
            else {
                if (element.parent.isEnabled) {
                    element.flags |= UIElementFlags.AncestorEnabled;
                }

                element.view = element.parent.view;
                element.depth = element.parent.depth + 1;
            }

            // todo -- maybe move this
            IRouteHandler routeHandler = element as IRouteHandler;
            if (routeHandler != null) {
                m_Router.AddRouteHandler(routeHandler);
            }

            m_ElementTree.AddItem(element);

            LightList<UIElement> children = element.children;

            if (children == null || children.Count == 0) {
                return;
            }

            List<UIElement> list;

            if (m_DepthMap.Count <= element.depth + 1) {
                list = ListPool<UIElement>.Get();
                m_DepthMap.Add(list);
            }
            else {
                list = m_DepthMap[element.depth + 1];
            }

            int idx = ~list.BinarySearch(0, list.Count, element.children[0], s_DepthIndexComparer);

            list.InsertRange(idx, children);

            for (int i = idx; i < list.Count; i++) {
                list[i].depthIndex = i;
            }

            for (int i = 0; i < children.Count; i++) {
                children[i].siblingIndex = i;
                InitHierarchy(children[i]);
            }
        }

        private static void InvokeOnCreate(UIElement element) {
            if (element.children != null) {
                for (int i = 0; i < element.children.Count; i++) {
                    InvokeOnCreate(element.children[i]);
                }
            }

            element.flags |= UIElementFlags.Created;
            element.OnCreate();

            Binding[] enabledBindings = element.OriginTemplate?.triggeredBindings;

            if (enabledBindings != null) {
                for (int i = 0; i < enabledBindings.Length; i++) {
                    if (enabledBindings[i].bindingType != BindingType.Constant) {
                        enabledBindings[i].Execute(element, element.templateContext);
                    }
                }
            }
        }

        private static void InvokeOnReady(UIElement element) {
            if (element.children != null) {
                for (int i = 0; i < element.children.Count; i++) {
                    InvokeOnReady(element.children[i]);
                }
            }

            // when creating new children in create or read, this can be called twice
            if ((element.flags & UIElementFlags.Initialized) == 0) {
                element.flags |= UIElementFlags.Initialized;
                element.OnReady();
            }
        }

        public static void DestroyElement(UIElement element) {
            element.view.Application.DoDestroyElement(element);
        }

        protected void DoDestroyElement(UIElement element) {
            if ((element.flags & UIElementFlags.Destroyed) != 0) {
                return;
            }

            element.flags |= UIElementFlags.Destroyed;
            element.flags &= ~(UIElementFlags.Enabled);

            if (element.children != null && element.children.Count != 0) {
                m_ElementTree.TraversePostOrder(element, (node) => {
                    node.flags |= UIElementFlags.Destroyed;
                    node.flags &= ~(UIElementFlags.Enabled);
                }, true);

                // traverse after setting all child flags for safety
                m_ElementTree.TraversePostOrder(element, (node) => { node.OnDestroy(); }, true);
            }
            else {
                element.OnDestroy();
            }

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementDestroyed(element);
            }

            RemoveUpdateDepthIndices(element);

            
            if (element.parent != null) {
                element.parent.children.Remove(element);
                for (int i = 0; i < element.parent.children.Count; i++) {
                    element.parent.children[i].siblingIndex = i;
                }
            }

            m_ElementTree.TraversePreOrder(element, (node) => {
                LightListPool<UIElement>.Release(ref node.children);
                // todo -- if child is poolable, pool it here
            }, true);


            // todo -- if element is poolable, pool it here
            
            onElementDestroyed?.Invoke(element);

        }

        internal void DestroyChildren(UIElement element) {
            // todo - handle template parent :(

            if ((element.flags & UIElementFlags.Destroyed) != 0) {
                return;
            }

            if (element.children == null || element.children.Count == 0) {
                return;
            }

            for (int i = 0; i < element.children.Count; i++) {
                UIElement child = element.children[i];
                child.flags |= UIElementFlags.Destroyed;
                child.flags &= ~(UIElementFlags.Enabled);

                m_ElementTree.TraversePostOrder(child, (node) => {
                    node.flags |= UIElementFlags.Destroyed;
                    node.flags &= ~(UIElementFlags.Enabled);
                }, true);
                
                m_ElementTree.TraversePostOrder(child, (node) => node.OnDestroy(), true);
            }

            // todo I think this is wrong, should be done just for each child?
            RemoveUpdateDepthIndicesStep(element);

            for (int i = 0; i < element.children.Count; i++) {
                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnElementDestroyed(element.children[i]);
                }
            }

            for (int i = 0; i < element.children.Count; i++) {
                m_ElementTree.TraversePostOrder(element.children[i], (node) => {
                    LightListPool<UIElement>.Release(ref node.children);
                }, true);
                m_ElementTree.RemoveHierarchy(element.children[i]);
            }

            element.children.Clear();
        }

        protected void RemoveUpdateDepthIndices(UIElement element) {
            List<UIElement> list = m_DepthMap[element.depth];
            list.RemoveAt(element.depthIndex);
            for (int i = element.depthIndex; i < list.Count; i++) {
                list[i].depthIndex = i;
            }

            RemoveUpdateDepthIndicesStep(element);
        }

        protected void RemoveUpdateDepthIndicesStep(UIElement element) {
            if (element.children == null || element.children.Count == 0) {
                return;
            }

            List<UIElement> list = m_DepthMap[element.depth + 1];
            int idx = element.children[0].depthIndex;
            list.RemoveRange(idx, element.children.Count);

            for (int i = idx; i < list.Count; i++) {
                list[i].depthIndex = i;
            }

            for (int i = idx; i < element.children.Count; i++) {
                RemoveUpdateDepthIndicesStep(element.children[i]);
            }
        }

        public void Update() {
            m_BindingSystem.OnUpdate();
            m_StyleSystem.OnUpdate();
            m_LayoutSystem.OnUpdate();
            m_InputSystem.OnUpdate();
            m_RenderSystem.OnUpdate();

            m_ElementTree.ConditionalTraversePreOrder((element) => {
                if (element == null) return true;
                if (element.isDisabled) return false;
                element.OnUpdate();
                return true;
            });

            onUpdate?.Invoke();
        }

        public static void EnableElement(UIElement element) {
            element.view.Application.DoEnableElement(element);
        }

        public static void DisableElement(UIElement element) {
            element.view.Application.DoDisableElement(element);
        }

        private static void RunEnableBinding(UIElement element) {
            Binding[] enabledBindings = element.OriginTemplate?.triggeredBindings;

            if (enabledBindings != null) {
                for (int i = 0; i < enabledBindings.Length; i++) {
                    if (enabledBindings[i].bindingType == BindingType.OnEnable) {
                        enabledBindings[i].Execute(element, element.templateContext);
                    }
                }
            }
        }

        public void DoEnableElement(UIElement element) {
            // no-op for already enabled elements
            if (element.isSelfEnabled) return;

            element.flags |= UIElementFlags.Enabled;

            // if element is not enabled (ie has a disabled ancestor), no-op 
            if (!element.isEnabled) return;

            element.OnEnable();
            RunEnableBinding(element);

            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            m_ElementTree.ConditionalTraversePreOrder(element, (child) => {
                child.flags |= UIElementFlags.AncestorEnabled;
                if (child.isSelfDisabled) return false;

                child.OnEnable(); // todo -- maybe enqueue and flush calls after so we don't have buffer problems
                RunEnableBinding(child);

                return true;
            });

            foreach (ISystem system in m_Systems) {
                system.OnElementEnabled(element);
            }

            onElementEnabled?.Invoke(element);
        }

        public void DoDisableElement(UIElement element) {
            // no-op for already disabled elements
            if (element.isSelfDisabled) return;

            element.flags &= ~(UIElementFlags.Enabled);

            // if element was already disabled via ancestor, no-op
            if (element.hasDisabledAncestor) {
                return;
            }

            element.OnDisable();

            m_ElementTree.ConditionalTraversePreOrder(element, (child) => {
                child.flags &= ~(UIElementFlags.AncestorEnabled);
                if (child.isSelfDisabled) return false;

                child.OnDisable(); // todo -- enqueue for later

                return true;
            });

            foreach (ISystem system in m_Systems) {
                system.OnElementDisabled(element);
            }

            element.view.InvokeElementDisabled(element);
            onElementDisabled?.Invoke(element);
        }

        public UIElement GetElement(int elementId) {
            return m_ElementTree.GetItem(elementId);
        }

        protected class DepthIndexComparer : IComparer<UIElement> {

            public int Compare(UIElement x, UIElement y) {
                if (x.parent == y.parent) {
                    return x.siblingIndex > y.siblingIndex ? 1 : -1;
                }

                UIElement p0 = x.parent;
                UIElement p1 = y.parent;

                while (p0.parent != p1.parent) {
                    p0 = p0.parent;
                    p1 = p1.parent;
                }

                return p0.siblingIndex > p1.siblingIndex ? 1 : -1;
            }

        }

    }

}