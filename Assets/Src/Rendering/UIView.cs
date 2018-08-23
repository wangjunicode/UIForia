using System;
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
    protected readonly LayoutSystem layoutSystem;
    protected readonly StyleSystem styleSystem;
    protected readonly ElementRegistrySystem elementSystem;
    
    private readonly Type elementType;

    protected UIView(Type elementType) {
        this.elementType = elementType;
        elementSystem = new ElementRegistrySystem();
        styleSystem = new StyleSystem();
        layoutSystem = new LayoutSystem(styleSystem);
        bindingSystem = new BindingSystem();
        lifeCycleSystem = new LifeCycleSystem();
    }

    public UIElement Root => root;
    
    protected abstract IRenderSystem renderSystem { get; set; }
    
    public abstract void Render();
    
    public virtual void Refresh() {
        bindingSystem.OnReset();
        renderSystem.OnReset();
        lifeCycleSystem.OnReset();
        layoutSystem.OnReset();
        styleSystem.OnReset();
        elementSystem.OnReset(); 
        
        root = TemplateParser.GetParsedTemplate(elementType, true).CreateWithoutScope(this);
        
        layoutSystem.OnInitialize();
        renderSystem.OnInitialize();
        
    }

    // todo -- make this stuff event based to make dependency graph explicit or removed
    public virtual void Register(UIElementCreationData elementData) {
        elementSystem.OnElementCreated(elementData);
        layoutSystem.OnElementCreated(elementData);
        styleSystem.OnElementCreated(elementData);
        lifeCycleSystem.OnElementCreated(elementData);
        renderSystem.OnElementCreated(elementData);
        bindingSystem.OnElementCreated(elementData);
    }

    public virtual void OnCreate() {
        root = TemplateParser.GetParsedTemplate(elementType).CreateWithoutScope(this);
        layoutSystem.OnInitialize();
        renderSystem.OnInitialize();
    }

    public virtual void OnDestroy() {
        lifeCycleSystem.OnDestroy();
        bindingSystem.OnDestroy();
        renderSystem.OnDestroy();
        layoutSystem.OnDestroy();
        styleSystem.OnDestroy();
        elementSystem.OnDestroy();
    }
    
    public virtual void Update() {
        bindingSystem.OnUpdate();
        HandleCreatedElements();
        HandleHidingElements();
        HandleShowingElements();
        HandleDestroyingElements();
        renderSystem.OnUpdate();
        HandleMouseEvents();
    }

    protected virtual void HandleDestroyingElements() { }

    protected virtual void HandleCreatedElements() { }

    protected virtual void HandleHidingElements() { }

    protected virtual void HandleShowingElements() { }

    protected virtual void HandleMouseEvents() { }

    protected virtual void HandleKeyboardEvents() { }

    protected virtual void HandleFocusEvents() { }

}