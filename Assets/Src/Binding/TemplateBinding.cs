using Src;

public class TemplateBinding : ISkipTreeTraversable {

    public int id;
    public readonly UIElement element;
    public readonly Binding[] bindings;
    public readonly UITemplateContext context;

    public TemplateBinding(UIElement element, Binding[] bindings, UITemplateContext context) {
        this.element = element;
        this.bindings = bindings;
        this.context = context;
    }

    public IHierarchical Element => element;
    public IHierarchical Parent => element.parent;

    public void OnParentChanged(TemplateBinding newParent) {
        
    }

    public void OnParentChanged(ISkipTreeTraversable newParent) {
        
    }

    public virtual void OnBeforeTraverse() {
        for (int i = 0; i < bindings.Length; i++) {
            bindings[i].Execute(element, context);
        }
    }

    public virtual void OnAfterTraverse() { }

}

public class RepeatTemplateBinding : TemplateBinding {

    public RepeatTemplateBinding(UIElement element, Binding[] bindings, UITemplateContext context)
        : base(element, bindings, context) {}

    public override void OnBeforeTraverse() {
        context.SetObjectAlias("$something", null);
    }

    public override void OnAfterTraverse() {
        context.RemoveObjectAlias("$something");
    }
    
}