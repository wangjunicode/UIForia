using System;
using UIForia;
using UnityEngine;
using Application = UIForia.Application;

public class UIView {

//    private static int ElementIdGenerator;
//    public static int NextElementId => ElementIdGenerator++;

    public event Action<UIElement> onElementCreated;
    public event Action<UIElement> onElementDestroyed;
    public event Action<UIElement> onElementEnabled;
    public event Action<UIElement> onElementDisabled;

//    public event Action onWillRefresh;
//    public event Action onRefresh;
//    public event Action onUpdate;
//    public event Action onReady;
//    public event Action onDestroy;

    private readonly Type m_ElementType;
    private readonly string m_Template;
    
    public int Depth { get; set; }
    public Rect Viewport { get; set; }
    public UIElement RootElement { get; private set; }
    public readonly Application Application;
    
    internal UIView(Application app, Rect rect, int depth, Type elementType, string template = null) {
        this.Application = app;
        this.Viewport = rect;
        this.Depth = depth;
        this.m_Template = template;
        this.m_ElementType = elementType;

        if (template != null) {
            this.RootElement = TemplateParser.ParseTemplateFromString(elementType, template).Create(this);
        }
        else {
            this.RootElement = TemplateParser.GetParsedTemplate(elementType).Create(this);
        }

        this.RootElement.view = this;

    }


    public void EnableElement(UIElement element) {
        Application.DoEnableElement(element);
    }
    
    public void DisableElement(UIElement element) {
        Application.DoDisableElement(element);
    }
    public void Refresh() {
        if (m_Template != null) {
            this.RootElement = TemplateParser.ParseTemplateFromString(m_ElementType, m_Template).Create(this);
        }
        else {
            this.RootElement = TemplateParser.GetParsedTemplate(m_ElementType).Create(this);
        }
    }

    public void Destroy() {
        
    }

    internal void InvokeElementAdded(UIElement element) {
        onElementCreated?.Invoke(element);
    }
    
    internal void InvokeElementEnabled(UIElement element) {
        onElementEnabled?.Invoke(element);
    }
    
    internal void InvokeElementDisabled(UIElement element) {
        onElementDisabled?.Invoke(element);
    }
    
