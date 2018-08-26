using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;
using Src;

[DebuggerDisplay("{name} {GetType()} id={id}")]
public class UIElement : IHierarchical {

    internal UIElementFlags flags;

    public string name;
    public readonly int id;

    [UsedImplicitly] public UIStyleSet style;

    protected internal UIElement parent;

    public UIElement() {
        id = UIView.NextElementId;
        this.flags = UIElementFlags.Enabled
                     | UIElementFlags.Shown
                     | UIElementFlags.RequiresLayout
                     | UIElementFlags.RequiresRendering;
    }

    public virtual void OnCreate() { }

    public virtual void OnUpdate() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void OnPropsChanged() { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroy() { }

    public override string ToString() {
        string retn = string.Empty;
        if (name != null) {
            retn += "<" + name + ":" + GetType().Name + " " + id + ">";
        }
        else {
            retn += "<" + GetType().Name + " " + id + ">";
        }
        return retn;
    }

    public int UniqueId => id;
    public IHierarchical Element => this;
    public IHierarchical Parent => parent;

}