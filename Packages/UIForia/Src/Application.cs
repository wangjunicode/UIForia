using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        protected readonly SkipTree<UIElement> m_ElementTree;

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

        internal static readonly Dictionary<string, ISVGXElementPainter> s_CustomPainters;
        internal static readonly Dictionary<string, Scrollbar> s_Scrollbars;

        public readonly TemplateParser templateParser;

        private static readonly LightList<Application> s_ApplicationList;

        private readonly UITaskSystem m_BeforeUpdateTaskSystem;
        private readonly UITaskSystem m_AfterUpdateTaskSystem;

        protected readonly SkipTree<UIElement> createTree;
        protected readonly SkipTree<UIElement> enableTree;
        protected readonly SkipTree<UIElement> disableTree;
        protected readonly SkipTree<UIElement> destroyTree;
        protected readonly SkipTree<UIElement> readyTree;
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
            this.m_ElementTree = new SkipTree<UIElement>();
            this.m_Views = new List<UIView>();
            this.updateTree = new SkipTree<UIElement>();
            this.createTree = new SkipTree<UIElement>();
            this.enableTree = new SkipTree<UIElement>();
            this.disableTree = new SkipTree<UIElement>();
            this.destroyTree = new SkipTree<UIElement>();
            this.readyTree = new SkipTree<UIElement>();

            m_StyleSystem = new StyleSystem();
            m_BindingSystem = new BindingSystem();
            m_LayoutSystem = new LayoutSystem(m_StyleSystem);
            m_InputSystem = new GameInputSystem(m_LayoutSystem);
            m_RenderSystem = new SVGXRenderSystem(null, m_LayoutSystem);
            m_RoutingSystem = new RoutingSystem();
            m_AnimationSystem = new AnimationSystem();

            styleImporter = new StyleSheetImporter(this);
            templateParser = new TemplateParser(this);

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

        public UIView AddView(string name, Rect rect, Type type, string template = null) {
            UIView view = new UIView(nextViewId++, name, this, rect, m_Views.Count, type, template);

            m_Views.Add(view);

            RegisterElement(view.RootElement);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }

            onViewAdded?.Invoke(view);
            return view;
        }

        public UIView AddView(string name, Rect rect) {
            UIView view = new UIView(nextViewId++, name, this, rect, m_Views.Count);

            m_Views.Add(view);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }

            onViewAdded?.Invoke(view);
            return view;
        }

        public UIView RemoveView(UIView view) {
            if (!m_Views.Remove(view)) return null;

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewRemoved(view);
            }

            onViewRemoved?.Invoke(view);
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
                Debug.Assert(element.View.RootElement == element, nameof(element.View.RootElement) + " must be null if providing a null parent");

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

            m_AfterUpdateTaskSystem.OnReset();
            m_BeforeUpdateTaskSystem.OnReset();

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

        protected void AddElementToLifeCycleTrees(UIElement element) {
            UIElement.UIElementTypeData typeData = element.GetTypeData();

            m_ElementTree.AddItem(element);
            if (typeData.requiresUpdate) {
                updateTree.AddItem(element);
            }

            if (typeData.requiresCreate && !element.isCreated) {
                createTree.AddItem(element);
            }

            if (typeData.requiresReady && !element.isReady) {
                readyTree.AddItem(element);
            }

            if (typeData.requiresDestroy) {
                destroyTree.AddItem(element);
            }

            if (typeData.requiresEnable) {
                enableTree.AddItem(element);
            }

            if (typeData.requiresDisable) {
                disableTree.AddItem(element);
            }
        }

        protected void InitHierarchy(UIElement element) {
            if (element.parent == null) {
                element.flags |= UIElementFlags.AncestorEnabled;
                element.depth = 0;
            }
            else {
                if (element.parent.isEnabled) {
                    element.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    element.flags &= ~UIElementFlags.AncestorEnabled;
                }

                element.View = element.parent.View;
                element.depth = element.parent.depth + 1;
            }

            AddElementToLifeCycleTrees(element);

            UIElement[] children = element.children.Array;
            int childCount = element.children.Count;

            for (int i = 0; i < childCount; i++) {
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
        }

        private static void InvokeEnable(UIElement element) {
            if (element.children != null) {
                for (int i = 0; i < element.children.Count; i++) {
                    InvokeOnReady(element.children[i]);
                }
            }

            // when creating new children in create or read, this can be called twice
            if ((element.flags & UIElementFlags.HasBeenEnabled) == 0) {
                element.flags |= UIElementFlags.HasBeenEnabled;
                element.OnReady();
            }

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
            if ((element.flags & UIElementFlags.HasBeenEnabled) == 0) {
                element.flags |= UIElementFlags.HasBeenEnabled;
                element.OnReady();
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

            for (int i = 0; i < toInternalDestroy.Count; i++) {
                toInternalDestroy[i].InternalDestroy();
            }
            
            LightListPool<UIElement>.Release(ref toInternalDestroy);
            
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
            updateTree.RemoveHierarchy(element);
            LightStack<UIElement>.Release(ref stack);
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
            // todo -- if parent changed we don't want to double update, best to iterate to array & diff a frame id
            updateTree.ConditionalTraversePreOrder(Time.frameCount, (element, frameId) => {
                if (element == null) return true; // when would element be null? root?
                if (element.isDisabled) return false;
                if (!element.isReady) return true;
                if (element.updateFrameId != frameId) {
                    element.updateFrameId = frameId;
                    element.OnUpdate();
                }

                return true;
            });

            m_AnimationSystem.OnUpdate();

            m_BindingSystem.OnUpdate();

            m_StyleSystem.OnUpdate();

            m_LayoutSystem.OnUpdate();
            m_InputSystem.OnUpdate();

            m_BeforeUpdateTaskSystem.OnUpdate();

            m_BindingSystem.OnLateUpdate();

            m_InputSystem.OnLateUpdate();

            m_RoutingSystem.OnUpdate();

            m_RenderSystem.OnUpdate();

            m_AfterUpdateTaskSystem.OnUpdate();

            onUpdate?.Invoke();
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
//
//            // if element is not enabled (ie has a disabled ancestor), no-op 
            if (!element.isEnabled) return;

//            element.OnEnable();
//
//            if (!element.isEnabled) {
//                return;
//            }
//
//            RunEnableBinding(element);
//
//            if (!element.isEnabled) {
//                return;
//            }


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

            if (targetPhase != -1 && targetPhase <= 2) {
                element.enablePhase = 0;
                LightStack<UIElement>.Release(ref stack);
                return;
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
                foreach (ISystem system in m_Systems) {
                    system.OnElementEnabled(element);
                }

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
            if (!element.isCreated || element.isDisabled) return;

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

                element.View.InvokeElementDisabled(element);
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

        public static void RegisterCustomScrollbar(string name, Scrollbar scrollbar) {
            s_Scrollbars.Add(name, scrollbar);
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
                updateTree.RemoveHierarchy(child);
            }

            // todo -- check if parent is in orphan hierarchy
            // todo -- element.onHierarchyChanged fn
            // todo -- element.onParentChanged
            // todo -- handle element changing views

            bool hasView = child.View != null;

            // we don't know the hierarchy at this point.
            // could be made up of a mix of elements in various states


            child.parent = parent;
            parent.children.Insert((int) index, child);

            if (hasView) {
                // need to deal with changed view / parent    
                return;
            }

            bool parentCreated = parent.isCreated;
            bool parentEnabled = parent.isEnabled;
            bool parentReady = parent.isReady;

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            UIView view = parent.View;
            stack.Push(child);

            LightList<UIElement> viewAddEvents = LightListPool<UIElement>.Get();

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                current.depth = current.parent.depth + 1;

                if (current.View != view) {
                    if (current.View != null) {
                        current.View.RemoveElement(current);
                    }
                    else {
                        viewAddEvents.Add(current);
                    }
                }

                current.View = view;
                // assume application doesn't change for now

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

//                AddElementToLifeCycleTrees(current);

                UIElement[] children = current.children.Array;
                int childCount = current.children.Count;
                for (int i = 0; i < childCount; i++) {
                    stack.Push(children[i]);
                }
            }

            for (int i = (int) index; i < parent.children.Count; i++) {
                parent.children[i].siblingIndex = i;
            }

            view.InvokeAddElements(viewAddEvents);

            LightListPool<UIElement>.Release(ref viewAddEvents);
            LightStack<UIElement>.Release(ref stack);

            if (parentEnabled && child.isEnabled) {
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child);
            }
        }

        public void SetParent(UIElement element, UIElement parent) {
            if (parent == null) {
                // add to orphan list & remove existing binding n stuff    
            }

            if (element.parent == parent) {
                return;
            }

            if (element.parent != null) {
                // remove hierarchy any from trees
                // remove hierarchy any per frame bindings
            }

            // adjust template root?
            element.parent = parent;
            parent.children.Add(element);
            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            element.depth = 0;
            bool ancestorEnabled = parent.isEnabled;
            stack.Push(element);

            if (parent.isEnabled) {
                element.flags |= UIElementFlags.AncestorEnabled;
            }
            else {
                element.flags &= ~UIElementFlags.AncestorEnabled;
            }

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                current.depth = current.parent.depth + 1;
                bool enabled = current.isEnabled;
                UIElement[] children = element.children.Array;
                int childCount = element.children.Count;

                for (int i = 0; i < childCount; i++) {
                    UIElement child = children[i];
                    if (enabled) {
                        child.flags |= UIElementFlags.AncestorEnabled;
                    }

                    stack.Push(child);
                }
            }

            LightStack<UIElement>.Release(ref stack);
            // update enabled / disabled
            // update depth 
            // events
            // life cycle

            // ready & create only get fired when attached
            // update selector map
        }

    }

}