    internal void InvokeElementDestroyed(UIElement element) {
        onElementDestroyed?.Invoke(element);
    }
    
//    public void Refresh() {
//        onWillRefresh?.Invoke();
//        foreach (ISystem system in systems) {
//            system.OnReset();
//        }
//        elementTree.Clear();
//        for (int i = 0; i < depthMap.Count; i++) {
//            depthMap[i].Clear();
//            List<UIElement> map = depthMap[i];
//            ListPool<UIElement>.Release(ref map);
//        }
//        depthMap.Clear();
//        rootElement = null;
//        Initialize(true);
//        onRefresh?.Invoke();
//    }

//
//    protected class DepthIndexComparer : IComparer<UIElement> {
//
//        public int Compare(UIElement x, UIElement y) {
//            if (x.parent == y.parent) {
//                return x.siblingIndex > y.siblingIndex ? 1 : -1;
//            }
//
//            UIElement p0 = x.parent;
//            UIElement p1 = y.parent;
//
//            while (p0.parent != p1.parent) {
//                p0 = p0.parent;
//                p1 = p1.parent;
//            }
//
//            return p0.siblingIndex > p1.siblingIndex ? 1 : -1;
//        }
//
//    }
//
//    // todo take a template instead of an init data instance? (and scope)
//    public void CreateElementFromTemplate(UIElement element, UIElement parent) {
//        if (parent == null) {
//            Debug.Assert(rootElement == null, nameof(rootElement) + " must be null if providing a null parent");
//
//            element.flags |= UIElementFlags.AncestorEnabled;
//            element.depth = 0;
//            rootElement = element;
//        }
//        else {
//            element.parent = parent;
//            if (parent.isEnabled) {
//                element.flags |= UIElementFlags.AncestorEnabled;
//            }
//
//            element.depth = element.parent.depth + 1;
//        }
//
//        List<UIElement> list;
//        if (depthMap.Count <= element.depth) {
//            list = ListPool<UIElement>.Get();
//            depthMap.Add(list);
//        }
//        else {
//            list = depthMap[element.depth];
//        }
//
//        InitHierarchy(element);
//
//        int index = ~list.BinarySearch(0, list.Count, element, s_DepthIndexComparer);
//
//        list.Insert(index, element);
//
//        for (int i = index; i < list.Count; i++) {
//            list[i].depthIndex = i;
//        }
//
//        for (int i = 0; i < systems.Count; i++) {
//            systems[i].OnElementCreatedFromTemplate(element);
//        }
//
//        InvokeOnCreate(element);
//        InvokeOnReady(element);
//        onElementCreated?.Invoke(element);
//    }
//
//    private static void InvokeOnCreate(UIElement element) {
//        if (element.children != null) {
//            for (int i = 0; i < element.children.Length; i++) {
//                InvokeOnCreate(element.children[i]);
//            }
//        }
//
//        element.flags |= UIElementFlags.Created;
//        element.OnCreate();
//    }
//
//    private static void InvokeOnReady(UIElement element) {
//        if (element.children != null) {
//            for (int i = 0; i < element.children.Length; i++) {
//                InvokeOnReady(element.children[i]);
//            }
//        }
//
//        element.flags |= UIElementFlags.Initialized;
//        element.OnReady();
//    }
//
//    // todo -- overload to destroy all children at once
//    public void DestroyElement(UIElement element) {
//        if ((element.flags & UIElementFlags.Destroyed) != 0) {
//            return;
//        }
//
//        element.flags |= UIElementFlags.Destroyed;
//        element.flags &= ~(UIElementFlags.Enabled);
//
//        if (element.children != null && element.children.Length != 0) {
//            elementTree.TraversePostOrder(element, (node) => {
//                node.flags |= UIElementFlags.Destroyed;
//                node.flags &= ~(UIElementFlags.Enabled);
//            }, true);
//
//            // traverse after setting all child flags for safety
//            elementTree.TraversePostOrder(element, (node) => { node.OnDestroy(); }, true);
//        }
//        else {
//            element.OnDestroy();
//        }
//
//        for (int i = 0; i < systems.Count; i++) {
//            systems[i].OnElementDestroyed(element);
//        }
//
//        RemoveUpdateDepthIndices(element);
//
//        UIElement[] newChildList = ArrayPool<UIElement>.GetExactSize(element.parent.children.Length - 1);
//        UIElement[] oldChildList = element.parent.children;
//        UIElement[] oldTemplateChildList = null;
//        if (element.parent != null) {
//            newChildList = new UIElement[element.parent.children.Length - 1]; 
//            oldChildList = element.parent.children;
//
//            int idx = 0;
//            for (int i = 0; i < oldChildList.Length; i++) {
//                if (oldChildList[i] != element) {
//                    newChildList[idx] = oldChildList[i];
//                    newChildList[idx].siblingIndex = idx;
//                    idx++;
//                }
//            }
//
//            element.parent.children = newChildList;
//
//        }
//
//        elementTree.TraversePreOrder(element, (node) => {
//            ArrayPool<UIElement>.Release(ref node.children);
//            // todo -- if child is poolable, pool it here
//        }, true);
//
//        ArrayPool<UIElement>.Release(ref oldChildList);
//        ArrayPool<UIElement>.Release(ref oldTemplateChildList);
//
//        // todo -- if element is poolable, pool it here
//    }
//
//    public void DestroyChildren(UIElement element) {
//        // todo - handle template parent :(
//
//        if ((element.flags & UIElementFlags.Destroyed) != 0) {
//            return;
//        }
//
//        if (element.children == null || element.children.Length == 0) {
//            return;
//        }
//
//        for (int i = 0; i < element.children.Length; i++) {
//            UIElement child = element.children[i];
//            child.flags |= UIElementFlags.Destroyed;
//            child.flags &= ~(UIElementFlags.Enabled);
//
//            elementTree.TraversePostOrder(element, (node) => {
//                node.flags |= UIElementFlags.Destroyed;
//                node.flags &= ~(UIElementFlags.Enabled);
//            }, true);
//
//            elementTree.TraversePostOrder(element, element, (node, e) => {
//                if (node != e) {
//                    node.OnDestroy();
//                }
//            }, true);
//        }
//
//        RemoveUpdateDepthIndicesStep(element);
//
//        for (int i = 0; i < element.children.Length; i++) {
//            for (int j = 0; j < systems.Count; j++) {
//                systems[j].OnElementDestroyed(element.children[i]);
//            }
//        }
//
//        for (int i = 0; i < element.children.Length; i++) {
//            elementTree.TraversePostOrder(element.children[i], (node) => {
//                ArrayPool<UIElement>.Release(ref node.children);
//            }, true);
//            elementTree.RemoveHierarchy(element.children[i]);
//        }
//
//        element.children = ArrayPool<UIElement>.Empty;
//    }
//
//    protected void RemoveUpdateDepthIndices(UIElement element) {
//        List<UIElement> list = depthMap[element.depth];
//        list.RemoveAt(element.depthIndex);
//        for (int i = element.depthIndex; i < list.Count; i++) {
//            list[i].depthIndex = i;
//        }
//
//        RemoveUpdateDepthIndicesStep(element);
//    }
//
//    protected void RemoveUpdateDepthIndicesStep(UIElement element) {
//        if (element.children == null || element.children.Length == 0) {
//            return;
//        }
//
//        List<UIElement> list = depthMap[element.depth + 1];
//        int idx = element.children[0].depthIndex;
//        list.RemoveRange(idx, element.children.Length);
//
//        for (int i = idx; i < list.Count; i++) {
//            list[i].depthIndex = i;
//        }
//
//        for (int i = idx; i < element.children.Length; i++) {
//            RemoveUpdateDepthIndicesStep(element.children[i]);
//        }
//    }
//
//    public virtual void OnDestroy() { }
//
//    public virtual void Update() {
//        styleSystem.OnUpdate();
//        bindingSystem.OnUpdate();
//        layoutSystem.OnUpdate();
//        inputSystem.OnUpdate();
//        renderSystem.OnUpdate();
//
//        elementTree.ConditionalTraversePreOrder((element) => {
//            if (element == null) return true;
//            if (element.isDisabled) return false;
//            element.OnUpdate();
//            return true;
//        });
//
//        onUpdate?.Invoke();
//    }
//
//    public void EnableElement(UIElement element) {
//        // no-op for already enabled elements
//        if (element.isSelfEnabled) return;
//
//        element.flags |= UIElementFlags.Enabled;
//
//        // if element is not enabled (ie has a disabled ancestor), no-op 
//        if (!element.isEnabled) return;
//
//        element.OnEnable();
//        // if element is now enabled we need to walk it's children
//        // and set enabled ancestor flags until we find a self-disabled child
//        elementTree.ConditionalTraversePreOrder(element, (child) => {
//            child.flags |= UIElementFlags.AncestorEnabled;
//            if (child.isSelfDisabled) return false;
//
//            child.OnEnable(); // todo -- maybe enqueue and flush calls after so we don't have buffer problems
//
//            return true;
//        });
//
//        foreach (ISystem system in systems) {
//            system.OnElementEnabled(element);
//        }
//
//        onElementEnabled?.Invoke(element);
//    }
//
//    public void DisableElement(UIElement element) {
//        // no-op for already disabled elements
//        if (element.isSelfDisabled) return;
//
//        element.flags &= ~(UIElementFlags.Enabled);
//
//        // if element was already disabled via ancestor, no-op
//        if (element.hasDisabledAncestor) {
//            return;
//        }
//
//        element.OnDisable();
//
//        elementTree.ConditionalTraversePreOrder(element, (child) => {
//            child.flags &= ~(UIElementFlags.AncestorEnabled);
//            if (child.isSelfDisabled) return false;
//
//            child.OnDisable(); // todo -- enqueue for later
//
//            return true;
//        });
//
//        foreach (ISystem system in systems) {
//            system.OnElementDisabled(element);
//        }
//
//        onElementDisabled?.Invoke(element);
//    }
//
//    public UIElement GetElement(int elementId) {
//        return elementTree.GetItem(elementId);
//    }

}