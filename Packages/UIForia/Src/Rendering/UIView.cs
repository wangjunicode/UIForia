using System;
using UIForia.Elements;
using UIForia.Layout;
using UnityEngine;
using Application = UIForia.Application;

// the idea behind a view is that it is a flat plane that can be oriented in 3d space and show content
public class UIView {

    public event Action<UIElement> onElementCreated;
    public event Action<UIElement> onElementDestroyed;
    public event Action<UIElement> onElementEnabled;
    public event Action<UIElement> onElementDisabled;

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

    internal bool is2D;
    
    public readonly int id;
    public readonly Application Application;
    public readonly string name;
    public RenderTexture renderTexture;
    
    internal UIView(int id, string name, Application app, Rect rect, int depth, Type elementType, string template = null) {
        this.id = id;
        this.name = name;
        this.Application = app;
        this.Viewport = rect;
        this.Depth = depth;
        this.m_Template = template;
        this.m_ElementType = elementType;
        this.rotation  = Quaternion.identity;
        this.scale = Vector3.one;
        this.position = Vector3.zero;
        this.size = new Size(Screen.width, Screen.height);
        this.Viewport = new Rect(position.x, position.y, size.width, size.height);
        Refresh();
    }

    public bool clipOverflow;

    public void SetZIndex() {
        
    }

    public void SetCamera(Camera camera) {
        
    }

    public void SetRenderTexture(RenderTexture texture) {
        
    }
    
    public void EnableElement(UIElement element) {
        Application.DoEnableElement(element);
    }

    public void DisableElement(UIElement element) {
        Application.DoDisableElement(element);
    }

    public void Refresh() {
        // todo allow hooks for custom context expressions here
        if (m_Template != null) {
            this.RootElement = Application.templateParser.ParseTemplateFromString(m_ElementType, m_Template).Create();
        }
        else {
            this.RootElement = Application.templateParser.GetParsedTemplate(m_ElementType).Create();
        }

        this.RootElement.View = this;
    }

    public void Destroy() { }

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

    public void SetPosition(Vector3 position) {
        Viewport = new Rect(position.x, position.y, Viewport.width, Viewport.height);
    }

    public void SetSize(int width, int height) {
        Viewport = new Rect(Viewport.x, Viewport.y, width, height);
    }

}