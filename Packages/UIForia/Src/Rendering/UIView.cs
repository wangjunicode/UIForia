using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;
using Application = UIForia.Application;

public class UIViewRootElement : UIElement {

    public UIViewRootElement() {
        flags |= UIElementFlags.ImplicitElement;
        flags |= UIElementFlags.Created;
    }

}

// the idea behind a view is that it is a flat plane that can be oriented in 3d space and show content
public class UIView {

    public event Action<UIElement> onElementCreated;
    public event Action<UIElement> onElementReady;
    public event Action<UIElement> onElementRegistered;
    public event Action<UIElement> onElementDestroyed;
    public event Action<UIElement> onElementHierarchyEnabled;
    public event Action<UIElement> onElementHierarchyDisabled;

    private readonly Type m_ElementType;
    private readonly string m_Template;

    public int Depth { get; set; }

    public Rect Viewport { get; set; }

    public UIElement RootElement {
        get {
            return dummyRoot[0];
        }
    }

    public float ScaleFactor { get; set; } = 1f;

    internal Matrix4x4 matrix;

    internal Vector3 position;

    public readonly int id;
    public readonly Application application;
    public readonly string name;
    public RenderTexture renderTexture;

    internal LightList<UIElement> visibleElements;
    internal UIViewRootElement dummyRoot;
    private int elementCount;

    public bool focusOnMouseDown;
    public bool sizeChanged;
    
    internal UIView(Application application, string name, UIElement element, Matrix4x4 matrix, Size size) {
        this.name = name;
        this.application = application;
        this.matrix = matrix;
        this.Viewport = new Rect(0, 0, size.width, size.height);
        this.visibleElements = new LightList<UIElement>(32);
        this.dummyRoot = new UIViewRootElement();
        this.dummyRoot.flags |= UIElementFlags.EnabledFlagSet;
        this.dummyRoot.style = new UIStyleSet(dummyRoot);
        this.dummyRoot.layoutResult = new LayoutResult(dummyRoot);
        this.dummyRoot.View = this;
        this.dummyRoot.children = new LightList<UIElement>(1);
        this.dummyRoot.AddChild(element);
        this.sizeChanged = true;
    }
    
//    internal UIView(int id, string name, Application app, Rect viewportRect, int depth, Type elementType, string template = null) {
//        this.id = id;
//        this.name = name;
//        this.application = app;
//        this.Viewport = viewportRect;
//        this.Depth = depth;
//        this.m_Template = template;
//        this.m_ElementType = elementType;
//        this.position = viewportRect.position;
//        this.visibleElements = new LightList<UIElement>(32);
//        this.dummyRoot = new UIViewRootElement();
//        this.dummyRoot.flags |= UIElementFlags.Enabled;
//        this.dummyRoot.flags |= UIElementFlags.AncestorEnabled;
//        this.dummyRoot.View = this;
//        this.sizeChanged = true;
//    }

//    internal UIView(Application application) {
//        this.id = 0;
//        this.name = "Default";
//        this.application = application;
//        this.visibleElements = new LightList<UIElement>(32);
//        this.dummyRoot = (UIViewRootElement) application.CreateElementFromPoolWithType(typeof(UIViewRootElement), null, 0, 0, 0);
//        this.dummyRoot.flags |= UIElementFlags.Enabled;
//        this.dummyRoot.flags |= UIElementFlags.AncestorEnabled;
//        this.dummyRoot.View = this;
//        this.sizeChanged = true;
//    }
//
//    public UIElement AddChild(UIElement element) {
//        application.InsertChild(dummyRoot, element, (uint) dummyRoot.children.Count);
//        return element;
//    }
//
//    public int GetElementCount() {
//        return elementCount;
//    }

    public void Destroy() {
        application.RemoveView(this);
    }

    internal void ElementRegistered(UIElement element) {
        elementCount++;
        onElementRegistered?.Invoke(element);
    }

    internal void ElementCreated(UIElement element) {
        onElementCreated?.Invoke(element);
    }

    internal void ElementDestroyed(UIElement element) {
        elementCount--;
        onElementDestroyed?.Invoke(element);
    }

    internal void ElementReady(UIElement element) {
        onElementReady?.Invoke(element);
    }

    internal void ElementHierarchyEnabled(UIElement element) {
        onElementHierarchyEnabled?.Invoke(element);
    }

    internal void ElementHierarchyDisabled(UIElement element) {
        onElementHierarchyDisabled?.Invoke(element);
    }

    public void SetPosition(Vector2 position) {
        if (position != Viewport.position) {
            sizeChanged = true;
        }

        Viewport = new Rect(position.x, position.y, Viewport.width, Viewport.height);
    }

    public void SetSize(int width, int height) {
        if (width != Viewport.width || height != Viewport.height) {
            sizeChanged = true;
        }

        Viewport = new Rect(Viewport.x, Viewport.y, width, height);
    }
    
    
    /// <returns>true in case the depth has been changed in order to get focus</returns>
    public bool RequestFocus() {
        var views = application.GetViews();
        if (focusOnMouseDown && Depth < views.Length - 1) {
            for (var index = 0; index < views.Length; index++) {
                UIView view = views[index];
                if (view.Depth > Depth) {
                    view.Depth--;
                }
            }

            Depth = views.Length - 1;
            application.SortViews();
            return true;
        }

        return false;
    }

}