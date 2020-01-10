using System;
using System.Collections.Generic;
using Src.Systems;
using UIForia.Animation;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Parsing;
using UIForia.Rendering;
using UIForia.Routing;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia {

    public abstract class Application {

#if UNITY_EDITOR
        public static List<Application> Applications = new List<Application>();
#endif

        public readonly string id;
        private static int ElementIdGenerator;
        public static int NextElementId => ElementIdGenerator++;
        private string templateRootPath;

        //  protected readonly BindingSystem m_BindingSystem;
        protected readonly IStyleSystem m_StyleSystem;
        protected ILayoutSystem m_LayoutSystem;
        protected IRenderSystem m_RenderSystem;
        protected IInputSystem m_InputSystem;
        protected RoutingSystem m_RoutingSystem;
        protected AnimationSystem m_AnimationSystem;
        protected LinqBindingSystem linqBindingSystem;

        protected ResourceManager resourceManager;

        public readonly StyleSheetImporter styleImporter;
        private readonly IntMap<UIElement> elementMap;
        protected readonly List<ISystem> m_Systems;

        public event Action<UIElement> onElementRegistered;

//        public event Action<UIElement> onElementCreated;
        public event Action<UIElement> onElementDestroyed;

        public event Action<UIElement> onElementEnabled;
//        public event Action<UIElement> onElementDisabled;

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
        private StructStack<ElemRef> elemRefStack;

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
            this.elemRefStack = new StructStack<ElemRef>(32);

            m_StyleSystem = new StyleSystem();
            m_LayoutSystem = new AwesomeLayoutSystem(this);
            m_InputSystem = new GameInputSystem(m_LayoutSystem);
            m_RenderSystem = new VertigoRenderSystem(Camera.current, this);
            m_RoutingSystem = new RoutingSystem();
            m_AnimationSystem = new AnimationSystem();
            linqBindingSystem = new LinqBindingSystem();

            elementMap = new IntMap<UIElement>();

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

            UIElement rootElement = templateData.templates[0].Invoke(null, new TemplateScope(this, null));

            UIView view = new UIView(this, "Default", rootElement, Matrix4x4.identity, new Size(Screen.width, Screen.height));

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
            this.elemRefStack = new StructStack<ElemRef>(32);
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

            elementMap = new IntMap<UIElement>();

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
                        throw new Exception($"Failed to register a custom painter with the name {paintAttr.name} from type {type.FullName} because it was already registered.");
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

        public Camera Camera { get; private set; }

        public LinqBindingSystem LinqBindingSystem => linqBindingSystem;
        public ResourceManager ResourceManager => resourceManager;

        public float Width => Screen.width;
        public float Height => Screen.height;


        public void SetCamera(Camera camera) {
            Camera = camera;
            RenderSystem.SetCamera(camera);
        }

        private int nextViewId = 0;

//        public UIView CreateView(string name, Rect rect, Type type, string template = null) {
//            UIView view = GetView(name);
//
//            if (view == null) {
//                view = new UIView(nextViewId++, name, this, rect, m_Views.Count, type, template);
//                m_Views.Add(view);
//
//                for (int i = 0; i < m_Systems.Count; i++) {
//                    m_Systems[i].OnViewAdded(view);
//                }
//
//                onViewAdded?.Invoke(view);
//            }
//            else {
//                if (view.RootElement.GetChild(0).GetType() != type) {
//                    throw new Exception($"A view named {name} with another root type ({view.RootElement.GetChild(0).GetType()}) already exists.");
//                }
//
//                view.Viewport = rect;
//            }
//
//            return view;
//        }

        public UIView RemoveView(UIView view) {
            if (!m_Views.Remove(view)) return null;

            for (int i = 0; i < m_Systems.Count; i++) {
                m_Systems[i].OnViewRemoved(view);
            }

            onViewRemoved?.Invoke(view);
            DestroyElement(view.dummyRoot);
            return view;
        }

        public UIElement CreateElement(Type type) {
            throw new NotImplementedException();
//            if (type == null) {
//                return null;
//            }
//
//            return templateParser.GetParsedTemplate(type)?.Create();
        }

        public T CreateElement<T>() where T : UIElement {
            throw new NotImplementedException();

//            return templateParser.GetParsedTemplate(typeof(T))?.Create() as T;
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

            elementMap.Clear();
            styleImporter.Reset();
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

            if (toInternalDestroy.Count > 0) {
                UIView view = toInternalDestroy[0].View;
                for (int i = 0; i < toInternalDestroy.Count; i++) {
                    view.ElementDestroyed(toInternalDestroy[i]);
                    toInternalDestroy[i].InternalDestroy();
                    elementPool.Release(toInternalDestroy[i]);
                    elementMap.Remove(toInternalDestroy[i].id);
                }
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

        public void Update() {
//            TemplateSettings settings = new TemplateSettings();
//            settings.applicationName = frameId.ToString();
//            settings.assemblyName = "Assembly-CSharp";
//            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated");
//            settings.codeFileExtension = "generated.cs";
//            settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + frameId;
//            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
//            TemplateCompiler compiler = new TemplateCompiler(settings);
//
//            CompiledTemplateData compiledOutput = new RuntimeTemplateData(settings);
//
//            Debug.Log("Starting");
//            Stopwatch watch = new Stopwatch();
//            watch.Start();
//            compiler.CompileTemplates(m_Views[0].RootElement.GetType(), compiledOutput);
//            watch.Stop();
//            Debug.Log("loaded app in " + watch.ElapsedMilliseconds);
//            
            m_InputSystem.OnUpdate();

            linqBindingSystem.OnUpdate();

            m_StyleSystem.OnUpdate();

            m_AnimationSystem.OnUpdate();

            m_InputSystem.OnLateUpdate();

            m_RoutingSystem.OnUpdate();

            m_LayoutSystem.OnUpdate();

            m_BeforeUpdateTaskSystem.OnUpdate();

            m_RenderSystem.OnUpdate();

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
            AnimationData data;
            styleImporter.ImportStyleSheetFromFile(fileName).TryGetAnimationData(animationName, out data);
            return data;
        }

        internal void InsertChild(UIElement parent, CompiledTemplate template, int index) {
//            UIElement ptr = parent;
//            LinqBindingNode bindingNode = null;
//
//            while (ptr != null) {
//                bindingNode = ptr.bindingNode;
//
//                if (bindingNode != null) {
//                    break;
//                }
//
//                ptr = ptr.parent;
//            }
//
//            TemplateScope2 templateScope = new TemplateScope2(this, null);
//            UIElement root = elementPool.Get(template.elementType);
//            root.siblingIndex = index;
//
//            if (parent.isEnabled) {
//                root.flags |= UIElementFlags.AncestorEnabled;
//            }
//
//            root.depth = parent.depth + 1;
//            root.View = parent.View;
//            template.Create(root, templateScope);
//
//            parent.children.Insert(index, root);
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

            UIView view = parent.View;
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

                elementMap[current.id] = current;

                if ((current.flags & UIElementFlags.Created) == 0) {
                    current.flags |= UIElementFlags.Created;
//                    current.style.Initialize();
                    for (int i = 0; i < m_Systems.Count; i++) {
                        m_Systems[i].OnElementCreated(current);
                    }

                    onElementRegistered?.Invoke(current);
                    current.OnCreate();
                    view.ElementRegistered(current);
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

        // might be the same as HydrateTemplate really but with templateId not hard coded
        public UIElement CreateSlot(int templateId, UIElement root, UIElement parent, TemplateScope scope) {
           throw new NotImplementedException("Deprecate");
        }

        public UIElement CreateSlot2(string slotName, TemplateScope scope, int defaultSlotId, UIElement root, UIElement parent) {
            int slotId = ResolveSlotId(slotName, scope.slotInputs, defaultSlotId, out UIElement contextRoot);
            if (contextRoot == null) {
                Assert.AreEqual(slotId, defaultSlotId);
                contextRoot = root;
            }
            UIElement retn = templateData.slots[slotId](contextRoot, parent, scope);
            // retn.bindingNode = new LinqBindingNode();
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
            retn.View = parent?.View;
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