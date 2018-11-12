using UIForia;
using UIForia.Input;

public struct MouseHandlerGroup {

    public readonly UITemplateContext context;
    public readonly MouseEventHandler[] handlers;
    public readonly InputEventType handledEvents;

    public MouseHandlerGroup(UITemplateContext context, MouseEventHandler[] bindings, InputEventType handledEvents) {
        this.context = context;
        this.handlers = bindings;
        this.handledEvents = handledEvents;
    }

}
