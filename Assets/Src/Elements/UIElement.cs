using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Src.Rendering;
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

[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public class UIElement : IHierarchical, IExpressionContextProvider {

    // todo some of this stuff isn't used often or present for many elements. may make sense to move to dictionaries so we keep things compact

    public readonly int id;
    public UIElementFlags flags; // todo make internal but available for testing
    public UIStyleSet style;

    // todo make readonly but assignable via style system

    public UIElement parent;
    public UITemplate templateRef;

    protected UIElement() {
        this.id = UIView.NextElementId;
        this.style = new UIStyleSet(this);
        this.flags = UIElementFlags.Enabled
                     | UIElementFlags.Shown
                     | UIElementFlags.RequiresLayout
                     | UIElementFlags.RequiresRendering;
    }

    // todo -- work on this interface
    public UIElement templateParent; // remove or move to cold data
    public UIElement[] children;
    public UITemplateContext templateContext; // move to cold data
    public UIChildrenElement transcludedChildren; // move to cold data

    public LayoutResult layoutResult { get; internal set; }

    private ElementRenderer renderer = ElementRenderer.DefaultInstanced;

    public ElementRenderer Renderer {
        get { return renderer; }
        set {
            if (value == null) {
                value = ElementRenderer.DefaultInstanced;
            }
            else {
                renderer = value;
            }
        }
    }

    public Vector2 scrollOffset { get; internal set; }

    public ComputedStyle ComputedStyle => style.computedStyle;

    public int depth { get; internal set; }
    public int depthIndex { get; internal set; }
    public int siblingIndex { get; internal set; }

    public IInputProvider Input { get; internal set; }

    public int ChildCount => children?.Length ?? 0;

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
    
    public UIElement GetChild(int index) {
        if (children == null || (uint) index >= (uint) children.Length) {
            return null;
        }

        return children[index];
    }

    public UIElement FindById(string id) {
        return FindById<UIElement>(id);
    }

    [PublicAPI]
    public T FindById<T>(string id) where T : UIElement {
        if (isPrimitive || children == null) {
            return null;
        }

        for (int i = 0; i < children.Length; i++) {
            if (children[i].GetAttribute("id") == id) {
                return children[i] as T;
            }

            if (children[i] is UIChildrenElement) {
                continue;
            }

            if (children[i]?.templateRef is UIElementTemplate) {
                continue;
            }

            UIElement childResult = children[i].FindById(id);

            if (childResult != null) {
                return childResult as T;
            }
        }

        return null;
    }

    [PublicAPI]
    public T FindFirstByType<T>() where T : UIElement {
        if (isPrimitive || children == null) {
            return null;
        }

        for (int i = 0; i < children.Length; i++) {
            if (children[i] is T) {
                return (T) children[i];
            }

            if (children[i] is UIChildrenElement) {
                continue;
            }

            if (children[i]?.templateRef is UIElementTemplate) {
                continue;
            }

            UIElement childResult = children[i].FindFirstByType<T>();
            if (childResult != null) {
                return (T) childResult;
            }
        }

        return null;
    }

    public List<T> FindByType<T>(List<T> retn = null) where T : UIElement {
        retn = retn ?? new List<T>();
        if (isPrimitive || children == null) {
            return retn;
        }

        for (int i = 0; i < children.Length; i++) {
            if (children[i] is T) {
                retn.Add((T) children[i]);
            }


            if (children[i] is UIChildrenElement) {
                continue;
            }

            if (children[i]?.templateRef is UIElementTemplate) {
                continue;
            }

            children[i].FindByType<T>(retn);
        }

        return retn;
    }

    public override string ToString() {
        
        if (HasAttribute("id")) {
            return "<" + GetDisplayName() + ":" + GetAttribute("id") + " " + id + ">";
        }

        if (style != null && style.HasBaseStyles) {
            return "<" + GetDisplayName() + "> " + style.BaseStyleNames;
        }
        else {
            return "<" + GetDisplayName() + " " + id + ">";
        }

    }

    public virtual string GetDisplayName() {
        return GetType().Name;
    }

    protected bool HasAttribute(string attr) {
        return GetAttribute(attr) != null;
    }

    protected string GetAttribute(string attr) {
        if (templateRef?.templateAttributes == null) {
            return null;
        }

        List<ValueTuple<string, string>> templateAttributes = templateRef.templateAttributes;
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