using System;
using System.Collections.Generic;
using Rendering;
using Src;
using Src.Layout;
using Src.Systems;
using UnityEngine;

public class UIView {

    public Type templateType;
    public Font font;

    private static int ElementIdGenerator;
    public static int NextElementId => ElementIdGenerator++;

    public UIElement root;
    public GameObject gameObject;

    private RectTransform rectTransform;
    public BindingSystem bindingSystem;
    public RenderSystem renderSystem;
    public LifeCycleSystem lifeCycleSystem;
    
    public UIView(GameObject gameObject) {
        this.gameObject = gameObject;
        bindingSystem = new BindingSystem();
        renderSystem = new RenderSystem(gameObject);
        lifeCycleSystem = new LifeCycleSystem();;
    }

    public void Refresh() {

        bindingSystem.Reset();
        renderSystem.Reset();
        root = TemplateParser.GetParsedTemplate(templateType, true).CreateWithoutScope(this);

    }

    public void Register(RegistrationData elementData) {
        bindingSystem.Register(elementData.element, elementData.bindings, elementData.context);
        lifeCycleSystem.Register(elementData.element);
        renderSystem.Register(elementData.element);
    }

    public void OnCreate() {
        renderSystem.font = font;
        root = TemplateParser.GetParsedTemplate(templateType).CreateWithoutScope(this);
        rectTransform = gameObject.transform as RectTransform;
    }

    public void Update() {
        bindingSystem.Update();
        HandleCreatedElements();
        HandleHidingElements();
        HandleShowingElements();
        HandleDestroyingElements();
        renderSystem.Update();
//        RunLayout();
        HandleMouseEvents();
    }

    private void HandleDestroyingElements() { }

    private void HandleCreatedElements() { }

    private void HandleHidingElements() { }

    private void HandleShowingElements() { }

    private void HandleMouseEvents() {
        List<UIStyleSet> respondsToHover = new List<UIStyleSet>();

    }

    

    private void HandleKeyboardEvents() { }

    private void HandleFocusEvents() { }

    private void RunLayout() {

        Rect viewport = rectTransform.rect; // compute this rect from canvas size offset by view's position on canvas

        Stack<LayoutData> layoutData = new Stack<LayoutData>();

        layoutData.Push(root.style.layout.Run(viewport, null, root));

        float xOffset = 0;
        float yOffset = 0;

        while (layoutData.Count != 0) {
            LayoutData data = layoutData.Pop();

            data.x += xOffset;
            data.y += yOffset;
            // convert to world space
            // apply to unity transforms
            // add / update masks
            // add / update scroll views

            for (int i = 0; i < data.children.Count; i++) {
                layoutData.Push(data.children[i]);
            }
        }

    }

}

public struct MouseHandleShape {

    public Rect rect;
    public float radius;
    public int elementId;
    public bool capture;
    public MouseEventType types;

    public static bool Contains(MouseHandleShape shape, Vector2 point) {
        Vector2 center = shape.rect.center;
        if ((point - center).sqrMagnitude < shape.radius * shape.radius) {
            return false;
        }
        return shape.rect.Contains(point);
    }

}