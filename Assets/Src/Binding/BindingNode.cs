using Src;

public class BindingNode : IHierarchical {

    public Binding[] bindings;
    public UITemplateContext context;
    public UIElement element;

    public virtual void OnUpdate(SkipTree<BindingNode>.TreeNode[] children) {
        for (int i = 0; i < bindings.Length; i++) {
            if (bindings[i].isEnabled) {
                bindings[i].Execute(element, context);
            }
        }

        if (!element.isEnabled || children == null) return;

        for (int i = 0; i < children.Length; i++) {
            children[i].item.OnUpdate(children[i].children);
        }
    }

    public virtual void Validate() { }

    public int UniqueId => element.id;
    public IHierarchical Element => element;
    public IHierarchical Parent => element.parent;

}