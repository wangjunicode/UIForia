using System;
using System.Collections.Generic;
using Rendering;
using Src;
using Src.Systems;

public abstract class UIView {


    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    private UIElement root;

    public readonly BindingSystem bindingSystem;
    public readonly LifeCycleSystem lifeCycleSystem;
    public readonly LayoutSystem layoutSystem;
    
    private Type elementType;

    protected UIView(Type elementType) {
        this.elementType = elementType;
        layoutSystem = new LayoutSystem();
        bindingSystem = new BindingSystem();
        lifeCycleSystem = new LifeCycleSystem();
    }

    public UIElement Root => root;
    
    public abstract IRenderSystem renderSystem { get; protected set; }
    
    public abstract void Render();
    
    public virtual void Refresh() {
        bindingSystem.OnReset();
        renderSystem.OnReset();
        lifeCycleSystem.OnReset();
        layoutSystem.OnReset();
        root = TemplateParser.GetParsedTemplate(elementType, true).CreateWithoutScope(this);
    }

    public virtual void Register(UIElementCreationData elementData) {
        layoutSystem.OnElementCreated(elementData);
        lifeCycleSystem.OnElementCreated(elementData);
        renderSystem.OnElementCreated(elementData);
        bindingSystem.OnElementCreated(elementData);
    }

    public virtual void OnCreate() {
        root = TemplateParser.GetParsedTemplate(elementType).CreateWithoutScope(this);
    }

    public virtual void OnDestroy() {
        lifeCycleSystem.OnDestroy();
        bindingSystem.OnDestroy();
        renderSystem.OnDestroy();
        layoutSystem.OnDestroy();
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