using UIForia;

public class BindingNode : IHierarchical {

    public Binding[] bindings;
    public UIElement element;
    public ExpressionContext context;

    public virtual void OnUpdate() {
                
        for (int i = 0; i < bindings.Length; i++) {
            bindings[i].Execute(element, context);
        }

    }

    public int UniqueId => element.id;
    public IHierarchical Element => element;
    public IHierarchical Parent => element.parent;


}