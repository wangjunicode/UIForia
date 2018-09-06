using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;
using Src;

[DebuggerDisplay("{GetType()} id={id} {name}")]
public class UIElement : IHierarchical {

    // todo make internal but available for testing
    public UIElementFlags flags;

    public string name;
    public readonly int id;

    // todo make readonly but assignable via style system
    [UsedImplicitly] public UIStyleSet style;

    
    // todo make readonly and hide this
    public int depth;
    
    protected internal UIElement parent;

    public UIElement() {
        id = UIView.NextElementId;
        this.flags = UIElementFlags.Enabled
                     | UIElementFlags.Shown
                     | UIElementFlags.RequiresLayout
                     | UIElementFlags.RequiresRendering;
    }

    public bool isShown => (flags & UIElementFlags.SelfAndAncestorShown) == UIElementFlags.SelfAndAncestorShown;

    public bool isImplicit => (flags & UIElementFlags.ImplicitElement) != 0;

    public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

    public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

    public bool isEnabled => (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

    public bool isDisabled => (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

    public bool hasDisabledAncestor => (flags & UIElementFlags.AncestorEnabled) == 0;
    
    public bool isDestroyed => (flags & UIElementFlags.Destroyed) != 0;
    
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

    // todo -- remove this so we can hide parent from user
    public int UniqueId => id;
    public IHierarchical Element => this;
    public IHierarchical Parent => parent;

}