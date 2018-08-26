using Src;

public class TemplateBinding : IHierarchical {

    public readonly UIElement element;
    public readonly Binding[] bindings;
    public readonly UITemplateContext context;

    public TemplateBinding(UIElement element, Binding[] bindings, UITemplateContext context) {
        this.element = element;
        this.bindings = bindings;
        this.context = context;
    }

    public int UniqueId => element.id;
    public IHierarchical Element => element;
    public IHierarchical Parent => element.parent;

}