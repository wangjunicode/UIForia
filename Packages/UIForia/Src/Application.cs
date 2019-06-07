using System;
using System.Collections.Generic;
using Src.Systems;
using UIForia.Animation;
using UIForia.AttributeProcessors;
using UIForia.Bindings;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomPainterAttribute : Attribute {

        public readonly string name;

        public CustomPainterAttribute(string name) {
            this.name = name;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomScrollbarAttribute : Attribute {

        public readonly string name;

        public CustomScrollbarAttribute(string name) {
            this.name = name;
        }

    }

    public abstract class Application {

        public readonly string id;
        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;
        private string templateRootPath;

        protected readonly BindingSystem m_BindingSystem;
        protected readonly IStyleSystem m_StyleSystem;
        protected ILayoutSystem m_LayoutSystem;
        protected IRenderSystem m_RenderSystem;
        protected IInputSystem m_InputSystem;
        protected RoutingSystem m_RoutingSystem;
        protected AnimationSystem m_AnimationSystem;

        public readonly StyleSheetImporter styleImporter;
        private readonly IntMap<UIElement> elementMap;
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

        protected internal readonly List<UIView> m_Views;

        public static readonly List<IAttributeProcessor> s_AttributeProcessors;

        internal static readonly Dictionary<string, ISVGXElementPainter> s_CustomPainters;
        internal static readonly Dictionary<string, Scrollbar> s_Scrollbars;

        public readonly TemplateParser templateParser;

        private static readonly LightList<Application> s_ApplicationList;

        private readonly UITaskSystem m_BeforeUpdateTaskSystem;
        private readonly UITaskSystem m_AfterUpdateTaskSystem;

        protected readonly SkipTree<UIElement> updateTree;

        static Application() {
            ArrayPool<UIElement>.SetMaxPoolSize(64);
            s_AttributeProcessors = new List<IAttributeProcessor>();
            s_ApplicationList = new LightList<Application>();
            s_CustomPainters = new Dictionary<string, ISVGXElementPainter>();
            s_Scrollbars = new Dictionary<string, Scrollbar>();
        }

        protected Application(string id, string templateRootPath = null) {
            this.id = id;
            this.templateRootPath = templateRootPath;

            // todo -- exceptions in constructors aren't good practice
            if (s_ApplicationList.Find(id, (app, _id) => app.id == _id) != null) {
                throw new Exception($"Applications must have a unique id. Id {id} was already taken.");
            }

            s_ApplicationList.Add(this);

            this.m_Systems = new List<ISystem>();
            this.m_Views = new List<UIView>();
            this.updateTree = new SkipTree<UIElement>();

            m_StyleSystem = new StyleSystem();
            m_BindingSystem = new BindingSystem();
            m_LayoutSystem = new LayoutSystem(this, m_StyleSystem);
            m_InputSystem = new GameInputSystem(m_LayoutSystem);
//            m_RenderSystem = new VertigoRenderSystem(Camera.current, m_LayoutSystem, m_StyleSystem); 
            m_RenderSystem = new SVGXRenderSystem(null, m_LayoutSystem);
            m_RoutingSystem = new RoutingSystem();
            m_AnimationSystem = new AnimationSystem();

            styleImporter = new StyleSheetImporter(this);
            templateParser = new TemplateParser(this);

            elementMap = new IntMap<UIElement>();

            m_Systems.Add(m_StyleSystem);
            m_Systems.Add(m_BindingSystem);
            m_Systems.Add(m_RoutingSystem);
            m_Systems.Add(m_InputSystem);
            m_Systems.Add(m_AnimationSystem);
            m_Systems.Add(m_LayoutSystem);
            m_Systems.Add(m_RenderSystem);

            m_BeforeUpdateTaskSystem = new UITaskSystem();
            m_AfterUpdateTaskSystem = new UITaskSystem();
            onApplicationCreated?.Invoke(this);
        }

        internal static void ProcessClassAttributes(Type type, IEnumerable<Attribute> attrs) {
            foreach (Attribute attr in attrs) {
                if (attr is CustomPainterAttribute paintAttr) {
                    if (type.GetConstructor(Type.EmptyTypes) == null || type.GetInterface(nameof(ISVGXElementPainter)) == null) {
                        throw new Exception($"Classes marked with [{nameof(CustomPainterAttribute)}] must provide a parameterless constructor" +
                                            $" and the class must implement {nameof(ISVGXElementPainter)}. Ensure that {type.FullName} conforms to these rules");
                    }

                    if (s_CustomPainters.ContainsKey(paintAttr.name)) {
                        throw new Exception($"Failed to register a custom painter with the name {paintAttr.name} from type {type.FullName} because it was already registered.");
                    }

                    s_CustomPainters.Add(paintAttr.name, (ISVGXElementPainter) Activator.CreateInstance(type));
                }
                else if (attr is CustomScrollbarAttribute scrollbarAttr) {
                    if (type.GetConstructor(Type.EmptyTypes) == null || !(typeof(Scrollbar)).IsAssignableFrom(type)) {
                        throw new Exception($"Classes marked with [{nameof(CustomScrollbarAttribute)}] must provide a parameterless constructor" +
                                            $" and the class must extend {nameof(Scrollbar)}. Ensure that {type.FullName} conforms to these rules");
                    }

                    if (s_Scrollbars.ContainsKey(scrollbarAttr.name)) {
                        throw new Exception($"Failed to register a custom scrollbar with the name {scrollbarAttr.name} from type {type.FullName} because it was already registered.");
                    }

                    s_Scrollbars.Add(scrollbarAttr.name, (Scrollbar) Activator.CreateInstance(type));
                }
            }
        }

        public string TemplateRootPath {
            get {
                if (templateRootPath == null) {
                    return UnityEngine.Application.dataPath;
                }

                return templateRootPath;
            }
            set { templateRootPath = value; }
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

        private int nextViewId = 0;

        public UIView CreateView(string name, Rect rect, Type type, string template = null) {
            UIView view = new UIView(nextViewId++, name, this, rect, m_Views.Count, type, template);

            m_Views.Add(view);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }

            view.Initialize();

            onViewAdded?.Invoke(view);
            return view;
        }

        public UIView CreateView(string name, Rect rect) {
            UIView view = new UIView(nextViewId++, name, this, rect, m_Views.Count);

            m_Views.Add(view);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }

            view.Initialize();

            onViewAdded?.Invoke(view);
            return view;
        }

        public UIView RemoveView(UIView view) {
            if (!m_Views.Remove(view)) return null;

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewRemoved(view);
            }

            onViewRemoved?.Invoke(view);
            DestroyElement(view.rootElement);
            return view;
        }

        public UIElement CreateElement(Type type) {
            if (type == null) {
                return null;
            }

            return templateParser.GetParsedTemplate(type)?.Create();
        }

        public T CreateElement<T>() where T : UIElement {
            return templateParser.GetParsedTemplate(typeof(T))?.Create() as T;
        }

        public void Refresh() {
            onWillRefresh?.Invoke();

            foreach (ISystem system in m_Systems) {
                system.OnReset();
            }

            onReady = null;
            onUpdate = null;

            elementMap.Clear();
            templateParser.Reset();
            styleImporter.Reset();
            ResourceManager.Reset();

            m_AfterUpdateTaskSystem.OnReset();
            m_BeforeUpdateTaskSystem.OnReset();

            // todo -- store root view, rehydrate. kill the rest
            for (int i = 0; i < m_Views.Count; i++) {
                // RegisterElement(m_Views[i].RootElement);

                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnViewAdded(m_Views[i]);
                }

                m_Views[i].Initialize();
            }

            onRefresh?.Invoke();
            onNextRefresh?.Invoke();
            onNextRefresh = null;
            onReady?.Invoke();
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

        public static void DestroyElement(UIElement element) {
            element.View.Application.DoDestroyElement(element);
        }

        internal void DoDestroyElement(UIElement element) {
            if ((element.flags & UIElementFlags.Destroyed) != 0) {
                return;
            }

            LightStack<UIElement> stack = new LightStack<UIElement>();
            LightList<UIElement> toInternalDestroy = LightListPool<UIElement>.Get();

            stack.Push(element);

            while (stack.Count > 0) {
                UIElement current = stack.PopUnchecked();

                UIElement[] children = current.children.Array;
                int childCount = current.children.Count;
                for (int i = childCount - 1; i >= 0; i--) {
                    stack.Push(children[i]);
                }

                if (!current.isDestroyed) {
                    current.flags |= UIElementFlags.Destroyed;
                    current.OnDestroy();
                    toInternalDestroy.Add(current);
                }
            }


            if (element.parent != null) {
                element.parent.children.Remove(element);
                for (int i = 0; i < element.parent.children.Count; i++) {
                    element.parent.children[i].siblingIndex = i;
                }
            }

            updateTree.RemoveHierarchy(element);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementDestroyed(element);
            }

            if (toInternalDestroy.Count > 0) {
                UIView view = toInternalDestroy[0].View;
                for (int i = 0; i < toInternalDestroy.Count; i++) {
                    view.ElementDestroyed(toInternalDestroy[i]);
                    toInternalDestroy[i].InternalDestroy();
                    elementMap.Remove(toInternalDestroy[i].id);
                }
            }

            LightListPool<UIElement>.Release(ref toInternalDestroy);

            onElementDestroyed?.Invoke(element);

            // todo -- if element is poolable, pool it here
            LightStack<UIElement>.Release(ref stack);
        }

        internal void DestroyChildren(UIElement element) {
            if (element.isDestroyed) {
                return;
            }

            if (element.children == null || element.children.Count == 0) {
                return;
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            LightList<UIElement> toInternalDestroy = LightListPool<UIElement>.Get();

            int childCount = element.children.Count;
            UIElement[] children = element.children.Array;

            for (int i = 0; i < childCount; i++) {
                stack.Push(children[i]);
                updateTree.RemoveHierarchy(children[i]);
            }

            while (stack.Count > 0) {
                UIElement current = stack.PopUnchecked();

                if (!current.isDestroyed) {
                    current.flags &= ~(UIElementFlags.Enabled);
                    current.flags |= UIElementFlags.Destroyed;
                    current.OnDestroy();
                    toInternalDestroy.Add(current);
                }

                childCount = current.children.Count;
                children = current.children.Array;

                for (int i = childCount - 1; i >= 0; i--) {
                    stack.Push(children[i]);
                }
            }

            for (int i = 0; i < element.children.Count; i++) {
                for (int j = 0; j < m_Systems.Count; j++) {
                    m_Systems[j].OnElementDestroyed(element.children[i]);
                }
            }

            if (toInternalDestroy.Count > 0) {
                UIView view = toInternalDestroy[0].View;
                for (int i = 0; i < toInternalDestroy.Count; i++) {
                    view.ElementDestroyed(toInternalDestroy[i]);
                    toInternalDestroy[i].InternalDestroy();
                    elementMap.Remove(toInternalDestroy[i].id);
                }
            }

            LightListPool<UIElement>.Release(ref toInternalDestroy);
            element.children.Clear();
        }

        public void Update() {
            // todo -- if parent changed we don't want to double update, best to iterate to array & diff a frame id
            updateTree.ConditionalTraversePreOrder(Time.frameCount, (element, frameId) => {
                if (element == null) return true; // when would element be null? root?
                if (element.isDisabled) return false;
                if (!element.isReady) return true;
                element.OnUpdate();
                return true;
            });

            m_AnimationSystem.OnUpdate();

            m_BindingSystem.OnUpdate();

            m_StyleSystem.OnUpdate();

            m_LayoutSystem.OnUpdate();

            m_InputSystem.OnUpdate();

            m_BeforeUpdateTaskSystem.OnUpdate();

            m_InputSystem.OnLateUpdate();

            m_RoutingSystem.OnUpdate();

            m_RenderSystem.OnUpdate();

            m_AfterUpdateTaskSystem.OnUpdate();

            onUpdate?.Invoke();

            m_Views[0].SetSize(Screen.width, Screen.height);
        }

        /// <summary>
        /// Note: you don't need to remove tasks from the system. Any canceled or otherwise completed task gets removed
        /// from the system automatically.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public UITask RegisterBeforeUpdateTask(UITask task) {
            return m_BeforeUpdateTaskSystem.AddTask(task);
        }

        /// <summary>
        /// Note: you don't need to remove tasks from the system. Any canceled or otherwise completed task gets removed
        /// from the system automatically.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public UITask RegisterAfterUpdateTask(UITask task) {
            return m_AfterUpdateTaskSystem.AddTask(task);
        }

        public static void EnableElement(UIElement element) {
            element.View.Application.DoEnableElement(element);
        }

        public static void DisableElement(UIElement element) {
            element.View.Application.DoDisableElement(element);
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
            if (element.isDestroyed) {
                return;
            }

            element.flags |= UIElementFlags.Enabled;
            // if element is not enabled (ie has a disabled ancestor), no-op 
            if (!element.isEnabled) return;

            int targetPhase = -1;
            UIElement ptr = element.parent;
            if (!ptr.isReady) {
                while (ptr != null) {
                    if (ptr.enablePhase != 0) {
                        targetPhase = ptr.enablePhase;
                        break;
                    }

                    ptr = ptr.parent;
                }
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            UIElement[] children;
            int childCount;

            stack.Push(element);

            element.enablePhase = 1;
            while (stack.Count > 0) {
                UIElement child = stack.PopUnchecked();

                if (child.isDestroyed) {
                    continue;
                }

                if (child.parent.isEnabled) {
                    child.flags |= UIElementFlags.AncestorEnabled;
                }

                if (child.isEnabled && !child.isCreated) {
                    child.flags |= UIElementFlags.Created;
                    child.OnCreate();
                    child.View.ElementCreated(child);
                }

                if (child.isDisabled) {
                    continue;
                }

                children = child.children.Array;
                childCount = child.children.Count;
                for (int i = childCount - 1; i >= 0; i--) {
                    stack.Push(children[i]);
                }
            }

            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            stack.Push(element);
            element.enablePhase = 2;

            if (!element.isEnabled || (targetPhase != -1 && targetPhase <= 2)) {
                element.enablePhase = 0;
                LightStack<UIElement>.Release(ref stack);
                return;
            }
            
            element.flags |= UIElementFlags.AncestorEnabled;

            foreach (ISystem system in m_Systems) {
                system.OnElementEnabled(element);
            }

            while (stack.Count > 0) {
                UIElement child = stack.PopUnchecked();
                child.flags |= UIElementFlags.AncestorEnabled;

                if (child.isSelfDisabled || child.isDestroyed) {
                    continue;
                }

                child.flags |= UIElementFlags.HasBeenEnabled;

                child.OnEnable();

                if (child.isEnabled) {
                    RunEnableBinding(child);
                    // need this isEnabled check in case a binding disabled the element
                    if (!child.isEnabled) {
                        continue;
                    }

                    children = child.children.Array;
                    childCount = child.children.Count;
                    for (int i = childCount - 1; i >= 0; i--) {
                        stack.Push(children[i]);
                    }
                }
            }

            element.enablePhase = 3;

            if (targetPhase != -1 && targetPhase <= 3) {
                element.enablePhase = 0;
                LightStack<UIElement>.Release(ref stack);
                return;
            }

            if (element.isEnabled) {
                element.View.ElementHierarchyEnabled(element);
                onElementEnabled?.Invoke(element);

                stack.Push(element);

                while (stack.Count > 0) {
                    UIElement child = stack.PopUnchecked();

                    if (child.isDestroyed) {
                        continue;
                    }

                    if (child.isEnabled && !child.isReady) {
                        child.flags |= UIElementFlags.Ready;
                        child.OnReady();
                        child.View.ElementReady(element);
                    }

                    if (child.isDisabled) {
                        continue;
                    }

                    children = child.children.Array;
                    childCount = child.children.Count;
                    for (int i = childCount - 1; i >= 0; i--) {
                        stack.Push(children[i]);
                    }
                }
            }

            element.enablePhase = 0;
            LightStack<UIElement>.Release(ref stack);
        }

        // todo bad things happen if we add children during disabling or enabling (probably)

        public void DoDisableElement(UIElement element) {
            // no-op for already disabled elements
            if (!element.isCreated || element.isDisabled) {
                element.flags &= ~(UIElementFlags.Enabled);
                return;
            }

            element.flags &= ~(UIElementFlags.Enabled);

            // if element was already disabled via ancestor, no-op
            if (element.hasDisabledAncestor) {
                return;
            }

            element.OnDisable();

            if (element.isEnabled) {
                return;
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            UIElement[] children = element.children.Array;
            int childCount = element.children.Count;

            for (int i = childCount - 1; i >= 0; i--) {
                stack.Push(children[i]);
            }

            while (stack.Count > 0) {
                UIElement child = stack.PopUnchecked();
                child.flags &= ~(UIElementFlags.AncestorEnabled);

                if (!child.isCreated) {
                    continue;
                }

                if ((child.flags & UIElementFlags.HasBeenEnabled) != 0) {
                    child.OnDisable();
                }

                if (child.isDisabled) {
                    children = child.children.Array;
                    childCount = child.children.Count;
                    for (int i = childCount - 1; i >= 0; i--) {
                        stack.Push(children[i]);
                    }
                }
            }

            LightStack<UIElement>.Release(ref stack);

            if (element.isDisabled) {
                foreach (ISystem system in m_Systems) {
                    system.OnElementDisabled(element);
                }

                element.View.ElementHierarchyDisabled(element);
                onElementDisabled?.Invoke(element);
            }
        }

        public UIElement GetElement(int elementId) {
            return elementMap.GetOrDefault(elementId);
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

        public static bool HasCustomPainter(string name) {
            return s_CustomPainters.ContainsKey(name);
        }

        public static ISVGXElementPainter GetCustomPainter(string name) {
            return s_CustomPainters.GetOrDefault(name);
        }

        public static Scrollbar GetCustomScrollbar(string name) {
            if (string.IsNullOrEmpty(name)) {
                return s_Scrollbars["UIForia.Default"];
            }

            return s_Scrollbars.GetOrDefault(name);
        }

        public void Animate(UIElement element, AnimationData animation) {
            m_AnimationSystem.Animate(element, animation);
        }

        public UIView[] GetViews() {
            return m_Views.ToArray();
        }

        internal void InsertChild(UIElement parent, UIElement child, uint index) {
            if (child.parent != null) {
                throw new NotImplementedException("Reparenting is not supported");
            }

            bool hasView = child.View != null;

            // we don't know the hierarchy at this point.
            // could be made up of a mix of elements in various states

            child.parent = parent;
            parent.children.Insert((int) index, child);

            if (hasView) {
                throw new NotImplementedException("Changing views is not supported");
            }

            bool parentEnabled = parent.isEnabled;

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            UIView view = parent.View;
            stack.Push(child);

            view.BeginAddingElements();

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                current.depth = current.parent.depth + 1;

                // todo -- we don't support changing views or any sort of re-parenting

                current.View = view;

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

                UIElement.UIElementTypeData typeData = current.GetTypeData();

                // todo -- build tree subsection & add it all at once
                if (typeData.requiresUpdate) {
                    updateTree.AddItem(current);
                }

                elementMap[current.id] = current;

                if (!current.isRegistered) {
                    current.style.Initialize();
                    current.flags |= UIElementFlags.Registered;
                    for (int i = 0; i < m_Systems.Count; i++) {
                        m_Systems[i].OnElementCreated(current);
                    }

                    view.ElementRegistered(current);
                    onElementRegistered?.Invoke(current);
                }

                UIElement[] children = current.children.Array;
                int childCount = current.children.Count;
                // reverse this?
                for (int i = 0; i < childCount; i++) {
                    children[i].siblingIndex = i;
                    stack.Push(children[i]);
                }
            }

            for (int i = 0; i < parent.children.Count; i++) {
                parent.children[i].siblingIndex = i;
            }

            view.EndAddingElements();

            LightStack<UIElement>.Release(ref stack);

            if (parentEnabled && child.isEnabled) {
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child);
            }
        }

    }

}