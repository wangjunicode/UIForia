using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src.Rendering;
using Src;
using Src.Extensions;
using Src.Systems;
using Src.Util;

public abstract class UIView {

    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    protected readonly BindingSystem bindingSystem;
    protected readonly IStyleSystem styleSystem;
    protected ILayoutSystem layoutSystem;
    protected IRenderSystem renderSystem;
    protected IInputSystem inputSystem;
    protected readonly SkipTree<UIElement> elementTree;

    protected readonly List<ISystem> systems;
    protected readonly Type elementType;
    private UIElement rootElement;

    private readonly string template;

    public event Action<UIElement> onElementCreated;
    public event Action<UIElement> onElementDestroyed;
    public event Action<UIElement> onElementEnabled;
    public event Action<UIElement> onElementDisabled;

    public event Action onWillRefresh;
    public event Action onRefresh;
    public event Action onUpdate;
    public event Action onReady;
    public event Action onDestroy;

    // init -> prepare systems for elements
    // ready -> about to handle first frame, initial template is loaded by now
    // OnElementCreated -> init an entire hierarchy of elements (1 call for the whole group)
    // OnElementDestroyed -> destroy an entire hierarchy of elements (1 call for the whole group)

    protected readonly List<List<UIElement>> depthMap;
    protected static readonly DepthIndexComparer s_DepthIndexComparer = new DepthIndexComparer();

    static UIView() {
        ArrayPool<UIElement>.SetMaxPoolSize(64);
    }

    protected UIView(Type elementType, string template = null) {
        this.elementType = elementType;
        this.systems = new List<ISystem>();
        this.elementTree = new SkipTree<UIElement>();
        this.template = template;
        this.depthMap = new List<List<UIElement>>();

        styleSystem = new StyleSystem();
        bindingSystem = new BindingSystem();

        systems.Add(styleSystem);
        systems.Add(bindingSystem);
    }

    public IRenderSystem RenderSystem => renderSystem;
    public ILayoutSystem LayoutSystem => layoutSystem;

    // todo -- always call OnCreate & OnReady, don't call OnEnabled unless actually enabled

    public UIElement RootElement => rootElement;

    public void Initialize(bool forceTemplateReparse = false) {
        foreach (ISystem system in systems) {
            system.OnInitialize();
        }

        if (template != null) {
            CreateElementFromTemplate(TemplateParser.ParseTemplateFromString(elementType, template).CreateWithoutScope(this), null);
        }
        else {
            CreateElementFromTemplate(TemplateParser.GetParsedTemplate(elementType, forceTemplateReparse).CreateWithoutScope(this), null);
        }

        foreach (ISystem system in systems) {
            system.OnReady();
        }

        onReady?.Invoke();
    }

    public void Refresh() {
        onWillRefresh?.Invoke();
        foreach (ISystem system in systems) {
            system.OnReset();
        }
        elementTree.Clear();
        for (int i = 0; i < depthMap.Count; i++) {
            depthMap[i].Clear();
            List<UIElement> map = depthMap[i];
            ListPool<UIElement>.Release(ref map);
        }
        depthMap.Clear();
        rootElement = null;
        Initialize(true);
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

            element.depth = element.parent.depth + 1;
        }

        elementTree.AddItem(element);

        UIElement[] children = element.ownChildren;

        if (children == null || children.Length == 0) {
            return;
        }

        List<UIElement> list;

        if (depthMap.Count <= element.depth + 1) {
            list = ListPool<UIElement>.Get();
            depthMap.Add(list);
        }
        else {
            list = depthMap[element.depth + 1];
        }

        int idx = ~list.BinarySearch(0, list.Count, element.ownChildren[0], s_DepthIndexComparer);

        list.InsertRange(idx, children);

        for (int i = idx; i < list.Count; i++) {
            list[i].depthIndex = i;
        }

