using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src.Systems;
using UIForia.Animation;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {

    /// <summary>
    /// Don't ever use this if you're not a UIForia Dev!
    /// </summary>
    public class UIForiaInternalApplicationSetupProxy {

        private Application application;

        public UIForiaInternalApplicationSetupProxy(Application application) {
            this.application = application;
        }

        public void CallInternalApiToSetupThings(UIElement element) { }

    }

    public abstract class Application {

#if UNITY_EDITOR
        public static List<Application> Applications = new List<Application>();
#endif

        internal Stopwatch layoutTimer = new Stopwatch();
        internal Stopwatch renderTimer = new Stopwatch();
        internal Stopwatch bindingTimer = new Stopwatch();

        public readonly string id;
        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;
        private string templateRootPath;

        internal readonly IStyleSystem m_StyleSystem;
        internal ILayoutSystem m_LayoutSystem;
        internal IRenderSystem m_RenderSystem;
        internal IInputSystem m_InputSystem;
        internal RoutingSystem m_RoutingSystem;
        internal AnimationSystem m_AnimationSystem;
        internal UISoundSystem m_UISoundSystem;
        internal LinqBindingSystem linqBindingSystem;

        protected ResourceManager resourceManager;

        protected readonly List<ISystem> m_Systems;

        public event Action<UIElement> onElementRegistered;

        public event Action<UIElement> onElementDestroyed;

        public event Action<UIElement> onElementEnabled;

        public event Action onWillRefresh;
        public event Action onRefresh;
        public event Action onUpdate;
        public event Action onReady;
        public event Action onDestroy;
        public event Action onNextRefresh;
        public event Action<UIView> onViewAdded;
        public event Action<UIView[]> onViewsSorted;
        public event Action<UIView> onViewRemoved;

        internal CompiledTemplateData templateData;

        internal int frameId;
        protected internal readonly List<UIView> m_Views;

        internal static readonly Dictionary<string, Type> s_CustomPainters;

        private static readonly LightList<Application> s_ApplicationList;

        private readonly UITaskSystem m_BeforeUpdateTaskSystem;
        private readonly UITaskSystem m_AfterUpdateTaskSystem;

        public static readonly UIForiaSettings Settings;
        private ElementPool elementPool;

        private Type lastKnownGoodRootElementType;

        static Application() {
            ArrayPool<UIElement>.SetMaxPoolSize(64);
            s_ApplicationList = new LightList<Application>();
            s_CustomPainters = new Dictionary<string, Type>();
            Settings = Resources.Load<UIForiaSettings>("UIForiaSettings");
            if (Settings == null) {
                throw new Exception("UIForiaSettings are missing. Use the UIForia/Create UIForia Settings to create it");
            }
        }

        // todo -- replace the static version with this one
        public UIForiaSettings settings => Settings;

        protected Application(CompiledTemplateData compiledTemplateData, ResourceManager resourceManager) {
            TemplateSettings templateSettings = compiledTemplateData.templateSettings;
            id = templateSettings.applicationName;

            this.elementPool = new ElementPool();

            this.resourceManager = resourceManager ?? new ResourceManager();

            this.m_Systems = new List<ISystem>();
            this.m_Views = new List<UIView>();

            m_StyleSystem = new StyleSystem();
            m_LayoutSystem = new AwesomeLayoutSystem(this);
            m_InputSystem = new GameInputSystem(m_LayoutSystem);
            m_RenderSystem = new VertigoRenderSystem(Camera.current, this);
            m_RoutingSystem = new RoutingSystem();
            m_AnimationSystem = new AnimationSystem();
            linqBindingSystem = new LinqBindingSystem();

            m_Systems.Add(m_StyleSystem);
            m_Systems.Add(linqBindingSystem);
            m_Systems.Add(m_RoutingSystem);
            m_Systems.Add(m_InputSystem);
            m_Systems.Add(m_AnimationSystem);
            m_Systems.Add(m_LayoutSystem);
            m_Systems.Add(m_RenderSystem);

            m_BeforeUpdateTaskSystem = new UITaskSystem();
            m_AfterUpdateTaskSystem = new UITaskSystem();

#if UNITY_EDITOR
            Applications.Add(this);
#endif

            templateData = compiledTemplateData;
            UIView view = null;

            UIElement rootElement = templateData.templates[0].Invoke(null, new TemplateScope(this, null));

            view = new UIView(this, "Default", rootElement, Matrix4x4.identity, new Size(Screen.width, Screen.height));

            m_Views.Add(view);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewAdded(view);
            }
        }

        public UIView CreateView<T>(string name, Size size, in Matrix4x4 matrix) where T : UIElement {
            Func<UIElement, TemplateScope, UIElement> template = templateData.GetTemplate<T>();

            if (template != null) {
                UIElement element = template.Invoke(null, new TemplateScope(this, null));
                UIView view = new UIView(this, name, element, matrix, size);
                m_Views.Add(view);

                for (int i = 0; i < m_Systems.Count; i++) {
                    m_Systems[i].OnViewAdded(view);
                }

                return view;
            }

            return null;
        }

        public UIView CreateView<T>(string name, Size size) where T : UIElement {
            return CreateView<T>(name, size, Matrix4x4.identity);
        }

        protected Application(string id, string templateRootPath = null, ResourceManager resourceManager = null) {
            this.id = id;
            this.templateRootPath = templateRootPath;
            // todo -- exceptions in constructors aren't good practice
            if (s_ApplicationList.Find(id, (app, _id) => app.id == _id) != null) {
                throw new Exception($"Applications must have a unique id. Id {id} was already taken.");
            }

            s_ApplicationList.Add(this);

            this.elementPool = new ElementPool();

            this.resourceManager = resourceManager ?? new ResourceManager();

            this.m_Systems = new List<ISystem>();
            this.m_Views = new List<UIView>();

            m_StyleSystem = new StyleSystem();
            m_LayoutSystem = new AwesomeLayoutSystem(this);
            m_InputSystem = new GameInputSystem(m_LayoutSystem);
            m_RenderSystem = new VertigoRenderSystem(Camera.current, this);
            m_RoutingSystem = new RoutingSystem();
            m_AnimationSystem = new AnimationSystem();
            linqBindingSystem = new LinqBindingSystem();
            m_UISoundSystem = new UISoundSystem();

            m_Systems.Add(m_StyleSystem);
            m_Systems.Add(linqBindingSystem);
            m_Systems.Add(m_RoutingSystem);
            m_Systems.Add(m_InputSystem);
            m_Systems.Add(m_AnimationSystem);
            m_Systems.Add(m_LayoutSystem);
            m_Systems.Add(m_RenderSystem);

            m_BeforeUpdateTaskSystem = new UITaskSystem();
            m_AfterUpdateTaskSystem = new UITaskSystem();

#if UNITY_EDITOR
            Applications.Add(this);
#endif
        }

        internal static void ProcessClassAttributes(Type type, Attribute[] attrs) {
            for (var i = 0; i < attrs.Length; i++) {
                Attribute attr = attrs[i];
                if (attr is CustomPainterAttribute paintAttr) {
                    if (type.GetConstructor(Type.EmptyTypes) == null || !typeof(RenderBox).IsAssignableFrom(type)) {
                        throw new Exception($"Classes marked with [{nameof(CustomPainterAttribute)}] must provide a parameterless constructor" +
                                            $" and the class must extend {nameof(RenderBox)}. Ensure that {type.FullName} conforms to these rules");
                    }

                    if (s_CustomPainters.ContainsKey(paintAttr.name)) {
                        throw new Exception(
                            $"Failed to register a custom painter with the name {paintAttr.name} from type {type.FullName} because it was already registered.");
                    }

                    s_CustomPainters.Add(paintAttr.name, type);
                }
            }
        }

        public string TemplateRootPath {
            get {
                if (templateRootPath == null) {
                    return string.Empty; // UnityEngine.Application.dataPath;
                }

                return templateRootPath;
            }
            set { templateRootPath = value; }
        }

        public IStyleSystem StyleSystem => m_StyleSystem;
        public IRenderSystem RenderSystem => m_RenderSystem;
        public ILayoutSystem LayoutSystem => m_LayoutSystem;
        public IInputSystem InputSystem => m_InputSystem;
        public RoutingSystem RoutingSystem => m_RoutingSystem;
        public UISoundSystem SoundSystem => m_UISoundSystem;

        public Camera Camera { get; private set; }

        public LinqBindingSystem LinqBindingSystem => linqBindingSystem;
        public ResourceManager ResourceManager => resourceManager;

        public Rect ScreenRect => new Rect {
            x = 0, y = 0, width = Width, height = Height
        };

        public float Width => Screen.width;
        public float Height => Screen.height;

        public void SetCamera(Camera camera) {
            Camera = camera;
            RenderSystem.SetCamera(camera);
        }

        public UIView RemoveView(UIView view) {
            if (!m_Views.Remove(view)) return null;

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewRemoved(view);
            }

            onViewRemoved?.Invoke(view);
            DestroyElement(view.dummyRoot);
            return view;
        }

        public void Refresh() {
            onWillRefresh?.Invoke();

            // kill all but the first view

            foreach (ISystem system in m_Systems) {
                system.OnReset();
            }

            // if the user refreshes the app with a broken template this check and the stored lastKnownGoodRootElementType will 
            // make sure a subsequent refresh can succeed.
            if (m_Views.Count > 0) {
                m_Views.Sort((v1, v2) => v1.id.CompareTo(v2.id));
                Type type = m_Views[0].RootElement.GetChild(0).GetType();
                lastKnownGoodRootElementType = type;
                for (int i = m_Views.Count - 1; i >= 0; i--) {
                    m_Views[i].Destroy();
                }
            }

            onReady = null;
            onUpdate = null;

            resourceManager.Reset();

            m_AfterUpdateTaskSystem.OnReset();
            m_BeforeUpdateTaskSystem.OnReset();

            throw new NotImplementedException("Need to re-implement refresh()");
            // CreateView("Default View", new Rect(0, 0, Width, Height), lastKnownGoodRootElementType);

            onRefresh?.Invoke();
            onNextRefresh?.Invoke();
            onNextRefresh = null;
            onReady?.Invoke();
        }

        public void Destroy() {
#if UNITY_EDITOR
            Applications.Remove(this);
#endif
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
            onElementEnabled = null;
            onElementDestroyed = null;
            onElementRegistered = null;
        }

        public static void DestroyElement(UIElement element) {
            element.View.application.DoDestroyElement(element);
        }

        internal void DoDestroyElement(UIElement element, bool removingChildren = false) {
            // do nothing if already destroyed
            if ((element.flags & UIElementFlags.Alive) == 0) {
                return;
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            LightList<UIElement> toInternalDestroy = LightList<UIElement>.Get();

            stack.Push(element);

            while (stack.Count > 0) {
                UIElement current = stack.array[--stack.size];

                if ((current.flags & UIElementFlags.Alive) == 0) {
                    continue;
                }

                current.flags &= ~(UIElementFlags.Alive);
                current.enableStateChangedFrameId = frameId;
                current.OnDestroy();
                toInternalDestroy.Add(current);

                UIElement[] children = current.children.array;
                int childCount = current.children.size;

                if (stack.size + childCount >= stack.array.Length) {
                    Array.Resize(ref stack.array, stack.size + childCount + 16);
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    // inline stack push
                    stack.array[stack.size++] = children[i];
                }
            }

            if (element.parent != null && !removingChildren) {
                element.parent.children.Remove(element);
                for (int i = 0; i < element.parent.children.Count; i++) {
                    element.parent.children[i].siblingIndex = i;
                }
            }

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementDestroyed(element);
            }

            for (int i = 0; i < toInternalDestroy.size; i++) {
                toInternalDestroy[i].InternalDestroy();
            }

            LightList<UIElement>.Release(ref toInternalDestroy);
            LightStack<UIElement>.Release(ref stack);

            onElementDestroyed?.Invoke(element);
        }

        internal void DestroyChildren(UIElement element) {
            if (element.isDestroyed) {
                return;
            }

            if (element.children == null || element.children.Count == 0) {
                return;
            }

            for (int i = 0; i < element.children.size; i++) {
                DoDestroyElement(element.children[i], true);
            }

            element.children.QuickClear();
        }

        // Triggered events fire immediately
        // Application Update future state
        //
        // Normal binding update for all elements
        // Input system update()
        // sync properties are written back to targets. these will invoke OnPropertySynchronized() if provided (just like OnPropertyChanged, but explicitly for `sync` write backs)
        // late update bindings & OnLateUpdate()
        // animation system update
        // style system update -> triggers OnStylePropertyChanged handlers
        // OnFrameCompleted()
        // user code finished here
        //
        // buffer changes from style system for render & layout thread to pick up (future state)
        //
        // render layout thread -> 
        // read buffered changes
        //     layout
        //     render
        //     join with user thread
        //     pause user thread
        // write layoutResult changes back to elements
        // maybe invoke render/layout callbacks if we support this
        // OnCulled()
        // OnLayoutChanged()
        // OnSizeChanged()
        // etc
        // continue user thread

        private LightList<UIElement> activeBuffer = new LightList<UIElement>(32);
        private LightList<UIElement> queuedBuffer = new LightList<UIElement>(32);

        public void Update() {
            // OnEnable()
            // get pending queue, enqueue
            // adding 1 element many times to the queue is fine
            // invoke enable callback immediately or deferred?

            // m_InputSystem.ReadInput();
            bool loop = true;
            bool firstRun = true;
            
            activeBuffer.Clear();

            for (int i = 0; i < m_Views.Count; i++) {
                activeBuffer.Add(m_Views[i].RootElement);
            }

            linqBindingSystem.BeginFrame();
            
            
            // enable element
            // it gets an update
            // it gets disabled
            // it gets enabled
            // now what? need to be lateUpdated?
            while (loop) {

                // bindings
                // OnBindingsUpdated()
                linqBindingSystem.BeforeUpdate(activeBuffer); // normal bindings + OnBeforeUpdate call 

                if (firstRun) {
                    m_InputSystem.OnUpdate();
                    firstRun = false;
                } 

                // late bindings?
                // onChange()
                // sync
                linqBindingSystem.AfterUpdate(activeBuffer); // on update call + write back 'sync' & onChange

                // m_AnimationSystem.OnUpdate(activeBuffer);

                // AfterUpdate()
                // linqBindingSystem.AfterUpdate(activeBuffer); // after update call

                if (queuedBuffer.size == 0) {
                    break;
                }

                LightList<UIElement> tmp = activeBuffer;
                activeBuffer = queuedBuffer;
                queuedBuffer = tmp;
                activeBuffer.Clear();
                // sort queued buffer by depth?
            }

            // bindingTimer.Reset();
            // bindingTimer.Start();
            // linqBindingSystem.OnUpdate();
            // bindingTimer.Stop();
            //
            // m_InputSystem.OnUpdate();
            //
            // linqBindingSystem.OnLateUpdate();
            //
            m_AnimationSystem.OnUpdate();
            //
            // m_RoutingSystem.OnUpdate(); // todo -- remove
            //
            // linqBindingSystem.OnFrameCompleted();

            m_StyleSystem.OnUpdate(); // buffer changes here

            // todo -- read changed data into layout/render thread
            layoutTimer.Reset();
            layoutTimer.Start();
            m_LayoutSystem.OnUpdate();
            layoutTimer.Stop();

            m_BeforeUpdateTaskSystem.OnUpdate();

            renderTimer.Reset();
            renderTimer.Start();
            m_RenderSystem.OnUpdate();
            renderTimer.Stop();

            m_AfterUpdateTaskSystem.OnUpdate();

            onUpdate?.Invoke();

            m_Views[0].SetSize(Screen.width, Screen.height);

            frameId++;
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
            element.View.application.DoEnableElement(element);
        }

        public static void DisableElement(UIElement element) {
            element.View.application.DoDisableElement(element);
        }

        public void DoEnableElement(UIElement element) {
            element.flags |= UIElementFlags.Enabled;

            // if element is not enabled (ie has a disabled ancestor or is not alive), no-op 
            if ((element.flags & UIElementFlags.SelfAndAncestorEnabled) != UIElementFlags.SelfAndAncestorEnabled) {
                return;
            }

            queuedBuffer.Add(element);
            
            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            stack.array[stack.size++].element = element;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                child.flags |= UIElementFlags.AncestorEnabled;

                // if the element is itself disabled or destroyed, keep going
                if ((child.flags & UIElementFlags.Enabled) == 0) {
                    continue;
                }

                // todo -- profile not calling enable when it's not needed
                // if (child.flags & UIElementFlags.RequiresEnableCall) {
                child.style.UpdateInheritedStyles();
                child.OnEnable();
                // }

                // We need to run all runCommands now otherwise animations in [normal] style groups won't run after enabling.
                child.style.RunCommands();

                if ((child.flags & UIElementFlags.HasBeenEnabled) == 0) {
                    child.View.ElementCreated(child);
                    // run once bindings if present
                }

                // register the flag set even if we get disabled via OnEnable, we just want to track that OnEnable was called at least once
                child.flags |= UIElementFlags.HasBeenEnabled;

                // only continue if calling enable didn't re-disable the element
                if ((child.flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled) {
                    child.enableStateChangedFrameId = frameId;
                    UIElement[] children = child.children.array;
                    int childCount = child.children.size;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 16);
                    }

                    for (int i = childCount - 1; i >= 0; i--) {
                        // inline stack push
                        stack.array[stack.size++].element = children[i];
                    }
                }
            }

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementEnabled(element);
            }

            StructStack<ElemRef>.Release(ref stack);

            onElementEnabled?.Invoke(element);
        }

        // todo bad things happen if we add children during disabling or enabling (probably)
        public void DoDisableElement(UIElement element) {
            // if element is already disabled or destroyed, no op
            if ((element.flags & UIElementFlags.Alive) == 0) {
                return;
            }

            bool wasDisabled = element.isDisabled;
            element.flags &= ~(UIElementFlags.Enabled);

            if (wasDisabled) {
                return;
            }

            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            stack.array[stack.size++].element = element;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                child.flags &= ~(UIElementFlags.AncestorEnabled);

                // if destroyed the whole subtree is also destroyed, do nothing.
                // if already disabled the whole subtree is also disabled, do nothing.

                if ((child.flags & (UIElementFlags.Alive | UIElementFlags.Enabled)) == 0) {
                    continue;
                }

                // todo -- profile not calling disable when it's not needed
                // if (child.flags & UIElementFlags.RequiresEnableCall) {
                child.OnDisable();
                // }

                // todo -- maybe do this on enable instead
                if (child.style.currentState != StyleState.Normal) {
                    // todo -- maybe just have a clear states method
                    child.style.ExitState(StyleState.Hover);
                    child.style.ExitState(StyleState.Active);
                    child.style.ExitState(StyleState.Focused);
                }

                child.enableStateChangedFrameId = frameId;

                // if child is still disabled after OnDisable, traverse it's children
                if (!child.isEnabled) {
                    UIElement[] children = child.children.array;
                    int childCount = child.children.size;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 16);
                    }

                    for (int i = childCount - 1; i >= 0; i--) {
                        // inline stack push
                        stack.array[stack.size++].element = children[i];
                    }
                }
            }

            // avoid checking in the loop if this is the originally disabled element
            if (element.parent.isEnabled) {
                element.flags |= UIElementFlags.AncestorEnabled;
            }

            StructStack<ElemRef>.Release(ref stack);

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnElementDisabled(element);
            }
        }

        public UIElement GetElement(int elementId) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            for (int i = 0; i < m_Views.Count; i++) {
                stack.Push(m_Views[i].RootElement);

                while (stack.size > 0) {
                    UIElement element = stack.PopUnchecked();

                    if (element.id == elementId) {
                        LightStack<UIElement>.Release(ref stack);
                        return element;
                    }

                    if (element.children == null) continue;

                    for (int j = 0; j < element.children.size; j++) {
                        stack.Push(element.children.array[j]);
                    }
                }
            }

            LightStack<UIElement>.Release(ref stack);
            return null;
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

        public UIView GetView(string name) {
            for (int i = 0; i < m_Views.Count; i++) {
                UIView v = m_Views[i];
                if (v.name == name) {
                    return v;
                }
            }

            return null;
        }

        public static Application Find(string appId) {
            return Applications.Find((app) => app.id == appId);
        }

        public static bool HasCustomPainter(string name) {
            return s_CustomPainters.ContainsKey(name);
        }

        public AnimationTask Animate(UIElement element, AnimationData animation) {
            return m_AnimationSystem.Animate(element, ref animation);
        }

        public void PauseAnimation(UIElement element, AnimationData animationData) {
            m_AnimationSystem.PauseAnimation(element, ref animationData);
        }

        public void ResumeAnimation(UIElement element, AnimationData animationData) {
            m_AnimationSystem.ResumeAnimation(element, ref animationData);
        }

        public void StopAnimation(UIElement element, AnimationData animationData) {
            m_AnimationSystem.StopAnimation(element, ref animationData);
        }

        public UIView[] GetViews() {
            return m_Views.ToArray();
        }

        public AnimationData GetAnimationFromFile(string fileName, string animationName) {
            throw new NotImplementedException("Re design this not to use style importer");
        }

        internal void InitializeElement(UIElement child) {
            bool parentEnabled = child.parent.isEnabled;

            UIView view = child.parent.View;

            StructStack<ElemRef> elemRefStack = StructStack<ElemRef>.Get();
            elemRefStack.Push(new ElemRef() {element = child});

            while (elemRefStack.size > 0) {
                UIElement current = elemRefStack.array[--elemRefStack.size].element;

                current.depth = current.parent.depth + 1;

                current.View = view;

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

                if ((current.flags & UIElementFlags.Created) == 0) {
                    current.flags |= UIElementFlags.Created;
//                    current.style.Initialize();
                    for (int i = 0; i < m_Systems.Count; i++) {
                        m_Systems[i].OnElementCreated(current);
                    }

                    onElementRegistered?.Invoke(current);
                    current.OnCreate();
                }

                UIElement[] children = current.children.array;
                int childCount = current.children.size;
                // reverse this?
                for (int i = 0; i < childCount; i++) {
                    children[i].siblingIndex = i;
                    elemRefStack.Push(new ElemRef() {element = children[i]});
                }
            }

            if (parentEnabled && child.isEnabled) {
                child.enableStateChangedFrameId = frameId;
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child);
            }

            StructStack<ElemRef>.Release(ref elemRefStack);
        }

        internal void InsertChild(UIElement parent, UIElement child, uint index) {
            child.parent = parent;
            parent.children.Insert((int) index, child);

            bool parentEnabled = parent.isEnabled;

            UIView view = parent.View;

            StructStack<ElemRef> elemRefStack = StructStack<ElemRef>.Get();
            elemRefStack.Push(new ElemRef() {element = child});

            while (elemRefStack.Count > 0) {
                UIElement current = elemRefStack.Pop().element;

                current.depth = current.parent.depth + 1;

                current.View = view;

                if (current.parent.isEnabled) {
                    current.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    current.flags &= ~UIElementFlags.AncestorEnabled;
                }

                if ((current.flags & UIElementFlags.Created) == 0) {
                    current.flags |= UIElementFlags.Created;
//                    current.style.Initialize();
                    for (int i = 0; i < m_Systems.Count; i++) {
                        m_Systems[i].OnElementCreated(current);
                    }

                    onElementRegistered?.Invoke(current);
                    current.OnCreate();
                }

                UIElement[] children = current.children.array;
                int childCount = current.children.size;
                // reverse this?
                for (int i = 0; i < childCount; i++) {
                    children[i].siblingIndex = i;
                    elemRefStack.Push(new ElemRef() {element = children[i]});
                }
            }

            for (int i = 0; i < parent.children.size; i++) {
                parent.children.array[i].siblingIndex = i;
            }

            if (parentEnabled && child.isEnabled) {
                child.enableStateChangedFrameId = frameId;
                child.flags &= ~UIElementFlags.Enabled;
                DoEnableElement(child);
            }

            StructStack<ElemRef>.Release(ref elemRefStack);
        }

        public void SortViews() {
            // let's bubble sort the views since only once view is out of place
            for (int i = (m_Views.Count - 1); i > 0; i--) {
                for (int j = 1; j <= i; j++) {
                    if (m_Views[j - 1].Depth > m_Views[j].Depth) {
                        UIView tempView = m_Views[j - 1];
                        m_Views[j - 1] = m_Views[j];
                        m_Views[j] = tempView;
                    }
                }
            }

            onViewsSorted?.Invoke(m_Views.ToArray());
        }

        internal void GetElementCount(out int totalElementCount, out int enabledElementCount, out int disabledElementCount) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            totalElementCount = 0;
            enabledElementCount = 0;

            for (int i = 0; i < m_Views.Count; i++) {
                stack.Push(m_Views[i].RootElement);

                while (stack.size > 0) {
                    totalElementCount++;
                    UIElement element = stack.PopUnchecked();

                    if (element.isEnabled) {
                        enabledElementCount++;
                    }

                    if (element.children == null) continue;

                    for (int j = 0; j < element.children.size; j++) {
                        stack.Push(element.children.array[j]);
                    }
                }
            }

            disabledElementCount = totalElementCount - enabledElementCount;
            LightStack<UIElement>.Release(ref stack);
        }

        public UIElement CreateSlot2(string slotName, TemplateScope scope, int defaultSlotId, UIElement root, UIElement parent) {
            int slotId = ResolveSlotId(slotName, scope.slotInputs, defaultSlotId, out UIElement contextRoot);
            if (contextRoot == null) {
                Assert.AreEqual(slotId, defaultSlotId);
                contextRoot = root;
            }

            scope.innerSlotContext = root;
            UIElement retn = templateData.slots[slotId](contextRoot, parent, scope);
            retn.View = parent.View;
            return retn;
        }

        public UIElement CreateTemplate(int templateSpawnId, UIElement contextRoot, UIElement parent, TemplateScope scope) {
            UIElement retn = templateData.slots[templateSpawnId](contextRoot, parent, scope);
            retn.View = parent.View;
            return retn;
        }

        // todo -- override that accepts an index into an array instead of a type, to save a dictionary lookup
        // todo -- don't create a list for every type, maybe a single pool list w/ sorting & a jump search or similar
        // todo -- register element in type map for selectors, might need to support subclass matching ie <KlangButton> and <OtherButton> with matching on <Button>
        // todo -- make children a linked list instead
        /// Returns the shell of a UI Element, space is allocated for children but no child data is associated yet, only a parent, view, and depth
        public UIElement CreateElementFromPool(int typeId, UIElement parent, int childCount, int attributeCount, int originTemplateId) {
            // children get assigned in the template function but we need to setup the list here
            UIElement retn = templateData.ConstructElement(typeId);
            retn.application = this;

            //retn.View = application.activeView;

            retn.templateMetaData = templateData.templateMetaData[originTemplateId];
            retn.id = NextElementId;
            retn.style = new UIStyleSet(retn);
            retn.layoutResult = new LayoutResult(retn);
            retn.flags = UIElementFlags.Enabled | UIElementFlags.Alive;

            retn.children = LightList<UIElement>.Get();
            retn.children.EnsureCapacity(childCount);
            retn.children.size = childCount;

            if (attributeCount > 0) {
                retn.attributes = new StructList<ElementAttribute>(attributeCount);
                retn.attributes.size = attributeCount;
            }

            retn.parent = parent;

            return retn;
        }

        public UIElement CreateElementFromPoolWithType(int typeId, UIElement parent, int childCount, int attrCount, int originTemplateId) {
            return CreateElementFromPool(typeId, parent, childCount, attrCount, originTemplateId);
        }

        public static int ResolveSlotId(string slotName, StructList<SlotUsage> slotList, int defaultId, out UIElement contextRoot) {
            if (slotList == null) {
                contextRoot = null;
                return defaultId;
            }

            for (int i = 0; i < slotList.size; i++) {
                if (slotList.array[i].slotName == slotName) {
                    contextRoot = slotList.array[i].outerContext;
                    return slotList.array[i].slotId;
                }
            }

            contextRoot = null;
            return defaultId;
        }

        // Doesn't expect to create the root
        public void HydrateTemplate(int templateId, UIElement root, TemplateScope scope) {
            templateData.templates[templateId](root, scope);
        }

        public void AddTemplateChildren(SlotTemplateElement slotTemplateElement, int templateId, int count) {
            throw new Exception("Verify this");
            if (templateId < 0) return;

            TemplateScope scope = new TemplateScope(this, null);

            for (int i = 0; i < count; i++) {
                UIElement root = slotTemplateElement.bindingNode.root;
                UIElement child = templateData.slots[templateId](root, root, scope);
                InsertChild(slotTemplateElement, child, (uint) slotTemplateElement.children.Count);
            }
        }

    }

}