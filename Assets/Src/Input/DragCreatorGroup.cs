using UIForia;
using UIForia.Input;

public struct DragCreatorGroup {

    public readonly UITemplateContext context;
    public readonly DragEventCreator[] creators;

    public DragCreatorGroup(UITemplateContext context, DragEventCreator[] creators) {
        this.context = context;
        this.creators = creators;
    }

    public DragEvent TryCreateEvent(object target, MouseInputEvent mouseEvent) {
        for (int i = 0; i < creators.Length; i++) {
            DragEvent evt = creators[i].Invoke(target, context, mouseEvent);
            if (evt != null) {
                return evt;
            }
        }

        return null;
    }

}