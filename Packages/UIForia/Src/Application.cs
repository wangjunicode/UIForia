using System;
using System.Collections.Generic;
using UIForia.AttributeProcessors;
using UIForia.Bindings;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public abstract class Application {

        public readonly string id;
        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;

        protected readonly BindingSystem m_BindingSystem;
        protected readonly IStyleSystem m_StyleSystem;
        protected ILayoutSystem m_LayoutSystem;
        protected IRenderSystem m_RenderSystem;
        protected IInputSystem m_InputSystem;
        protected RoutingSystem m_RoutingSystem;

        public readonly StyleSheetImporter styleImporter;

        protected readonly SkipTree<UIElement> m_ElementTree;
        protected readonly SkipTree<UIElement> m_UpdateTree;

        protected readonly List<ISystem> m_Systems;

        public event Action<UIElement> onElementRegistered;
        public event Action<UIElement> onElementCreated;
        public event Action<UIElement> onElementDestroyed;
        public event Action<UIElement> onElementEnabled;
        public event Action<UIElement> onElementDisabled;

        public event Action onWillRefresh;
        public event Action onRefresh;
        public event Action onUpdate;
        public event Action onReady;
        public event Action onDestroy;
        public event Action onNextRefresh;
        public event Action<UIView> onViewAdded;
        public event Action<UIView> onViewRemoved;

        public static event Action<Application> onApplicationCreated;
        public static event Action<Application> onApplicationDestroyed;

        protected readonly List<UIView> m_Views;

        public static readonly List<IAttributeProcessor> s_AttributeProcessors;
        private static readonly Dictionary<Type, bool> s_RequiresUpdateMap;
        
        internal static readonly Dictionary<string, ISVGXElementPainter> s_CustomPainters;
        
        public readonly TemplateParser templateParser;

        private static readonly LightList<Application> s_ApplicationList;

        static Application() {
            ArrayPool<UIElement>.SetMaxPoolSize(64);
            s_RequiresUpdateMap = new Dictionary<Type, bool>();
            s_AttributeProcessors = new List<IAttributeProcessor>();
            s_ApplicationList = new LightList<Application>();
            s_CustomPainters = new Dictionary<string, ISVGXElementPainter>();
        }

        protected Application(string id) {
            this.id = id;

            if (s_ApplicationList.Find(id, (app, _id) => app.id == _id) != null) {
                throw new Exception($"Applications must have a unique id. Id {id} was already taken.");
            }

            s_ApplicationList.Add(this);
            
            this.m_Systems = new List<ISystem>();
            this.m_ElementTree = new SkipTree<UIElement>();
            this.m_Views = new List<UIView>();
            this.m_UpdateTree = new SkipTree<UIElement>();

            m_StyleSystem = new StyleSystem();
            m_BindingSystem = new BindingSystem();
            m_LayoutSystem = new LayoutSystem(m_StyleSystem);
            m_InputSystem = new DefaultInputSystem(m_LayoutSystem, m_StyleSystem);
            m_RenderSystem = new SVGXRenderSystem(null, m_LayoutSystem);
            m_RoutingSystem = new RoutingSystem();

            styleImporter = new StyleSheetImporter(this);
            templateParser = new TemplateParser(this);

            m_Systems.Add(m_StyleSystem);
            m_Systems.Add(m_BindingSystem);
            m_Systems.Add(m_RoutingSystem);
            m_Systems.Add(m_InputSystem);
            m_Systems.Add(m_LayoutSystem);
            m_Systems.Add(m_RenderSystem);
            
            onApplicationCreated?.Invoke(this);
            
        }

        public IStyleSystem StyleSystem => m_StyleSystem;
        public BindingSystem BindingSystem => m_BindingSystem;
        public IRenderSystem RenderSystem => m_RenderSystem;
        public ILayoutSystem LayoutSystem => m_LayoutSystem;
        public IInputSystem InputSystem => m_InputSystem;
        public RoutingSystem RoutingSystem => m_RoutingSystem;

        public Camera Camera { get; private set; }

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

        internal UIElement CreateChildElement(UIElement parent, Type type) {
            if (type == null) {
                return null;
            }

            ParsedTemplate template = templateParser.GetParsedTemplate(type);

            if (template == null) {
                return null;
            }

            UIElement retn = template.Create();

            retn.templateContext.rootObject = parent;
            retn.parent = parent;
            parent.children.Add(retn);
            RegisterElement(retn);

            return retn;
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

            InitHierarchy(element);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementCreated(element);
            }

            onElementRegistered?.Invoke(element);

            InvokeAttributeProcessors(element);
            InvokeOnCreate(element);
            InvokeOnReady(element);
            onElementCreated?.Invoke(element);
        }

        public void Refresh() {
            onWillRefresh?.Invoke();
            foreach (ISystem system in m_Systems) {
                system.OnReset();
            }

            onReady = null;
            onUpdate = null;

            m_ElementTree.TraversePreOrder((el) => el.OnDestroy());

            m_ElementTree.Clear();

            templateParser.Reset();
            styleImporter.Reset();
            ResourceManager.Reset(); // todo use 1 instance per application

            for (int i = 0; i < m_Views.Count; i++) {
                m_Views[i].Refresh();
                RegisterElement(m_Views[i].RootElement);

                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnViewAdded(m_Views[i]);
                }
            }

            onRefresh?.Invoke();
            onNextRefresh?.Invoke();
            onNextRefresh = null;
        }

        public void Destroy() {
            onApplicationDestroyed?.Invoke(this);
            onDestroy?.Invoke();

            foreach (ISystem system in m_Systems) {
                system.OnDestroy();
            }

            foreach (UIView view in m_Views) {
                view.Destroy();
            }

            m_ElementTree.Clear();
            onRefresh = null;
            onNextRefresh = null;
            onReady = null;
            onUpdate = null;
            onDestroy = null;
            onNextRefresh = null;
            onElementCreated = null;
            onElementEnabled = null;
            onElementDisabled = null;
            onElementDestroyed = null;
            onElementRegistered = null;
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

            m_ElementTree.AddItem(element);
            Type elementType = element.GetType();
            if (!s_RequiresUpdateMap.TryGetValue(elementType, out bool requiresUpdate)) {
                requiresUpdate = ReflectionUtil.IsOverride(elementType.GetMethod(nameof(UIElement.OnUpdate)));
                s_RequiresUpdateMap[elementType] = requiresUpdate;
            }

            if (requiresUpdate) {
                m_UpdateTree.AddItem(element);
            }

            LightList<UIElement> children = element.children;

            if (children == null || children.Count == 0) {
                return;
            }

            for (int i = 0; i < children.Count; i++) {
                children[i].siblingIndex = i;
                InitHierarchy(children[i]);
            }
        }

        private static void InvokeAttributeProcessors(UIElement element) {
            List<ElementAttribute> attributes = element.GetAttributes();

            // todo -- the origin template can figure out which processors to invoke at compile time, saves potentially a lot of cycles

            for (int i = 0; i < s_AttributeProcessors.Count; i++) {
                s_AttributeProcessors[i].Process(element, element.OriginTemplate, attributes);
            }

            if (element.children == null) return;

            for (int i = 0; i < element.children.Count; i++) {
                InvokeAttributeProcessors(element.children[i]);
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

            if (element.parent != null) {
                element.parent.children.Remove(element);
                for (int i = 0; i < element.parent.children.Count; i++) {
                    element.parent.children[i].siblingIndex = i;
                }
            }

            onElementDestroyed?.Invoke(element);

            // todo -- if element is poolable, pool it here
            m_ElementTree.TraversePreOrder(element, (el) => { el.InternalDestroy(); }, true);

            m_UpdateTree.RemoveHierarchy(element);
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


            for (int i = 0; i < element.children.Count; i++) {
                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnElementDestroyed(element.children[i]);
                }
            }

            for (int i = 0; i < element.children.Count; i++) {
                m_ElementTree.TraversePostOrder(element.children[i], (node) => { LightListPool<UIElement>.Release(ref node.children); }, true);
                m_ElementTree.RemoveHierarchy(element.children[i]);
            }

            element.children.Clear();
        }

        public void Update() {
            m_BindingSystem.OnUpdate();
            m_StyleSystem.OnUpdate();
            m_LayoutSystem.OnUpdate();
            m_InputSystem.OnUpdate();
            m_RenderSystem.OnUpdate();

            m_RoutingSystem.OnUpdate();

            m_UpdateTree.ConditionalTraversePreOrder((element) => {
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

            if ((element.flags & UIElementFlags.Initialized) != 0) {
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
            }

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

            if ((element.flags & UIElementFlags.Initialized) != 0) {
                element.OnDisable();
            }

            m_ElementTree.ConditionalTraversePreOrder(element, (child) => {
                child.flags &= ~(UIElementFlags.AncestorEnabled);
                if (child.isSelfDisabled) return false;

                if ((child.flags & UIElementFlags.Initialized) != 0) {
                    child.OnDisable(); // todo -- enqueue for later
                }

                return true;
            });


            foreach (ISystem system in m_Systems) {
                system.OnElementDisabled(element);
            }

            if ((element.flags & UIElementFlags.Initialized) != 0) {
                element.view.InvokeElementDisabled(element);
                onElementDisabled?.Invoke(element);
            }
        }

        public UIElement GetElement(int elementId) {
            return m_ElementTree.GetItem(elementId);
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnAttributeSet(element, attributeName, currentValue, previousValue);
            }
        }

        public static void RefreshAll() {
            for (int i = 0; i < s_ApplicationList.Count; i++) {
                s_ApplicationList[i].Refresh();
            }
        }

        public UIView GetView(int i) {
            if (i < 0 || i >= m_Views.Count) return null;
            return m_Views[i];
        }

        public static Application Find(string appId) {
            return s_ApplicationList.Find(appId, (app, _id) => app.id == _id);
        }

        public static void RegisterCustomPainter(string name, ISVGXElementPainter painter) {
            s_CustomPainters[name] = painter;
        }

        public static bool HasCustomPainter(string name) {
            return s_CustomPainters.ContainsKey(name);
        }

        public static ISVGXElementPainter GetCustomPainter(string name) {
            return s_CustomPainters.GetOrDefault(name);
        }
        
    }

}