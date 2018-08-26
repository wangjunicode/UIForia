using System;
using System.Collections.Generic;
using Rendering;
using Src;
using Src.Systems;

public abstract class UIView {

    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    private UIElement root;

    // todo -- move to protected & interfaces
    protected readonly BindingSystem bindingSystem;
    protected readonly LifeCycleSystem lifeCycleSystem;
    //protected readonly ILayoutSystem layoutSystem;
    protected readonly StyleSystem styleSystem;
    protected readonly ElementRegistrySystem elementSystem;

    protected List<ISystem> systems;
    private readonly Type elementType;

    protected UIView(Type elementType) {
        this.elementType = elementType;
        this.systems = new List<ISystem>();
        elementSystem = new ElementRegistrySystem();
        styleSystem = new StyleSystem();
        bindingSystem = new BindingSystem();
        lifeCycleSystem = new LifeCycleSystem();
        systems.Add(elementSystem);
        systems.Add(styleSystem);
        systems.Add(bindingSystem);
        systems.Add(lifeCycleSystem);
    }

    public UIElement Root => root;
    
    protected abstract IRenderSystem renderSystem { get; set; }
    protected abstract ILayoutSystem layoutSystem { get; set; }
    protected abstract IInputSystem inputSystem { get; set; }
    
    public abstract void Render();
    
    public virtual void Refresh() {
        lifeCycleSystem.OnReset();
        elementSystem.OnReset(); 
        bindingSystem.OnReset();
        renderSystem.OnReset();
        layoutSystem.OnReset();
        styleSystem.OnReset();
        
        root = TemplateParser.GetParsedTemplate(elementType, true).CreateWithoutScope(this);
        
        lifeCycleSystem.OnInitialize();
        styleSystem.OnInitialize();
        layoutSystem.OnInitialize();
        renderSystem.OnInitialize();
        inputSystem.OnInitialize();
        bindingSystem.OnInitialize();
    }

    // todo -- make this stuff event based to make dependency graph explicit or removed
    public virtual void Register(UIElementCreationData elementData) {
        elementSystem.OnElementCreated(elementData);
        lifeCycleSystem.OnElementCreated(elementData);
        styleSystem.OnElementCreated(elementData);
        layoutSystem.OnElementCreated(elementData);
        renderSystem.OnElementCreated(elementData);
        bindingSystem.OnElementCreated(elementData);
        inputSystem.OnElementCreated(elementData);
    }

    public virtual void DestroyElement(UIElement element) {
        lifeCycleSystem.OnElementDestroyed(element);
        inputSystem.OnElementDestroyed(element);
        bindingSystem.OnElementDestroyed(element);
        renderSystem.OnElementDestroyed(element);
        layoutSystem.OnElementDestroyed(element);
        styleSystem.OnElementDestroyed(element);
        elementSystem.OnElementDestroyed(element);
    }
    
    public virtual void OnCreate() {
        root = TemplateParser.GetParsedTemplate(elementType).CreateWithoutScope(this);
        layoutSystem.OnInitialize();
        renderSystem.OnInitialize();
        styleSystem.OnInitialize();
        inputSystem.OnInitialize();
        bindingSystem.OnInitialize();
        lifeCycleSystem.OnInitialize();
    }

    public virtual void OnDestroy() {
        inputSystem.OnDestroy();
        lifeCycleSystem.OnDestroy();
        bindingSystem.OnDestroy();
        renderSystem.OnDestroy();
        layoutSystem.OnDestroy();
        styleSystem.OnDestroy();
        elementSystem.OnDestroy();
    }
    
    public virtual void Update() {
        layoutSystem.OnUpdate();
        bindingSystem.OnUpdate();
        lifeCycleSystem.OnUpdate();
        renderSystem.OnUpdate();
    }

    // todo -- enqueue these to be flushed at end of update
    public void EnableElement(UIElement element) {
        if ((element.flags & UIElementFlags.Enabled) != 0) {
            return;
        }
        element.flags |= UIElementFlags.Enabled;
        // expect life cycle system in invoke handlers for this
        if (element.parent != null && (element.parent.flags & UIElementFlags.Enabled) != 0) {
            foreach (ISystem system in systems) {
                system.OnElementEnabled(element);
            }
        }
    }

    // todo -- enqueue these to be flushed at end of update
    public void DisableElement(UIElement element) {
        if ((element.flags & UIElementFlags.Enabled) == 0) {
            return;
        }
        element.flags &= ~(UIElementFlags.Enabled);
        // expect life cycle system in invoke handlers for this
        if (element.parent != null && (element.parent.flags & UIElementFlags.Enabled) != 0) {
            foreach (ISystem system in systems) {
                system.OnElementDisabled(element);
            }
        }
    }

    // todo -- enqueue these to be flushed at end of update
    public void ShowElement(UIElement element) {
        if ((element.flags & UIElementFlags.Shown) != 0) {
            return;
        }
        element.flags |= UIElementFlags.Shown;
        // expect life cycle system in invoke handlers for this
        if (element.parent != null && (element.parent.flags & UIElementFlags.Shown) != 0) {
            foreach (ISystem system in systems) {
                system.OnElementShown(element);
            }
        }
    }

    // todo -- enqueue these to be flushed at end of update
    public void HideElement(UIElement element) {
        if ((element.flags & UIElementFlags.Shown) == 0) {
            return;
        }
        element.flags |= UIElementFlags.Shown;
        // expect life cycle system in invoke handlers for this
        if (element.parent != null && (element.parent.flags & UIElementFlags.Shown) == 0) {
            foreach (ISystem system in systems) {
                system.OnElementHidden(element);
            }
        }
    }

}