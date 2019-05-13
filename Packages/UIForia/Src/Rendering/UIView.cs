using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Application = UIForia.Application;

public class UIViewRootElement : UIElement {

    public UIViewRootElement() {
        flags |= UIElementFlags.BuiltIn;
        flags |= UIElementFlags.ImplicitElement;
        flags |= UIElementFlags.Created;
        flags |= UIElementFlags.Ready;
    }

}

// the idea behind a view is that it is a flat plane that can be oriented in 3d space and show content
public class UIView {

    public event Action<UIElement> onElementCreated;
    public event Action<UIElement> onElementDestroyed;
    public event Action<UIElement> onElementEnabled;
    public event Action<UIElement> onElementDisabled;
    public event Action<IReadOnlyList<UIElement>> onElementsAdded;
    public event Action<UIElement> onElementRemoved;

    private readonly Type m_ElementType;
    private readonly string m_Template;

    public int Depth { get; set; }
    public Rect Viewport { get; set; }
    public UIElement RootElement { get; private set; }
    public float ScaleFactor { get; set; } = 1f;

    internal Matrix4x4 matrix;
    internal Size size;

    internal Vector3 position;
    internal Vector3 scale;
    internal Quaternion rotation;

    public readonly int id;
    public readonly Application Application;
    public readonly string name;
    public RenderTexture renderTexture;

    internal LightList<UIElement> elements;
    internal LightList<UIElement> visibleElements;
    private UIViewRootElement rootElement;

    internal UIView(int id, string name, Application app, Rect rect, int depth, Type elementType, string template = null) {
        this.id = id;
        this.name = name;
        this.Application = app;
        this.Viewport = rect;
        this.Depth = depth;
        this.m_Template = template;
        this.m_ElementType = elementType;
        this.rotation = Quaternion.identity;
        this.scale = Vector3.one;
        this.position = Vector3.zero;
        this.size = new Size(Screen.width, Screen.height);
        this.Viewport = new Rect(position.x, position.y, size.width, size.height);
        this.elements = new LightList<UIElement>(32);
        this.visibleElements = new LightList<UIElement>(32);
        Refresh();
    }

    internal UIView(int id, string name, Application app, Rect rect, int depth) {
        this.id = id;
        this.name = name;
        this.Application = app;
        this.Viewport = rect;
        this.Depth = depth;
        this.rotation = Quaternion.identity;
        this.scale = Vector3.one;
        this.position = Vector3.zero;
        this.size = new Size(Screen.width, Screen.height);
        this.Viewport = new Rect(position.x, position.y, size.width, size.height);
        this.rootElement = new UIViewRootElement();
        this.rootElement.flags |= UIElementFlags.Enabled;
        this.rootElement.flags |= UIElementFlags.AncestorEnabled;
        this.rootElement.View = this;
    }


    public UIElement AddChild(UIElement element) {
        Application.InsertChild(rootElement, element, (uint)rootElement.children.Count);
        return element;
    }

    public bool clipOverflow;
    public bool focusOnMouseDown;

    public void SetZIndex() { }

    public void SetCamera(Camera camera, CameraEvent renderHook) { }

    public void SetRenderTexture(RenderTexture texture) { }

    public void EnableElement(UIElement element) {
        Application.DoEnableElement(element);
    }

    public void DisableElement(UIElement element) {
        Application.DoDisableElement(element);
    }

    public void Refresh() {
        if (m_Template != null) {
            this.RootElement = Application.templateParser.ParseTemplateFromString(m_ElementType, m_Template).Create();
        }
        else {
            this.RootElement = Application.templateParser.GetParsedTemplate(m_ElementType).Create();
        }

        this.RootElement.View = this;
    }

    public void Destroy() { }

    internal void InvokeElementEnabled(UIElement element) {
        onElementEnabled?.Invoke(element);
    }

    internal void InvokeElementDisabled(UIElement element) {
        onElementDisabled?.Invoke(element);
    }

    internal void InvokeElementDestroyed(UIElement element) {
        onElementDestroyed?.Invoke(element);
    }

    public void SetPosition(Vector3 position) {
        Viewport = new Rect(position.x, position.y, Viewport.width, Viewport.height);
    }

    public void SetSize(int width, int height) {
        Viewport = new Rect(Viewport.x, Viewport.y, width, height);
    }

    public UIElement CreateElement<T>() {
        ParsedTemplate parsedTemplate = Application.templateParser.GetParsedTemplate(typeof(T));
        if (parsedTemplate == null) {
            return null;
        }

        try {
            // todo -- shouldn't auto - add to child list
            UIElement element = parsedTemplate.Create();
            rootElement.AddChild(element);
            return element;
        }
        catch {
            throw;
        }
    }

    internal void InvokeAddElements(IReadOnlyList<UIElement> addedElements) {
        onElementsAdded?.Invoke(addedElements);
    }
    
    public void RemoveElement(UIElement current) {
        // todo -- event
    }

}