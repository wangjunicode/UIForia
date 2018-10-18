using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src;
using Src.Systems;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Flags]
public enum QueryOptions {

    ScopeToTemplate = 1 << 0,
    IgnoreInactive = 1 << 1,
    IgnoreNonRendered = 1 << 2,
    IgnoreImplicit = 1 << 3,
    IgnoreActive = 1 << 4,

}

[DebuggerDisplay("{ToString()}")]
public class UIElement : IHierarchical, IExpressionContextProvider {

    // todo some of this stuff isn't used often or present for many elements. may make sense to move to dictionaries so we keep things compact

    public readonly int id;
    public string name;
    public UIElementFlags flags; // todo make internal but available for testing
    public UIStyleSet style;

    internal IReadOnlyList<ValueTuple<string, string>> templateAttributes;

    // todo make readonly but assignable via style system

    public UIElement parent;

    protected UIElement() {
        this.id = UIView.NextElementId;
        this.flags = UIElementFlags.Enabled
                     | UIElementFlags.Shown
                     | UIElementFlags.RequiresLayout
                     | UIElementFlags.RequiresRendering;
    }

    // todo -- work on this interface
    public UIElement templateParent;
    public UIElement[] templateChildren;
    public UIElement[] ownChildren;
    public UITemplateContext templateContext;

    public LayoutResult layoutResult { get; internal set; }

    public Vector2 scrollOffset { get; internal set; }

    public ComputedStyle ComputedStyle => style.computedStyle;
    
    public int depth { get; internal set; }
    public int depthIndex { get; internal set; }
    public int siblingIndex { get; internal set; }

    public IInputProvider Input { get; internal set; }

    public bool isShown => (flags & UIElementFlags.SelfAndAncestorShown) == UIElementFlags.SelfAndAncestorShown;

    public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;

    public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

    public bool isEnabled => (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

    public bool isDisabled => (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

    public bool hasDisabledAncestor => (flags & UIElementFlags.AncestorEnabled) == 0;

    public bool isDestroyed => (flags & UIElementFlags.Destroyed) != 0;

    internal bool isPrimitive => (flags & UIElementFlags.Primitive) != 0;

    public virtual void OnCreate() { }

    public virtual void OnReady() { }

    public virtual void OnUpdate() { }

    public virtual void OnEnable() { }

    public virtual void OnDisable() { }

    public virtual void OnPropsChanged() { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroy() { }

    public bool EnableBinding(string propertyName) {
       throw new NotImplementedException();
        //return templateContext.view.bindingSystem.EnableBinding(this, propertyName);
    }

    public bool DisableBinding(string propertyName) {
        return true;
        // return templateContext.view.bindingSystem.DisableBinding(this, propertyName);
    }

    public bool HasBinding(string propertyName) {
        return false;
        //return templateContext.view.bindingSystem.HasBinding(this, propertyName);
    }

    public void SetEnabled(bool active) {
        if (active && isSelfDisabled) {
            templateContext.view.EnableElement(this);
        }
        else if (!active && isSelfEnabled) {
            templateContext.view.DisableElement(this);
        }
    }

    public UIElement FindById(string id) {
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

    public T FindById<T>(string id) where T : UIElement {
        if (isPrimitive || ownChildren == null) {
            return null;
        }

        for (int i = 0; i < ownChildren.Length; i++) {
            if (ownChildren[i].GetAttribute("id") == id) {
                return ownChildren[i] as T;
            }

            UIElement childResult = ownChildren[i].FindByIdTemplateScoped(id);
            if (childResult != null) {
                return childResult as T;
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

    public T FindFirstByType<T>() where T : UIElement {
        if (isPrimitive || ownChildren == null) {
            return null;
        }

        for (int i = 0; i < ownChildren.Length; i++) {
            if (ownChildren[i] is T) {
                return (T) ownChildren[i];
            }

            UIElement childResult = ownChildren[i].FindFirstByTypeTemplateScoped<T>();
            if (childResult != null) {
                return (T) childResult;
            }
        }

        return null;
    }

    protected UIElement FindFirstByTypeTemplateScoped<T>() where T : UIElement {
        if (isPrimitive || templateChildren == null) {
            return null;
        }

        for (int i = 0; i < templateChildren.Length; i++) {
            if (templateChildren[i] is T) {
                return (T) templateChildren[i];
            }

            UIElement childResult = templateChildren[i].FindFirstByTypeTemplateScoped<T>();
            if (childResult != null) {
                return (T) childResult;
            }
        }

        return null;
    }

    public List<T> FindByType<T>(List<T> retn = null) where T : UIElement {
        retn = retn ?? new List<T>();
        if (isPrimitive || ownChildren == null) {
            return retn;
        }

        for (int i = 0; i < ownChildren.Length; i++) {
            if (ownChildren[i] is T) {
                retn.Add((T) ownChildren[i]);
            }

            ownChildren[i].FindByTypeTemplateScoped<T>(retn);
        }

        return retn;
    }

    protected void FindByTypeTemplateScoped<T>(List<T> retn) where T : UIElement {
        if (isPrimitive || ownChildren == null || templateChildren == null) {
            return;
        }

        for (int i = 0; i < templateChildren.Length; i++) {
            if (templateChildren[i] is T) {
                retn.Add((T) templateChildren[i]);
            }

            templateChildren[i].FindByTypeTemplateScoped(retn);
        }
    }

    public override string ToString() {
        string retn = string.Empty;
        if (HasAttribute("id")) {
            retn += "<" + GetType().Name + ":" + GetAttribute("id") + " " + id + ">";
        }
        if (name != null) {
            retn += "<" + name + ":" + GetType().Name + " " + id + ">";
        }
        else if (style != null && style.HasBaseStyles) {
            return "<" + GetType().Name + ": " + style.GetBaseStyleNames() + ">";
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

    public int UniqueId => id;

    IExpressionContextProvider IExpressionContextProvider.ExpressionParent => templateParent;

    public IHierarchical Element => this;
    public IHierarchical Parent => parent;

    public class DepthComparerAscending : IComparer<UIElement> {

        public int Compare(UIElement a, UIElement b) {
            if (a.depth != b.depth) {
                return a.depth < b.depth ? 1 : -1;
            }

            if (a.parent == b.parent) {
                return a.siblingIndex < b.siblingIndex ? 1 : -1;
            }

            if (a.parent == null) return 1;
            if (b.parent == null) return -1;

            return a.parent.siblingIndex < b.parent.siblingIndex ? 1 : -1;
        }

    }

}