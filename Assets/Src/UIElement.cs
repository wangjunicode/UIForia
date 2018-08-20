using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;
using Src;

[DebuggerDisplay("id={id}")]
public class UIElement : IHierarchical {

    internal UIElementFlags flags;

    public readonly int id;
    public string name;

    // todo -- children probably go away since 'children' will mean different
    // things in different contexts and systems
    //public List<UIElement> children;
    [UsedImplicitly] public UIStyleSet style;

    protected internal UIElement parent;

    public UIElement() {
        id = UIView.NextElementId;
        this.flags = 0;
    }

    public virtual void Initialize() { }

    public virtual void OnPropsChanged() { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroyed() { }

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

    public IHierarchical Element => this;
    public IHierarchical Parent => parent;

}