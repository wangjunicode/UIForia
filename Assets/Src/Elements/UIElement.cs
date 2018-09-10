using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;
using Src;

[Flags]
public enum QueryOptions {

    ScopeToTemplate = 1 << 0,
    IgnoreInactive = 1 << 1,
    IgnoreNonRendered = 1 << 2,
    IgnoreImplicit = 1 << 3,
    IgnoreActive = 1 << 4,

}

public class TemplateQueryInterface {

    public T FindElementById<T>(string id) where T : UIElement {
        return null;
    }

    public UIElement FindElementById(string id) {
        return null;
    }

    public List<T> FindElementsByType<T>() where T : UIElement {
        return null;
    }

    // Where((s) => {});
    // active, inactive
    // typeof parent == 
    // typeof RenderedParent == {}
    // hasAttribute
    // FindElementsWithAttribute("attr", value?)
    // FindElementOfType()
    // FindElementWithParentOfType()
    // FindElementsWithChildrenOfType()
    // FindElementsWithAncestorOfType()
    // FindElementsWithAncestorWithAttribute();
    // QueryOption.ScopeToTemplate | QueryOption.IgnoreInactive | QueryOption.IgnoreImplicit | QueryOption.IgnoreAttributeCasing

}

[DebuggerDisplay("{GetType()} id={id} {name}")]
public class UIElement : IHierarchical {

    // todo make internal but available for testing
    public UIElementFlags flags;

    // probably better as a list since we never have many of these and list uses less memory
    internal IReadOnlyList<ValueTuple<string, string>> templateAttributes;

    public string name;
    public readonly int id;

    // todo make readonly but assignable via style system
    [UsedImplicitly] public UIStyleSet style;


    // todo make readonly and hide this
    public int depth;  
    
    public UITransform transform;

    protected internal UIElement parent;

    public UIElement() {
        id = UIView.NextElementId;
        transform = new UITransform();
        this.flags = UIElementFlags.Enabled
                     | UIElementFlags.Shown
                     | UIElementFlags.RequiresLayout
                     | UIElementFlags.RequiresRendering;
    }

    internal UITemplateContext templateContext;
    internal UIElement[] templateChildren;
    internal UIElement[] ownChildren;

    public bool isShown => (flags & UIElementFlags.SelfAndAncestorShown) == UIElementFlags.SelfAndAncestorShown;

    public bool isImplicit => (flags & UIElementFlags.ImplicitElement) != 0;

    public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

    public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

    public bool isEnabled => (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

    public bool isDisabled => (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

    public bool hasDisabledAncestor => (flags & UIElementFlags.AncestorEnabled) == 0;

    public bool isDestroyed => (flags & UIElementFlags.Destroyed) != 0;

    internal bool isPrimitive => (flags & UIElementFlags.Primitive) != 0;

    public virtual void OnCreate() { }

    public virtual void OnUpdate() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void OnPropsChanged() { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroy() { }


    public bool EnableBinding(string propertyName) {
        return true;
    }

    public bool DisableBinding(string propertyName) {
        return true;
    }
    
    public bool HasBinding(string propertyName) {
        return false;
    }

    public void SetEnabled(bool active) {
        if (active) {
            templateContext.view.EnableElement(this);
        }
        else {
            templateContext.view.DisableElement(this);
        }
    }
    
    protected UIElement FindById(string id) {
        if (isPrimitive || ownChildren == null) {
            return null;
        }

        for (int i = 0; i < ownChildren.Length; i++) {
            if (ownChildren[i].GetAttribute("id") == id) {
                return ownChildren[i];
            }

            UIElement childResult = ownChildren[i].FindByIdTemplateScoped(id);
            if (childResult != null) {
                return childResult;
            }
        }

        return null;
    }

    private UIElement FindByIdTemplateScoped(string id) {
        if (isPrimitive || templateChildren == null) {
            return null;
        }

        for (int i = 0; i < templateChildren.Length; i++) {
            if (templateChildren[i].GetAttribute("id") == id) {
                return templateChildren[i];
            }

            UIElement childResult = templateChildren[i].FindByIdTemplateScoped(id);
            if (childResult != null) {
                return childResult;
            }
        }

        return null;
    }
    
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

    protected bool HasAttribute(string attr) {
        return GetAttribute(attr) != null;
    }

    protected string GetAttribute(string attr) {
        if (templateAttributes == null) return null;
        for (int i = 0; i < templateAttributes.Count; i++) {
            if (templateAttributes[i].Item1 == attr) {
                return templateAttributes[i].Item2;
            }
        }

        return null;
    }

    // todo -- remove this so we can hide parent from user
    public int UniqueId => id;
    public IHierarchical Element => this;
    public IHierarchical Parent => parent;

}