using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src;
using Src.Systems;

public class OrphanView : UIView {

    public OrphanView() : base(null) { }

    internal override bool SetElementParent(UIElement element, UIElement newParent) {
        if (!newParent.CanAcceptChild(element)) {
            return false;
        }

        element.view = this;
        element.parent = newParent;
        return true;
    }

}

public abstract class UIView {

    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    // todo -- move to interfaces
    protected internal readonly BindingSystem bindingSystem;

    protected readonly StyleSystem styleSystem;
    protected readonly SkipTree<UIElement> elementTree;

    protected readonly List<ISystem> systems;
    protected readonly Type elementType;
    private UIElement rootElement;

    private readonly string template;

    // init -> prepare systems for elements
    // ready -> about to handle first frame, initial template is loaded by now
    // OnElementCreated -> init an entire hierarchy of elements (1 call for the whole group)
    // OnElementDestroyed -> destroy an entire hierarchy of elements (1 call for the whole group)

    protected UIView(Type elementType, string template = null) {
        this.elementType = elementType;
        this.systems = new List<ISystem>();
        this.elementTree = new SkipTree<UIElement>();
        this.template = template;

        styleSystem = new StyleSystem();
        bindingSystem = new BindingSystem();

        systems.Add(styleSystem);
        systems.Add(bindingSystem);
    }

    public UIElement RootElement => rootElement;

    internal static readonly OrphanView OrphanView = new OrphanView();

    public void Initialize(bool forceTemplateReparse = false) {
        foreach (ISystem system in systems) {
            system.OnInitialize();
        }

        if (template != null) {
            CreateElement(TemplateParser.ParseTemplateFromString(elementType, template).CreateWithoutScope(this), null);
        }
        else {
            CreateElement(TemplateParser.GetParsedTemplate(elementType, forceTemplateReparse).CreateWithoutScope(this), null);
        }

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

    internal virtual bool SetElementParent(UIElement element, UIElement newParent) {
        if (!newParent.CanAcceptChild(element)) {
            return false;
        }

        UIElement oldParent = element.parent;
        element.parent = newParent;
        
        if (element.view == null) {
            InitHierarchy(element);
        }

        element.view = this;


        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementParentChanged(element, oldParent, newParent);
        }

        return true;
    }

    protected void InitHierarchy(UIElement element) {
        element.view = this;

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
        if (children == null) {
            return;
        }

        for (int i = 0; i < children.Length; i++) {
            children[i].siblingIndex = i;
            InitHierarchy(children[i]);
        }
    }

    // todo take a template instead of an init data instance? (and scope)
    public void CreateElement(MetaData data, UIElement parent) {
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

        InitHierarchy(data.element);
        data.element.view = this;

        for (int i = 0; i < systems.Count; i++) {
            systems[i].OnElementCreated(data);
        }

        InvokeOnCreate(data);
        InvokeOnReady(data);
    }

    private static void InvokeOnCreate(MetaData elementData) {
        for (int i = 0; i < elementData.children.Count; i++) {
            InvokeOnCreate(elementData.children[i]);
        }

        elementData.element.OnCreate();
    }

    private static void InvokeOnReady(MetaData elementData) {
        for (int i = 0; i < elementData.children.Count; i++) {
            InvokeOnReady(elementData.children[i]);
        }

        elementData.element.OnReady();
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