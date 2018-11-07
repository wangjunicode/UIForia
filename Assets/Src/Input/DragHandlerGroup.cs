using UIForia;
using UIForia.Input;

public struct DragHandlerGroup {

    public readonly UITemplateContext context;
    public readonly InputEventType handledEvents;
    public readonly DragEventHandler[] handlers;

    public DragHandlerGroup(UITemplateContext context, DragEventHandler[] handlers, InputEventType handledEvents) {
        this.context = context;
        this.handlers = handlers;
        this.handledEvents = handledEvents;
    }

}