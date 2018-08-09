using System;
using System.Collections.Generic;
using Rendering;
using Src;

public class UIElement : IHierarchical {

    [Flags]
    internal enum UIElementFlags {

        RequiresRendering = 1 << 0
                            
    }
    
    internal UIElementFlags flags;
    public readonly int id;
    public UIElement parent;
    public List<UIElement> children;
    public TemplateContext providedContext;
    public TemplateContext referenceContext;
    public UIStyle style = new UIStyle();
    public UITransform transform;
    public UIView view;

    public UIElement() {
        id = UIView.NextElementId;
        transform = new UITransform(null, null);
    }

    public UIElement(UIView view) {
        id = UIView.NextElementId;
        this.view = view;
    }

    public virtual void Initialize() { }

    public virtual void OnPropsChanged(List<object> props) { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroyed() { }
    
    public override string ToString() {
        string retn = string.Empty;
        retn += "<" + GetType().Name + ">";
        return retn;
    }

    public IHierarchical Parent => parent;

}