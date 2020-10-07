using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UIForia.Animation;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace UIForia {

    internal struct ApplicationSetup {

        public Dictionary<Type, int> typeTemplateMap;
        public LightList<TemplateData> templateData;

        public Type rootType;
        // public StyleDatabase styleDatabase;

    }

    public abstract class Application : IDisposable {

        private static SizeInt UIApplicationSize;

        public bool IsValid { get; internal set; }

        public static float dpiScaleFactor = Mathf.Max(1, Screen.dpi / 100f);

        public static readonly float originalDpiScaleFactor = Mathf.Max(1, Screen.dpi / 100f);

        public float DPIScaleFactor {
            get => dpiScaleFactor;
            set => dpiScaleFactor = value;
        }

        public static SizeInt UiApplicationSize => UIApplicationSize;

        public static List<Application> Applications = new List<Application>();

        internal Stopwatch layoutTimer = new Stopwatch();
        internal Stopwatch renderTimer = new Stopwatch();
        internal Stopwatch bindingTimer = new Stopwatch();
        internal Stopwatch loopTimer = new Stopwatch();

        public readonly string id;
        internal StyleSystem styleSystem;
        internal LayoutSystem layoutSystem;
        internal RenderSystem2 renderSystem;
        internal InputSystem inputSystem;
        internal RoutingSystem routingSystem;
        internal AnimationSystem animationSystem;
        internal UISoundSystem soundSystem;
        internal LinqBindingSystem linqBindingSystem;
        internal ElementSystem elementSystem;
        internal TemplateSystem templateSystem;

        protected ResourceManager resourceManager;

        protected List<ISystem> systems;

        internal DataList<ElementId>.Shared viewRootIds;

        public event Action<UIElement> onElementRegistered;
        public event Action onElementDestroyed;
        public event Action<UIElement> onElementEnabled;
        public event Action<UIView[]> onViewsSorted;
        public event Action<UIView> onViewRemoved;
        public event Action onRefresh;

        internal CompiledTemplateData templateData;

        internal int frameId;
        protected internal List<UIView> views;

        internal static Dictionary<string, Type> s_CustomPainters;

        private UITaskSystem m_BeforeUpdateTaskSystem;
        private UITaskSystem m_AfterUpdateTaskSystem;

        public static readonly UIForiaSettings Settings;

        static Application() {
            ArrayPool<UIElement>.SetMaxPoolSize(64);
            s_CustomPainters = new Dictionary<string, Type>();
            Settings = Resources.Load<UIForiaSettings>("UIForiaSettings");
            if (Settings == null) {
                throw new Exception("UIForiaSettings are missing. Use the UIForia/Create UIForia Settings to create it");
            }
        }

        protected TemplateSettings templateSettings;

        protected Application(string applicationName) {
            this.id = applicationName;
            this.resourceManager = new ResourceManager();
            this.viewRootIds = new DataList<ElementId>.Shared(8, Allocator.Persistent);
        }

        protected Application(bool isPreCompiled, TemplateSettings templateSettings, ResourceManager resourceManager, Action<UIElement> onElementRegistered) {
            this.templateSettings = templateSettings;
            this.onElementRegistered = onElementRegistered;
            this.id = templateSettings.applicationName;
            this.resourceManager = resourceManager ?? new ResourceManager();
            this.viewRootIds = new DataList<ElementId>.Shared(8, Allocator.Persistent);

            Applications.Add(this);
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnEditorReload;
#endif
        }

#if UNITY_EDITOR
        private void OnEditorReload() {
            templateData?.Destroy();
            templateData = null;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnEditorReload;
        }
#endif

        protected virtual void CreateSystems() {
            elementSystem = new ElementSystem(InitialElementCapacity);
            styleSystem = new StyleSystem(elementSystem);
            textSystem = new TextSystem(this, elementSystem, null); // todo -- register text effects //templateSettings.textEffectDefs);
            routingSystem = new RoutingSystem();
            linqBindingSystem = new LinqBindingSystem();
            soundSystem = new UISoundSystem();
            animationSystem = new AnimationSystem(elementSystem);
            layoutSystem = new LayoutSystem(this, elementSystem, textSystem);
            renderSystem = new RenderSystem2(this, layoutSystem, elementSystem);
            inputSystem = new GameInputSystem(layoutSystem, new KeyboardInputManager());
            templateSystem = new TemplateSystem(this);
        }


        internal void Initialize(ApplicationSetup applicationSetup) {
            systems = new List<ISystem>();
            views = new List<UIView>();

            frameId = 1;

            CreateSystems();


            textSystem.frameId = frameId;

            systems.Add(routingSystem);
            systems.Add(inputSystem);
            systems.Add(animationSystem);

            m_BeforeUpdateTaskSystem = new UITaskSystem();
            m_AfterUpdateTaskSystem = new UITaskSystem();

            // if (isPreCompiled) {
            //     templateData = TemplateLoader.LoadPrecompiledTemplates(templateSettings);
            // }
            // else {
            //     templateData = TemplateLoader.LoadRuntimeTemplates(templateSettings.rootType, templateSettings);
            // }
            
            templateSystem.Initialize(applicationSetup);

            viewRootIds.size = 0;

            UIView view = new UIView(views.Count, this, "Default", Matrix4x4.identity, new Size(Width, Height));

            viewRootIds.Add(view.dummyRoot.id);

            templateSystem.CreateEntryPoint(view.dummyRoot, applicationSetup.rootType);
            return;
            UIElement rootElement = templateData.templates[0].Invoke(null, new TemplateScope(this));
            view.Init(rootElement);

            views.Add(view);

            layoutSystem.OnViewAdded(view);
            renderSystem.OnViewAdded(view);
        }

        public UIView CreateView<T>(string name, Size size, in Matrix4x4 matrix) where T : UIElement {
            if (templateData.TryGetTemplate<T>(out DynamicTemplate dynamicTemplate)) {
                UIElement element = templateData.templates[dynamicTemplate.templateId].Invoke(null, new TemplateScope(this));

                UIView view = new UIView(views.Count, this, name, element, matrix, size);

                viewRootIds.Add(view.dummyRoot.id);

                view.Depth = views.Count;
                views.Add(view);

                layoutSystem.OnViewAdded(view);
                renderSystem.OnViewAdded(view);
                return view;
            }

            throw new TemplateNotFoundException($"Unable to find a template for {typeof(T)}. This is probably because you are trying to load this template dynamically and did include the type in the {nameof(TemplateSettings.dynamicallyCreatedTypes)} list.");
        }

        public UIView CreateView<T>(string name, Size size) where T : UIElement {
            return CreateView<T>(name, size, Matrix4x4.identity);
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
                        throw new Exception($"Failed to register a custom painter with the name {paintAttr.name} from type {type.FullName} because it was already registered.");
                    }

                    s_CustomPainters.Add(paintAttr.name, type);
                }
            }
        }

        public StyleSystem StyleSystem => styleSystem;
        public IRenderSystem RenderSystem => renderSystem;
        public LayoutSystem LayoutSystem => layoutSystem;
        public InputSystem InputSystem => inputSystem;
        public RoutingSystem RoutingSystem => routingSystem;
        public UISoundSystem SoundSystem => soundSystem;

        public Camera Camera { get; private set; }

        public ResourceManager ResourceManager => resourceManager;

        public void SetScreenSize(int width, int height) {
            UIApplicationSize.width = width;
            UIApplicationSize.height = height;
        }

        public float Width => UiApplicationSize.width / dpiScaleFactor;

        public float Height => UiApplicationSize.height / dpiScaleFactor;

        private int initialElementCapacity;
        public TextSystem textSystem;

        public int InitialElementCapacity {
            get => initialElementCapacity < 32 ? 32 : initialElementCapacity;
            set => initialElementCapacity = value < 32 ? 32 : value;
        }

        public void SetCamera(Camera camera) {
            Rect rect = camera.pixelRect;
            UIApplicationSize.height = (int) rect.height;
            UIApplicationSize.width = (int) rect.width;

            if (views.Count > 0) {
                views[0].Viewport = new Rect(0, 0, Width, Height);
            }

            Camera = camera;
            RenderSystem.SetCamera(camera);
        }

        public UIView RemoveView(UIView view) {
            if (!views.Remove(view)) return null;

            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnViewRemoved(view);
            }

            DestroyElement(view.dummyRoot);
            onViewRemoved?.Invoke(view);
            return view;
        }


        internal void Reset() {
            for (int i = views.Count - 1; i >= 0; i--) {
                views[i].Destroy();
            }

            foreach (ISystem system in systems) {
                system.OnDestroy();
            }

            elementSystem.Dispose();
            layoutSystem.Dispose();
            textSystem.Dispose();
            renderSystem.Dispose();
            templateData.Destroy();

            resourceManager.Reset();

            m_AfterUpdateTaskSystem.OnDestroy();
            m_BeforeUpdateTaskSystem.OnDestroy();
        }

        internal void Refresh() {
            // if (isPreCompiled) {
            //     Debug.Log("Cannot refresh application because it is using precompiled templates");
            //     return;
            // }

            throw new NotImplementedException("reset not implemented");

            Stopwatch stopwatch = Stopwatch.StartNew();

            Reset();


            // Initialize();

            onRefresh?.Invoke();

            stopwatch.Stop();
            Debug.Log("Refreshed " + id + " in " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " seconds");
        }

        internal void Destroy() {
            if (!IsValid) {
                return;
            }

            Applications.Remove(this);
            templateData?.Destroy();

            foreach (ISystem system in systems) {
                system.OnDestroy();
            }

            for (int i = views.Count - 1; i >= 0; i--) {
                views[i].Destroy();
            }

            onElementEnabled = null;
            onElementDestroyed = null;
            onElementRegistered = null;
        }

        public static void DestroyElement(UIElement element) {
            element.View.application.DoDestroyElement(element);
        }

        internal unsafe void DoDestroyElement(UIElement element) {
            // do nothing if already destroyed

            ElementTable<ElementMetaInfo> metaTable = elementSystem.metaTable;

            if (!elementSystem.IsAlive(element.id)) {
                return;
            }

            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            UIView view = element.View;

            stack.Push(element);

            ref ElementMetaInfo m = ref metaTable.array[element.id.index];
            m.flags |= UIElementFlags.DisableRoot;

            while (stack.size > 0) {
                UIElement current = stack.array[--stack.size];

                ref ElementMetaInfo metaInfo = ref metaTable.array[current.id.index];

                // if already dead, continue
                if (metaInfo.generation != current.id.generation) {
                    continue;
                }

                current.isAlive = false;

                view.activeElementCount--;
                elementSystem.disabledElementsThisFrame.Add(current.id);
                elementSystem.DestroyElement(current.id, current.id == element.id);

                try {
                    current.OnDestroy();
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }

                int childCount = current.ChildCount;

                if (stack.size + childCount >= stack.array.Length) {
                    Array.Resize(ref stack.array, stack.size + childCount + 32);
                }

                UIElement ptr = current.GetLastChild();
                while (ptr != null) {
                    // inline stack push
                    stack.array[stack.size++] = ptr;
                    ptr = ptr.GetPreviousSibling();
                }
            }

            inputSystem.OnElementDestroyed(element);

            LightStack<UIElement>.Release(ref stack);

            try {
                onElementDestroyed?.Invoke();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public void Update() {
            if (!IsValid) return;
            frameId++;

            textSystem.frameId = frameId;

            // input queries against last frame layout
            inputSystem.Read();
            loopTimer.Reset();
            loopTimer.Start();

            Rect rect;
            if (!ReferenceEquals(Camera, null)) {
                rect = Camera.pixelRect;
            }
            else {
                rect = new Rect(0, 0, 1920, 1080);
            }

            UIApplicationSize.height = (int) rect.height;
            UIApplicationSize.width = (int) rect.width;

            viewRootIds.size = 0;
            for (int i = 0; i < views.Count; i++) {
                // todo -- this is totally wrong! needs proper coords and size shouldn't always bee app size
                views[i].Viewport = new Rect(0, 0, UIApplicationSize.width, UIApplicationSize.height);
                viewRootIds.Add(views[i].dummyRoot.id);
            }

            Profiler.BeginSample("UIForia::InputUpdate");
            inputSystem.OnUpdate();
            Profiler.EndSample();

            m_BeforeUpdateTaskSystem.OnUpdate();

            bindingTimer.Reset();
            bindingTimer.Start();
            Profiler.BeginSample("UIForia::Bindings");
            // right now, out of order elements wont get bindings until next frame. this miiight be ok but probably will cause weirdness. likely want this to change
            for (int i = 0; i < views.Count; i++) {
                linqBindingSystem.NewUpdateFn(views[i].RootElement);
            }

            bindingTimer.Stop();
            Profiler.EndSample();
            routingSystem.OnUpdate();

            Profiler.BeginSample("UIForia::HandleEnableStateChanges");
            if (elementSystem.enabledElementsThisFrame.size > 0 || elementSystem.disabledElementsThisFrame.size > 0) {
                new FilterEnabledDisabledElementsJob() {
                    metaTable = elementSystem.metaTable,
                    enabledElements = elementSystem.enabledElementsThisFrame,
                    disabledElements = elementSystem.disabledElementsThisFrame
                }.Run();
            }

            new UpdateTraversalTable() {
                hierarchyTable = elementSystem.hierarchyTable,
                metaTable = elementSystem.metaTable,
                traversalTable = elementSystem.traversalTable,
                rootIds = viewRootIds,
            }.Run();

            Profiler.EndSample();
            Profiler.BeginSample("UIForia::AnimationUpdate");
            animationSystem.OnUpdate();
            Profiler.EndSample();

            Profiler.BeginSample("UIForia::FlushStyleChanges");
            styleSystem.FlushChangeSets(elementSystem, layoutSystem, renderSystem);
            Profiler.EndSample();

            if (elementSystem.disabledElementsThisFrame.size > 0) {
                layoutSystem.HandleElementDisabled(elementSystem.disabledElementsThisFrame);
                textSystem.HandleElementDisabled(elementSystem.disabledElementsThisFrame);
                renderSystem.HandleElementsDisabled(elementSystem.disabledElementsThisFrame);
            }

            if (elementSystem.enabledElementsThisFrame.size > 0) {
                layoutSystem.HandleElementEnabled(elementSystem.enabledElementsThisFrame);
                textSystem.HandleElementEnabled(elementSystem.enabledElementsThisFrame);
                renderSystem.HandleElementsEnabled(elementSystem.enabledElementsThisFrame);
            }

            layoutTimer.Restart();
            Profiler.BeginSample("UIForia::Layout");
            layoutSystem.RunLayout();
            Profiler.EndSample();
            layoutTimer.Stop();

            Profiler.BeginSample("UIForia::TextAnimation");
            textSystem.AnimateText();
            Profiler.EndSample();

            renderTimer.Restart();
            Profiler.BeginSample("UIForia::Rendering");
            renderSystem.OnUpdate();
            Profiler.EndSample();
            renderTimer.Stop();

            m_AfterUpdateTaskSystem.OnUpdate();

            elementSystem.CleanupFrame();
            textSystem.CleanupFrame();

            frameId++;

            loopTimer.Stop();
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

        internal unsafe void DoEnableElement(UIElement element) {
            // if element is not enabled (ie has a disabled ancestor or is not alive), no-op 
            if (element.isDestroyed) {
                return;
            }

            if ((element.flags & UIElementFlags.HasBeenEnabled) != 0) {
                if (element.isEnabled) {
                    return;
                }
            }

            // if has been enabled & isEnabled return 

            ElementTable<ElementMetaInfo> metaTable = elementSystem.metaTable;

            if (!element.isAncestorEnabled) {
                element.isSelfEnabled = true;
                metaTable[element.id].flags |= UIElementFlags.Enabled;
                return;
            }

            UIView view = element.View;

            int viewId = view.id;

            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            stack.array[stack.size++].element = element;

            element.isSelfEnabled = true;

            metaTable[element.id].flags |= UIElementFlags.HasBeenEnabled | UIElementFlags.Enabled | UIElementFlags.EnabledRoot;
            element.flags |= UIElementFlags.HasBeenEnabled | UIElementFlags.Enabled | UIElementFlags.EnabledRoot;

            UIElement[] instanceTable = elementSystem.instanceTable;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                ref ElementMetaInfo metaInfo = ref metaTable.array[child.id.index];

                metaInfo.flags |= UIElementFlags.AncestorEnabled;
                child.isAncestorEnabled = true;

                // if the element is itself disabled or destroyed, keep going
                if ((metaInfo.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                elementSystem.enabledElementsThisFrame.Add(child.id);
                hierarchyTable.array[element.id.index].viewId = viewId;

                child.style.UpdateInheritedStyles(); // todo -- move this
                view.activeElementCount++;
                metaInfo.flags |= UIElementFlags.HasBeenEnabled;
                element.flags |= UIElementFlags.HasBeenEnabled;
                try {
                    child.OnEnable();
                }
                catch (Exception e) {
                    Debug.Log(e);
                }

                // todo -- move this
                // We need to run all runCommands now otherwise animations in [normal] style groups won't run after enabling.
                child.style.RunCommands(); // goes away with style system redesign

                // register the flag set even if we get disabled via OnEnable, we just want to track that OnEnable was called at least once

                // only continue if calling enable didn't re-disable the element
                if ((metaInfo.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                    int childCount = elementSystem.hierarchyTable[child.id].childCount;
                    child.enableStateChangedFrameId = frameId;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 32);
                    }

                    UIElement ptr = instanceTable[hierarchyTable.array[child.id.index].lastChildId.index];

                    while (ptr != null) {
                        // inline stack push
                        stack.array[stack.size++].element = ptr;
                        ptr = instanceTable[hierarchyTable.array[ptr.id.index].prevSiblingId.index];
                    }
                }
            }

            StructStack<ElemRef>.Release(ref stack);

            if (onElementEnabled != null) {
                try {
                    onElementEnabled.Invoke(element);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        public unsafe void DoDisableElement(UIElement element) {
            // if element is already disabled or destroyed, no op

            if (element.isDestroyed || element.isSelfDisabled) {
                return;
            }

            if (!element.isAncestorEnabled) {
                elementSystem.metaTable[element.id].flags &= ~UIElementFlags.Enabled;
                element.isSelfEnabled = false;
                return;
            }

            UIView view = element.View;

            ref ElementTable<ElementMetaInfo> metaTable = ref elementSystem.metaTable;

            // if element is now enabled we need to walk it's children
            // and set enabled ancestor flags until we find a self-disabled child
            StructStack<ElemRef> stack = StructStack<ElemRef>.Get();
            stack.array[stack.size++].element = element;

            UIElement[] instanceTable = elementSystem.instanceTable;
            ElementTable<HierarchyInfo> hierarchyTable = elementSystem.hierarchyTable;

            // stack operations in the following code are inlined since this is a very hot path
            while (stack.size > 0) {
                // inline stack pop
                UIElement child = stack.array[--stack.size].element;

                ref ElementMetaInfo metaInfo = ref metaTable.array[child.id.index];

                child.isAncestorEnabled = false;
                metaInfo.flags &= ~(UIElementFlags.AncestorEnabled);

                // if destroyed the whole subtree is also destroyed, do nothing.
                // if already disabled the whole subtree is also disabled, do nothing.

                if ((metaInfo.flags & UIElementFlags.Enabled) == 0) {
                    continue;
                }

                elementSystem.disabledElementsThisFrame.Add(child.id);
                if ((child.flags & UIElementFlags.HasBeenEnabled) != 0) {
                    view.activeElementCount--;
                    // todo -- profile not calling disable when it's not needed
                    // if (child.flags & UIElementFlags.RequiresEnableCall) {
                    try {
                        child.OnDisable();
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                    }
                }

                // todo -- maybe do this on enable instead
                if (child.style.currentState != StyleState.Normal) {
                    child.style.ExitState(StyleState.Hover | StyleState.Active | StyleState.Focused);
                }

                // if child is still disabled after OnDisable, traverse it's children
                if (child.isDisabled) {
                    int childCount = elementSystem.hierarchyTable.array[child.id.index].childCount;
                    child.enableStateChangedFrameId = frameId;
                    if (stack.size + childCount >= stack.array.Length) {
                        Array.Resize(ref stack.array, stack.size + childCount + 32);
                    }

                    UIElement ptr = instanceTable[hierarchyTable.array[child.id.index].lastChildId.index];

                    while (ptr != null) {
                        // inline stack push
                        stack.array[stack.size++].element = ptr;
                        ptr = instanceTable[hierarchyTable.array[ptr.id.index].prevSiblingId.index];
                    }
                }
            }

            // was disabled in loop, need to reset it here
            // set this after the loop so we dont have special cases inside it.
            element.isAncestorEnabled = true;
            element.isSelfEnabled = false;
            metaTable.array[element.id.index].flags &= ~UIElementFlags.Enabled;
            metaTable.array[element.id.index].flags |= UIElementFlags.DisableRoot;

            StructStack<ElemRef>.Release(ref stack);

            inputSystem.BlurOnDisableOrDestroy();
        }

        public UIElement GetElement(ElementId elementId) {
            return elementSystem.instanceTable[elementId.index];
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
            styleSystem.OnAttributeSet(element, attributeName, currentValue, previousValue);
            for (int i = 0; i < systems.Count; i++) {
                systems[i].OnAttributeSet(element, attributeName, currentValue, previousValue);
            }
        }

        public static void RefreshAll() {
            for (int i = 0; i < Applications.Count; i++) {
                Applications[i].Refresh();
            }
        }

        public UIView GetView(int i) {
            if (i < 0 || i >= views.Count) return null;
            return views[i];
        }

        public UIView GetView(string name) {
            for (int i = 0; i < views.Count; i++) {
                UIView v = views[i];
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

        public UIView[] GetViews() {
            return views.ToArray();
        }

        internal void InitializeElement(UIElement child) {
            bool parentEnabled = child.parent.isEnabled;

            UIView view = child.parent.View;

            StructStack<ElemRef> elemRefStack = StructStack<ElemRef>.Get();
            elemRefStack.Push(new ElemRef() {element = child});

            ElementTable<ElementMetaInfo> metaTable = elementSystem.metaTable;

            while (elemRefStack.size > 0) {
                UIElement current = elemRefStack.array[--elemRefStack.size].element;

                current.hierarchyDepth = current.parent.hierarchyDepth + 1;

                current.View = view;

                ref ElementMetaInfo metaInfo = ref metaTable[current.id];

                // metaInfo.flags &= ~UIElementFlags.Enabled;
                // current.flags &= ~UIElementFlags.Enabled;

                current.isAncestorEnabled = current.parent.isEnabled;

                if (current.parent.isEnabled) {
                    metaInfo.flags |= UIElementFlags.AncestorEnabled;
                }
                else {
                    metaInfo.flags &= ~UIElementFlags.AncestorEnabled;
                }

                // always true, oder?
                if ((metaInfo.flags & UIElementFlags.Created) == 0) {
                    metaInfo.flags |= UIElementFlags.Created;
                    current.flags |= UIElementFlags.Created;
                    routingSystem.OnElementCreated(current);

                    try {
                        onElementRegistered?.Invoke(current);
                        current.OnCreate(); // might have been disabled here!
                    }
                    catch (Exception e) {
                        Debug.LogWarning(e);
                    }
                }

                UIElement ptr = current.GetFirstChild();
                int idx = 0;
                while (ptr != null) {
                    ptr.siblingIndex = idx++;
                    elemRefStack.Push(new ElemRef() {element = ptr});
                    ptr = ptr.GetNextSibling();
                }
            }

            if (parentEnabled && child.isEnabled) {
                DoEnableElement(child);
            }

            StructStack<ElemRef>.Release(ref elemRefStack);
        }

        public void SortViews() {
            // let's bubble sort the views since only once view is out of place
            for (int i = (views.Count - 1); i > 0; i--) {
                for (int j = 1; j <= i; j++) {
                    if (views[j - 1].Depth > views[j].Depth) {
                        UIView tempView = views[j - 1];
                        views[j - 1] = views[j];
                        views[j] = tempView;
                    }
                }
            }

            onViewsSorted?.Invoke(views.ToArray());
        }

        internal void GetElementCount(out int totalElementCount, out int enabledElementCount, out int disabledElementCount) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            totalElementCount = 0;
            enabledElementCount = 0;

            for (int i = 0; i < views.Count; i++) {
                stack.Push(views[i].RootElement);

                while (stack.size > 0) {
                    totalElementCount++;
                    UIElement element = stack.PopUnchecked();

                    if (element.isEnabled) {
                        enabledElementCount++;
                    }

                    UIElement ptr = element.GetFirstChild();

                    while (ptr != null) {
                        stack.Push(ptr);
                        ptr = ptr.GetNextSibling();
                    }
                }
            }

            disabledElementCount = totalElementCount - enabledElementCount;
            LightStack<UIElement>.Release(ref stack);
        }

        public UIElement CreateSlot(string slotName, TemplateScope scope, int defaultSlotId, UIElement root, UIElement parent) {
            int slotId = ResolveSlotId(slotName, scope.slotInputs, defaultSlotId, out UIElement contextRoot);
            if (contextRoot == null) {
                Assert.AreEqual(slotId, defaultSlotId);
                contextRoot = root;
            }

            // context 0 = innermost
            // context[context.size - 1] = outermost

            scope.innerSlotContext = root;
            // for each override with same name add to reference array at index?
            // will have to be careful with names but can change to unique ids when we need alias support and match on that
            UIElement retn = templateData.slots[slotId](contextRoot, parent, scope);
            retn.View = parent.View;
            return retn;
        }

        public UIElement CreateTemplate(int templateSpawnId, UIElement contextRoot, UIElement parent, TemplateScope scope) {
            UIElement retn = templateData.slots[templateSpawnId](contextRoot, parent, scope);

            InitializeElement(retn);

            return retn;
        }

        /// Returns the shell of a UI Element, space is allocated for children but no child data is associated yet, only a parent, view, and depth
        public UIElement CreateElementFromPool(UIElement element, UIElement parent, int childCount, int attributeCount, int originTemplateId) {
            // children get assigned in the template function but we need to setup the list here
            // ConstructedElement retn = templateData.ConstructElement(typeId);
            // UIElement element = retn.element;

            element.application = this;
            element.templateMetaData = templateData.templateMetaData[originTemplateId];

            const UIElementFlags flags = UIElementFlags.Enabled | UIElementFlags.Alive | UIElementFlags.NeedsUpdate;

            element.id = elementSystem.CreateElement(element, parent?.hierarchyDepth + 1 ?? 0, -999, -999, flags);
            element.flags = flags;
            element.style = new UIStyleSet(element);

            if (attributeCount > 0) {
                element.attributes = new StructList<ElementAttribute>(attributeCount);
                element.attributes.size = attributeCount;
            }

            element.parent = parent;

            if (parent != null) {
                elementSystem.AddChild(parent.id, element.id);
            }

            return element;
        }

        public TemplateMetaData GetTemplateMetaData(int metaDataId) {
            return templateData.templateMetaData[metaDataId];
        }

        public static int ResolveSlotId(string slotName, StructList<SlotUsage> slotList, int defaultId, out UIElement contextRoot) {
            if (slotList == null) {
                contextRoot = null;
                return defaultId;
            }

            for (int i = 0; i < slotList.size; i++) {
                if (slotList.array[i].slotName == slotName) {
                    contextRoot = slotList.array[i].context;
                    return slotList.array[i].slotId;
                }
            }

            contextRoot = null;
            return defaultId;
        }

        // Doesn't expect to create the root
        public void HydrateTemplate(int templateId, UIElement root, TemplateScope scope) {
            templateData.templates[templateId](root, scope);
            scope.Release();
        }

        public void Dispose() {
            viewRootIds.Dispose();
            elementSystem?.Dispose();
            layoutSystem?.Dispose();
            textSystem?.Dispose();
            resourceManager?.Dispose();
            renderSystem?.Dispose();
        }

        private static string PathHack([CallerFilePath] string path = "") {
            return path.Replace(nameof(Application) + ".cs", "");
        }

        internal static string GetSourceDirectory() {
            return PathHack();
        }

        public void Render(float surfaceWidth, float surfaceHeight, CommandBuffer commandBuffer) {
            if (!IsValid) return;
            renderSystem.Render(surfaceWidth, surfaceHeight, commandBuffer);
        }

        public static void SetCustomPainters(Dictionary<string, Type> dictionary) {
            s_CustomPainters = dictionary;
        }

        private GameObject gameObject;

        public GameObject GetDummyGameObject() {
            if (gameObject == null) {
                gameObject = new GameObject("UIForia::Dummy");
            }

            return gameObject;
        }

    }

}