using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src;
using Src.Systems;

public abstract class UIView : IElementRegistry {

    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    // todo -- move to interfaces
    protected readonly BindingSystem bindingSystem;

    //protected readonly LifeCycleSystem lifeCycleSystem;
    protected readonly StyleSystem styleSystem;
    protected readonly SkipTree<UIElement> elementTree;

    protected readonly List<ISystem> systems;
    protected readonly Type elementType;
    private UIElement rootElement;

    // init -> prepare systems for elements
    // ready -> about to handle first frame, initial template is loaded by now
    // OnElementCreated -> init an entire hierarchy of elements (1 call for the whole group)
    // OnElementDestroyed -> destroy an entire hierarchy of elements (1 call for the whole group)

    protected UIView(Type elementType) {
        this.elementType = elementType;
        this.systems = new List<ISystem>();
        this.elementTree = new SkipTree<UIElement>();

        styleSystem = new StyleSystem(this);
        bindingSystem = new BindingSystem();

        systems.Add(styleSystem);
        systems.Add(bindingSystem);
    }

    public UIElement RootElement => rootElement;
    
    public void Initialize(bool forceTemplateReparse = false) {
        foreach (ISystem system in systems) {
            system.OnInitialize();
        }

        CreateElement(TemplateParser.GetParsedTemplate(elementType, forceTemplateReparse).CreateWithoutScope(this), null);

        foreach (ISystem system in systems) {
            system.OnReady();
        }
    }

    public void Refresh() {
        foreach (ISystem system in systems) {
            system.OnReset();
        }

        rootElement = null;
        Initialize(true);
    }

    protected void InitHierarchy(InitData elementData) {
        if (elementData.element.parent == null) {
            elementData.element.flags |= UIElementFlags.AncestorEnabled;
        }
        else {
            if (elementData.element.parent.isEnabled) {
                elementData.element.flags |= UIElementFlags.AncestorEnabled;
            }
        }

        elementTree.AddItem(elementData.element);

        for (int i = 0; i < elementData.children.Count; i++) {
            InitHierarchy(elementData.children[i]);
        }
    }

    // todo take a template instead of an init data instance? (and scope)
    public void CreateElement(InitData data, UIElement parent) {
        if (parent == null) {
            Debug.Assert(rootElement == null, nameof(rootElement) + " must be null if providing a null parent");

            data.element.flags |= UIElementFlags.AncestorEnabled;

            rootElement = data.element;
        }
        else {
            data.element.parent = parent;
            if (parent.isEnabled) {
                data.element.flags |= UIElementFlags.AncestorEnabled;
            }
        }

        InitHierarchy(data);

        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementCreated(data);
        }

        InvokeOnCreate(data);
    }

    private void InvokeOnCreate(InitData elementData) {
        for (int i = 0; i < elementData.children.Count; i++) {
            InvokeOnCreate(elementData.children[i]);
        }

        elementData.element.OnCreate();
    }

    public void DestroyElement(UIElement element) {
        if ((element.flags & UIElementFlags.Destroyed) != 0) {
            return;
        }

        element.flags |= UIElementFlags.Destroyed;
        element.flags &= ~(UIElementFlags.Enabled);
        elementTree.TraversePreOrder(element, (node) => {
            node.flags |= UIElementFlags.Destroyed;
            node.flags &= ~(UIElementFlags.Enabled);
        }, true);
        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementDestroyed(element);
        }
    }

    public virtual void OnDestroy() { }

    public virtual void Update() {
        elementTree.ConditionalTraversePreOrder((element) => {
            if (element == null) return true;
            if (element.isDisabled) return false;
            element.OnUpdate();
            return true;
        });
        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnUpdate();
        }
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
    }

    public UIElement GetElement(int elementId) {
        return elementTree.GetItem(elementId);
    }

}

/*
 enableTree.TraversePreOrder((element) => {
            element.flags |= UIElementFlags.Enabled;
            element.flags &= ~(UIElementFlags.PendingEnable);
        });

        enableTree.GetRootItems(scratchList);

        for (int i = 0; i < scratchList.Count; i++) {
            UIElement root = scratchList[i];
            if (root.hasDisabledAncestor) continue;

            // enable tree will contain everything that had EnableElement called on it
            // we need to walk the tree until we find a disabled child and set the AncestorEnabled flag
            // if the element was disabled before the flag is updated, invoke it's OnEnabled handler
            elementTree.ConditionalTraversePreOrder(root, (element) => {
                bool wasDisabled = element.isDisabled;
                element.flags |= UIElementFlags.AncestorEnabled;

                if ((element.flags & UIElementFlags.PendingEnable) != 0) {
                    element.flags |= UIElementFlags.Enabled;
                    element.flags &= ~(UIElementFlags.PendingEnable);
                }

                if (wasDisabled && !element.isDisabled) {
                    // invoke OnEnabled if present
                    element.OnEnable();
                }

                return element.isSelfDisabled;
            });
        }

        enableTree.Clear();
        scratchList.Clear();
        */