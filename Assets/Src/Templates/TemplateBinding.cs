using Src;

public class TemplateBinding : IHierarchical {

    public readonly UIElement element;
    public readonly Binding[] bindings;
    public readonly TemplateContext context;

    public TemplateBinding(UIElement element, Binding[] bindings, TemplateContext context) {
        this.element = element;
        this.bindings = bindings;
        this.context = context;
    }

    public IHierarchical Element => element;
    public IHierarchical Parent => element.parent;

}