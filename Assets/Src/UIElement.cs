using System;
using System.Collections.Generic;
using Rendering;
using Src;

public class UIElement : IHierarchical {

    [Flags]
    internal enum UIElementFlags {

        RequiresRendering = 1 << 0,
        HasRenderedElement = 1 << 1,
        RenderElementCreated = 1 << 2,
                            
    }
    
    internal UIElementFlags flags;
    public readonly int id;
    public UIElement parent;
    public List<UIElement> children;
    public UIStyle style = new UIStyle();
    public UITransform transform;

    public UIElement() {
        id = UIView.NextElementId;
        transform = new UITransform(null, null);
    }

    public virtual void Initialize() { }

    public virtual void OnPropsChanged() { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroyed() { }
    
    public override string ToString() {
        string retn = string.Empty;
        retn += "<" + GetType().Name + ">";
        return retn;
    }

    public IHierarchical Element => this;
    public IHierarchical Parent => parent;
    
    public bool isEnabled { get; set; }
    public bool HasChildren => children != null && children.Count > 0;

    public static void Traverse(UIElement root, Action<UIElement> action) {
        Stack<UIElement> stack = new Stack<UIElement>();
        stack.Push(root);
        
        while (stack.Count > 0) {
            UIElement element = stack.Pop();
            
            action(element);

            if (!element.HasChildren) continue;
            
            for (int i = 0; i < element.children.Count; i++) {
                stack.Push(element.children[i]);
            }
        }
    }

}