        for (int i = 0; i < children.Length; i++) {
            children[i].siblingIndex = i;
            InitHierarchy(children[i]);
        }
    }

    // create element, find where to insert in child depth list

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

    // todo take a template instead of an init data instance? (and scope)
    public void CreateElementFromTemplate(MetaData data, UIElement parent) {
        UIElement element = data.element;
        if (parent == null) {
            Debug.Assert(rootElement == null, nameof(rootElement) + " must be null if providing a null parent");

            element.flags |= UIElementFlags.AncestorEnabled;
            element.depth = 0;
            rootElement = element;
        }
        else {
            element.parent = parent;
            if (parent.isEnabled) {
                element.flags |= UIElementFlags.AncestorEnabled;
            }

            element.depth = element.parent.depth + 1;
        }

        List<UIElement> list;
        if (depthMap.Count <= element.depth) {
            list = ListPool<UIElement>.Get();
            depthMap.Add(list);
        }
        else {
            list = depthMap[element.depth];
        }

        InitHierarchy(data.element);

        int index = ~list.BinarySearch(0, list.Count, element, s_DepthIndexComparer);

        list.Insert(index, element);

        for (int i = index; i < list.Count; i++) {
            list[i].depthIndex = i;
        }


        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementCreatedFromTemplate(data);
        }

        InvokeOnCreate(data.element);
        InvokeOnReady(data.element);
        onElementCreated?.Invoke(data.element);
    }

    private static void InvokeOnCreate(UIElement element) {
        if (element.ownChildren != null) {
            for (int i = 0; i < element.ownChildren.Length; i++) {
                InvokeOnCreate(element.ownChildren[i]);
            }
        }

        element.flags |= UIElementFlags.Created;
        element.OnCreate();
    }

    private static void InvokeOnReady(UIElement element) {
        if (element.ownChildren != null) {
            for (int i = 0; i < element.ownChildren.Length; i++) {
                InvokeOnReady(element.ownChildren[i]);
            }
        }

        element.flags |= UIElementFlags.Initialized;
        element.OnReady();
    }

    // todo -- overload to destroy all children at once
    public void DestroyElement(UIElement element) {
        if ((element.flags & UIElementFlags.Destroyed) != 0) {
            return;
        }

        element.flags |= UIElementFlags.Destroyed;
        element.flags &= ~(UIElementFlags.Enabled);

        if (element.ownChildren != null && element.ownChildren.Length != 0) {
            elementTree.TraversePostOrder(element, (node) => {
                node.flags |= UIElementFlags.Destroyed;
                node.flags &= ~(UIElementFlags.Enabled);
            }, true);

            // traverse after setting all child flags for safety
            elementTree.TraversePostOrder(element, (node) => { node.OnDestroy(); }, true);
        }
        else {
            element.OnDestroy();
        }

        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementDestroyed(element);
        }

        RemoveUpdateDepthIndices(element);

        UIElement[] newChildList = ArrayPool<UIElement>.GetExactSize(element.parent.ownChildren.Length - 1);
        UIElement[] oldChildList = element.parent.ownChildren;
        UIElement[] oldTemplateChildList = null;
        if (element.parent != null) {
            newChildList = new UIElement[element.parent.ownChildren.Length - 1]; 
            oldChildList = element.parent.ownChildren;

            int idx = 0;
            for (int i = 0; i < oldChildList.Length; i++) {
                if (oldChildList[i] != element) {
                    newChildList[idx] = oldChildList[i];
                    newChildList[idx].siblingIndex = idx;
                    idx++;
                }
            }

            element.parent.ownChildren = newChildList;

            if (element.templateParent == element.parent) {
                if (element.parent.templateChildren == oldChildList) {
                    element.parent.templateChildren = newChildList;
                }
                else {
                    idx = 0;
                    UIElement[] newTemplateChildList = new UIElement[element.parent.templateChildren.Length - 1];
                    oldTemplateChildList = element.parent.templateChildren;
                    if (oldTemplateChildList != null) {
                        for (int i = 0; i < oldTemplateChildList.Length; i++) {
                            if (oldTemplateChildList[i] != element) {
                                newTemplateChildList[idx] = oldTemplateChildList[i];
                                // todo -- template sibling index?
                                idx++;
                            }
                        }
                    }

                    element.parent.templateChildren = newTemplateChildList;
                }
            }
        }

        elementTree.TraversePreOrder(element, (node) => {
            ArrayPool<UIElement>.Release(ref node.ownChildren);
            ArrayPool<UIElement>.Release(ref node.templateChildren);
            // todo -- if child is poolable, pool it here
        }, true);

        ArrayPool<UIElement>.Release(ref oldChildList);
        ArrayPool<UIElement>.Release(ref oldTemplateChildList);

        // todo -- if element is poolable, pool it here
    }

    public void DestroyChildren(UIElement element) {
        // todo - handle template parent :(

        if ((element.flags & UIElementFlags.Destroyed) != 0) {
            return;
        }

        if (element.ownChildren == null || element.ownChildren.Length == 0) {
            return;
        }

        for (int i = 0; i < element.ownChildren.Length; i++) {
            UIElement child = element.ownChildren[i];
            child.flags |= UIElementFlags.Destroyed;
            child.flags &= ~(UIElementFlags.Enabled);

            elementTree.TraversePostOrder(element, (node) => {
                node.flags |= UIElementFlags.Destroyed;
                node.flags &= ~(UIElementFlags.Enabled);
            }, true);

            elementTree.TraversePostOrder(element, element, (node, e) => {
                if (node != e) {
                    node.OnDestroy();
                }
            }, true);
        }

        RemoveUpdateDepthIndicesStep(element);

        for (int i = 0; i < element.ownChildren.Length; i++) {
            for (int j = 0; j < systems.Count; j++) {
                systems[j].OnElementDestroyed(element.ownChildren[i]);
            }
        }

        for (int i = 0; i < element.ownChildren.Length; i++) {
            elementTree.TraversePostOrder(element.ownChildren[i], (node) => {
                ArrayPool<UIElement>.Release(ref node.ownChildren);
                ArrayPool<UIElement>.Release(ref node.templateChildren);
            }, true);
            elementTree.RemoveHierarchy(element.ownChildren[i]);
        }

        element.ownChildren = ArrayPool<UIElement>.Empty;
        element.templateChildren = ArrayPool<UIElement>.Empty;
    }

    protected void RemoveUpdateDepthIndices(UIElement element) {
        List<UIElement> list = depthMap[element.depth];
        list.RemoveAt(element.depthIndex);
        for (int i = element.depthIndex; i < list.Count; i++) {
            list[i].depthIndex = i;
        }

        RemoveUpdateDepthIndicesStep(element);
    }

    protected void RemoveUpdateDepthIndicesStep(UIElement element) {
        if (element.ownChildren == null || element.ownChildren.Length == 0) {
            return;
        }

        List<UIElement> list = depthMap[element.depth + 1];
        int idx = element.ownChildren[0].depthIndex;
        list.RemoveRange(idx, element.ownChildren.Length);

        for (int i = idx; i < list.Count; i++) {
            list[i].depthIndex = i;
        }

        for (int i = idx; i < element.ownChildren.Length; i++) {
            RemoveUpdateDepthIndicesStep(element.ownChildren[i]);
        }
    }

    public virtual void OnDestroy() { }

    public virtual void Update() {
        styleSystem.OnUpdate();
        bindingSystem.OnUpdate();
        layoutSystem.OnUpdate();
        inputSystem.OnUpdate();
        renderSystem.OnUpdate();

        elementTree.ConditionalTraversePreOrder((element) => {
            if (element == null) return true;
            if (element.isDisabled) return false;
            element.OnUpdate();
            return true;
        });

        onUpdate?.Invoke();
    }

    public void EnableElement(UIElement element) {
        // no-op for already enabled elements
        if (element.isSelfEnabled) return;

        element.flags |= UIElementFlags.Enabled;

        // if element is not enabled (ie has a disabled ancestor), no-op 
        if (!element.isEnabled) return;

        element.OnEnable();
        // if element is now enabled we need to walk it's children
        // and set enabled ancestor flags until we find a self-disabled child
        elementTree.ConditionalTraversePreOrder(element, (child) => {
            child.flags |= UIElementFlags.AncestorEnabled;
            if (child.isSelfDisabled) return false;

            child.OnEnable(); // todo -- maybe enqueue and flush calls after so we don't have buffer problems

            return true;
        });

        foreach (ISystem system in systems) {
            system.OnElementEnabled(element);
        }

        onElementEnabled?.Invoke(element);
    }

    public void DisableElement(UIElement element) {
        // no-op for already disabled elements
        if (element.isSelfDisabled) return;

        element.flags &= ~(UIElementFlags.Enabled);

        // if element was already disabled via ancestor, no-op
        if (element.hasDisabledAncestor) {
            return;
        }

        element.OnDisable();

        elementTree.ConditionalTraversePreOrder(element, (child) => {
            child.flags &= ~(UIElementFlags.AncestorEnabled);
            if (child.isSelfDisabled) return false;

            child.OnDisable(); // todo -- enqueue for later

            return true;
        });

        foreach (ISystem system in systems) {
            system.OnElementDisabled(element);
        }

        onElementDisabled?.Invoke(element);
    }

    public UIElement GetElement(int elementId) {
        return elementTree.GetItem(elementId);
    }

}